namespace Chat
{
  using System.IO;
  using System.Net;
  using System.Reflection;

  using Funq;

  using ServiceStack;
  using ServiceStack.Auth;
  using ServiceStack.Configuration;
  using ServiceStack.Mvc;

  public class AppHost : AppHostBase
  {
    public AppHost() : base("Chat", typeof(ServerEventsServices).GetTypeInfo().Assembly)
    {
      var liveSettings = this.MapProjectPath("~/appsettings.txt");
      this.AppSettings = File.Exists(liveSettings)
                      ? (IAppSettings)new TextFileSettings(liveSettings)
                      : new AppSettings();
    }

    public override void Configure(Container container)
    {
      this.Plugins.Add(new RazorFormat());
      this.Plugins.Add(new ServerEventsFeature());

      this.SetConfig(new HostConfig
                  {
                    DefaultContentType = MimeTypes.Json,
                    AllowSessionIdsInHttpParams = true,
                  });

      this.CustomErrorHttpHandlers.Remove(HttpStatusCode.Forbidden);

      //Register all Authentication methods you want to enable for this web app.            
      this.Plugins.Add(new AuthFeature(
        () => new AuthUserSession(),
        new IAuthProvider[] {
                              new TwitterAuthProvider(this.AppSettings),   //Sign-in with Twitter
                              new FacebookAuthProvider(this.AppSettings),  //Sign-in with Facebook
                              new GithubAuthProvider(this.AppSettings),    //Sign-in with GitHub
                            }));

      container.RegisterAutoWiredAs<MemoryChatHistory, IChatHistory>();

      // for lte IE 9 support
      this.Plugins.Add(new CorsFeature());
    }
  }
}