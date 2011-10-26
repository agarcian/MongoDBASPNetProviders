using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebUI4.Areas.IPC.Controllers
{
    public class ViewerController : Controller
    {
        //
        // GET: /IPCViewer/Viewer/

        public ActionResult Index()
        {
            return RedirectToAction("Search");
        }


        public ActionResult Catalog(string id)
        {
            return View();
        }

        [HttpGet]
        public ActionResult Search()
        {
            return View();
        }



    }
}
