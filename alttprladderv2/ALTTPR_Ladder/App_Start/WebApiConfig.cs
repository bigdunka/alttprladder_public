using System.Web.Http;

namespace ALTTPR_Ladder { 
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "LadderAPI",
                routeTemplate: "api/v1/LadderAPI/{action}/{id}",
                defaults: new { Controller = "LadderAPI", id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "PublicAPI",
                routeTemplate: "api/v1/PublicAPI/{action}/{id}",
                defaults: new { Controller = "PublicAPI", id = RouteParameter.Optional }
            );
        }
    }
}

