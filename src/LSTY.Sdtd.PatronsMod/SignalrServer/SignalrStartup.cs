using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Owin;

namespace LSTY.Sdtd.PatronsMod.SignalR
{
    public class SignalrStartup
    {
        public void Configuration(IAppBuilder app)
        {
            app.MapSignalR(new HubConfiguration()
            {
                // 启用 JSONPJSONP 请求是不安全的，但一些较旧的浏览器（和一些 IE 版本）需要 JSONP 才能跨域工作
                EnableJSONP = false,
#if DEBUG
                EnableDetailedErrors = true,
#endif
                EnableJavaScriptProxies = false
            });

            GlobalHost.HubPipeline.AddModule(new GlobalExceptionModule());

            if (string.IsNullOrEmpty(ModApi.AppSettings.AccessToken) == false)
            {
                GlobalHost.HubPipeline.AddModule(new AuthorizeModule(new ApiKeyAuthorizeHubConnection(), null));
            }

            GlobalHost.Configuration.MaxIncomingWebSocketMessageSize = null;
        }
    }
}