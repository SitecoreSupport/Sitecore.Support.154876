using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using Facebook;
using Newtonsoft.Json.Linq;
using Sitecore.Social.Facebook.Connector.Managers;
using Sitecore.Social.Facebook.Connector.Paths;
using Sitecore.Social.Facebook.Exceptions.Analyzers;
using Sitecore.Social.Infrastructure.Utils;
using Sitecore.Social.NetworkProviders.Args;
using Sitecore.Social.NetworkProviders.Connector.Paths;
using Sitecore.Social.NetworkProviders.Interfaces;
using Sitecore.Web;

namespace Sitecore.Support.Social.Facebook.Networks.Providers
{
  public class FacebookProvider : Sitecore.Social.Facebook.Networks.Providers.FacebookProvider, IAuth
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
          WebRequestManager manager =
            new WebRequestManager(string.Format(CultureInfo.CurrentCulture,
              FacebookPathsFactory.Facebook.API.AccessToken +
              "?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}",
              new object[] {args.Application.ApplicationKey, fullUrl, args.Application.ApplicationSecret, str2}));
          string response = manager.GetResponse();
          if (response != null)
          {
            JToken token;
            JToken token2;
            JObject obj2 = JObject.Parse(response);
            if (!obj2.TryGetValue("access_token", out token))
            {
              throw new FacebookOAuthException("Access token was not present in the response.");
            }
            DateTime? nullable = null;
            DateTime? nullable2 = null;
            if (obj2.TryGetValue("expires_in", out token2))
            {
              nullable = new DateTime?(DateTime.UtcNow);
              nullable2 = new DateTime?(nullable.Value.AddSeconds((double) token2.Value<int>()));
            }
            AuthCompletedArgs authCompletedArgs = new AuthCompletedArgs
            {
              Application = args.Application,
              AccessTokenSecret = token.Value<string>(),
              CallbackPage = args.CallbackUrl,
              ExternalData = args.ExternalData,
              AttachAccountToLoggedInUser = args.AttachAccountToLoggedInUser,
              IsAsyncProfileUpdate = args.IsAsyncProfileUpdate,
              AccessTokenSecretIssueDate = nullable,
              AccessTokenSecretExpirationDate = nullable2
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
  }
}