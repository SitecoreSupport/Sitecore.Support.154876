using Newtonsoft.Json.Linq;
using Sitecore.Social.Facebook.Connector.Managers;
using Sitecore.Social.Facebook.Connector.Paths;
using Sitecore.Social.Facebook.Exceptions.Analyzers;
using Sitecore.Social.NetworkProviders;
using Sitecore.Social.NetworkProviders.Args;
using Sitecore.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Web;
using Facebook;
using Sitecore.Social.NetworkProviders.Interfaces;
using Sitecore.Diagnostics;
using Sitecore.Social.NetworkProviders.Exceptions;
using Sitecore.Social.Infrastructure.Exceptions;

namespace Sitecore.Support.Social.Facebook.Networks.Providers
{
  public class FacebookProvider : Sitecore.Social.Facebook.Networks.Providers.FacebookProvider, IAuth, IGetAccountInfo
  {
    public FacebookProvider(Sitecore.Social.NetworkProviders.Application application) : base(application)
    {
    }

    void IAuth.AuthGetAccessToken(AuthArgs args)
    {
      HttpRequest request = HttpContext.Current.Request;
      if (string.IsNullOrEmpty(request.QueryString.Get("error")))
      {
        string str2 = request.QueryString.Get("code");
        if (!string.IsNullOrEmpty(str2))
        {
          string fullUrl = WebUtil.GetFullUrl("/layouts/system/Social/Connector/SocialLogin.ashx?type=access");
          WebRequestManager manager = new WebRequestManager(string.Format(CultureInfo.CurrentCulture, FacebookPathsFactory.Facebook.API.AccessToken + "?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}", new object[] { args.Application.ApplicationKey, fullUrl, args.Application.ApplicationSecret, str2 }));
          string response = manager.GetResponse();
          if (response != null)
          {
            JToken token = null;
            JObject objJsonResponse = JObject.Parse(response);

            if (!objJsonResponse.TryGetValue("access_token", out token))
            {
              throw new FacebookOAuthException("Sitecore.Support.154876 - Access token was not present in the response");
            }
            AuthCompletedArgs authCompletedArgs = new AuthCompletedArgs
            {
              Application = args.Application,
              AccessTokenSecret = token.Value<string>(),
              CallbackPage = args.CallbackPage,
              ExternalData = args.ExternalData,
              AttachAccountToLoggedInUser = args.AttachAccountToLoggedInUser,
              IsAsyncProfileUpdate = args.IsAsyncProfileUpdate
            };
            if (!string.IsNullOrEmpty(args.CallbackType))
            {
              base.InvokeAuthCompleted(args.CallbackType, authCompletedArgs);
            }
          }
          else
          {
            new FacebookExceptionAnalyzer().Analyze(manager.ErrorText);
          }
        }
      }
    }


    AccountBasicData IGetAccountInfo.GetAccountBasicData(Account account)
    {
      Assert.IsNotNull(account, "Account parameter is null");
      List<string> fields = new List<string> {
        "first_name",
        "last_name",
        "id",
        "email"
    };
      IDictionary<string, object> dictionary = this.GetAccountData(account, "/me", fields);
      if (dictionary != null)
      {
        string str = dictionary["first_name"] + " " + dictionary["last_name"];
        string email = "";
        if (dictionary.Keys.Contains("email"))
        {
          email = dictionary["email"] as string;
        }
        return new AccountBasicData
        {
          Account = account,
          Id = dictionary["id"] as string,
          Email = email,
          FullName = str
        };
      }
      return null;
    }

    private IDictionary<string, object> GetAccountData(Account account, string access, IEnumerable<string> fields) =>
        this.FacebookRequest(account, access, null, (Func<FacebookClient, string, object, IDictionary<string, object>>)((facebookClient, feedPath, inputParams) => (facebookClient.Get(feedPath, new Dictionary<string, object> { {
        "fields",
        string.Join(",", fields)
    } }) as IDictionary<string, object>)));

    private IDictionary<string, object> FacebookRequest(Account account, string feedPath, object inputParams, Func<FacebookClient, string, object, IDictionary<string, object>> action)
    {
      IDictionary<string, object> dictionary;
      try
      {
        FacebookClient client = new FacebookClient(account.AccessTokenSecret);
        dictionary = action(client, feedPath, inputParams);
      }
      catch (FacebookApiLimitException exception)
      {
        throw new AuthException(exception);
      }
      catch (FacebookOAuthException exception2)
      {
        throw new AuthException(exception2);
      }
      catch (FacebookApiException exception3)
      {
        throw new SocialException(exception3);
      }
      catch (Exception exception4)
      {
        throw new SocialException(exception4);
      }
      return dictionary;
    }
  }
}
