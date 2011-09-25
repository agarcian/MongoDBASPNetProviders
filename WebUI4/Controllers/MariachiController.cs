using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebUI4.Controllers
{
    public class MariachiController : Controller
    {
        //protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        //{
        //    base.Initialize(requestContext);
        //    TempDataProvider = new Microsoft.Web.Mvc.CookieTempDataProvider(requestContext.HttpContext);
        //}

        //
        // GET: /Mariacheros/

        public ActionResult Index()
        {
            return View();
        }

    }
}
