using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebUI4.Areas.IPC.Controllers
{
    public class ProjectController : Controller
    {
        //
        // GET: /IPCViewer/Project/

        public ActionResult Index()
        {
            return View();
        }


        public ActionResult GetStarted()
        {
            return View();
        }

    }
}
