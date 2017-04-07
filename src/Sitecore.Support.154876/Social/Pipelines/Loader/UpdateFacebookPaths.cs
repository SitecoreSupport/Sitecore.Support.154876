using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using Sitecore.Social.Facebook.Connector.Paths;
using Sitecore.Social.NetworkProviders.Connector.Paths;

namespace Sitecore.Support.Social.Pipelines.Loader
{
  public class UpdateFacebookPaths
  {
    public void Process(PipelineArgs args)
    {
      var facebook = new FacebookPaths("http://www.facebook.com/home.php", new SocialPaths.CommandsPaths("type=facebook_auth", "type=facebook_access", "type=facebook_connect", "type=facebook_add", "type=facebook_remove"), new FacebookPaths.Links("https://www.facebook.com/v2.5/dialog/oauth", "https://graph.facebook.com/v2.5/oauth/access_token", "/v2.5/{0}/accounts", "/v2.5/{0}/feed", "/v2.5/me", "/v2.5/{0}", "/v2.5/{0}/comments", "v2.5", "/v2.5/me/ids_for_business"));
      typeof(FacebookPathsFactory).GetField("Facebook").SetValue(null, facebook);
      Log.Info("DEBUG 154876:"+FacebookPathsFactory.Facebook.API.AccessToken, this);
    }
  }
}
