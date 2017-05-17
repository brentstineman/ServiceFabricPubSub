using System.Web.Http;
using Owin;
using Swashbuckle.Swagger;
using Swashbuckle.Application;

namespace Administration
{
    public static class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public static void ConfigureApp(IAppBuilder appBuilder)
        {
            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();

            config.EnableSwagger(c => c.SingleApiVersion("v2", "PubSubClientApi"))
                 .EnableSwaggerUi();

            config.Formatters.Clear();
            config.Formatters.Add(new System.Net.Http.Formatting.JsonMediaTypeFormatter());

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional  
                }
         );

            config.Formatters.Clear();
            config.Formatters.Add(new System.Net.Http.Formatting.JsonMediaTypeFormatter());

            appBuilder.UseWebApi(config);
        }
    }
}
