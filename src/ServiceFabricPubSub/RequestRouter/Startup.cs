using System.Web.Http;
using Owin;

namespace RequestRouterService
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{tenantId}/{topicName}/{subscriberName}",
                defaults: new { tenantId = RouteParameter.Optional, topicName = RouteParameter.Optional, subscriberName = RouteParameter.Optional }
            );

            appBuilder.UseWebApi(config);
        }
    }
}
