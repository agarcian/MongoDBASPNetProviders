using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.Common.MVC;

namespace WebUI4.Areas.Mariachi.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Mariachi/Home/
        [CompressFilter]
        public ActionResult Index()
        {
            return RedirectToAction("Index", "Website");
        }

    }
}
