using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.Common.MVC;
using AltovientoSolutions.Security;


namespace WebUI4.Areas.IPC.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /IPCViewer/Home/

        public ActionResult Index()
        {
            return RedirectToActionPermanent("Index", "Project");
        }




    }
}
