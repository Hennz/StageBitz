using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace StageBitz.Service
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

//            config.Routes.MapHttpRoute(
//name: "DefaultActionNameApi2",
//routeTemplate: "api/{controller}/{action}",
//defaults: new { action = "get", id = RouteParameter.Optional }
//);
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            config.Routes.MapHttpRoute(
                name: "DefaultActionNameApi",
                routeTemplate: "api/{controller}/{action}",
                defaults: new { action = "get" }
            );

            
  //          config.Routes.MapHttpRoute("ActionApi",
  //"api/{controller}/{action}",
  //null,
  //new { action = @"[a-zA-Z]+" });

  //          config.Routes.MapHttpRoute(
  //              name: "SecndApi",
  //              routeTemplate: "api/{controller}/{action}/{id}",
  //              defaults: new { id = RouteParameter.Optional }
  //          );

            // Uncomment the following line of code to enable query support for actions with an IQueryable or IQueryable<T> return type.
            // To avoid processing unexpected or malicious queries, use the validation settings on QueryableAttribute to validate incoming queries.
            // For more information, visit http://go.microsoft.com/fwlink/?LinkId=279712.
            //config.EnableQuerySupport();

            // To disable tracing in your application, please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            //config.EnableSystemDiagnosticsTracing();
        }
    }
}
