using System.Web.Http;
using Owin;
using Swashbuckle.Swagger;
using Swashbuckle.Application;

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

            config.EnableSwagger(c => c.SingleApiVersion("v1", "PubSubClientApi"))
                 .EnableSwaggerUi();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{tenantId}/{topicName}",
                defaults: new { tenantId = RouteParameter.Optional, topicName = RouteParameter.Optional }
            );

            config.Formatters.Clear();
            config.Formatters.Add(new System.Net.Http.Formatting.JsonMediaTypeFormatter());

            appBuilder.UseWebApi(config);
        }
    }
}
