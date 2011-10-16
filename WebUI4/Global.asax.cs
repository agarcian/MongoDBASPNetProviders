using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace WebUI4
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.IgnoreRoute("{*content}", new { content = @"(.*/)?content(/.*)?" });
            routes.IgnoreRoute("{*allaspx}", new { allaspx = @".*\.aspx(/.*)?" });
            routes.IgnoreRoute("{*favicon}", new { favicon = @"(.*/)?favicon.ico(/.*)?" });


           
            //routes.MapRoute(
            //    "Default", // Route name
            //    "{controller}/{action}/{id}", // URL with parameters
            //    new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new string[] {"WebUI4.Controllers"} // Parameter defaults
            //);

            // In order to support custom urls for users, we need to explicitely indicate which routes are allowed.
            // Everything else will fall into the profile format.

            routes.MapRoute(
                "Default", // Route name
                "", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new string[] { "WebUI4.Controllers" } // Parameter defaults
            );


            routes.MapRoute(
               "Account", // Route name
               "Account/{action}/{id}", // URL with parameters
               new { controller = "Account", action = "Index", id = UrlParameter.Optional }, new string[] { "WebUI4.Controllers" } // Parameter defaults
           );

            routes.MapRoute(
               "Home", // Route name
               "Home/{action}/{id}", // URL with parameters
               new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new string[] { "WebUI4.Controllers" } // Parameter defaults
           );

            routes.MapRoute(
               "Spaces", // Route name
               "Spaces/{action}/{id}", // URL with parameters
               new { controller = "Spaces", action = "Index", id = UrlParameter.Optional }, new string[] { "WebUI4.Controllers" } // Parameter defaults
           );

            routes.MapRoute(
               "Referrals", // Route name
               "Referrals/{action}/{id}", // URL with parameters
               new { controller = "Referrals", action = "Index", id = UrlParameter.Optional }, new string[] { "WebUI4.Controllers" } // Parameter defaults
           );

            // On any Area or controller that is not matched, will fall through to treat the first element in the url as the profile.
            var route = routes.MapRoute(
                "Website",
            "{user}/{action}/{id}",
            new { controller = "Website", action = "Profile", id = UrlParameter.Optional }, new string[] { "WebUI4.Areas.Mariachi.Controllers" }
            );

            route.DataTokens["area"] = "Mariachi";


        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);
        }


        void Application_PreRequestHandlerExecute(object sender, EventArgs e)
        {
            if (System.Web.HttpContext.Current.Session != null)
                SetCulture();
        }



        private void SetCulture()
        {

            // Get the current culture.
            System.Globalization.CultureInfo culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Globalization.CultureInfo uiCulture = System.Threading.Thread.CurrentThread.CurrentUICulture;


            string neutralCultureFromUrl = Request.Url.Host.Split('.')[0].ToLower();
            // Expect to get a the characters es from es.mywebsite.com.

            if (String.Compare(culture.TwoLetterISOLanguageName, neutralCultureFromUrl, true) != 0)
            {
                switch (neutralCultureFromUrl)
                {
                    case "es":
                        culture = new System.Globalization.CultureInfo("es");
                        uiCulture = new System.Globalization.CultureInfo("es");
                        break;
                    default:
                        //culture = new System.Globalization.CultureInfo("en-US");
                        //uiCulture = new System.Globalization.CultureInfo("en-US");
                        break;
                }
            }

            // set up the thread's culture before the handler executes
            System.Threading.Thread.CurrentThread.CurrentUICulture = uiCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
        }


        public void Application_Error(object sender, EventArgs e)
        {
            Response.Filter.Dispose();
        }
    
    }
}