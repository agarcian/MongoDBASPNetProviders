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

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional }, new string[] {"WebUI4.Controllers"} // Parameter defaults
            );

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