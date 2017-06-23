using System;
using System.Web.Http;
using Newtonsoft.Json;

namespace TestImpactAnalyzer.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            config.Formatters.Remove(config.Formatters.XmlFormatter);
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            json.SerializerSettings.Formatting = Formatting.Indented;
            json.SerializerSettings.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}
