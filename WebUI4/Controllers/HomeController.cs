using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Profile;
using AltovientoSolutions.Common.MVC;

namespace WebUI4.Controllers
{
    public class HomeController : Controller
    {

        //protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        //{
        //    base.Initialize(requestContext);
        //    TempDataProvider = new Microsoft.Web.Mvc.CookieTempDataProvider(requestContext.HttpContext);
        //}

        [CompressFilter]
        public ActionResult Index()
        {
            //string nameInProfile = WebUI4.Models.AccountHelper.GetProfileName(Models.AccountHelper.ProfileNameFormat.FirstNameOnly);

            return RedirectPermanent(ConfigurationManager.AppSettings["StartUrl"]);
        }
    }
}
