using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebUI4.Areas.Mariachi.Controllers
{
    public class APIController : Controller
    {
        //
        // GET: /Mariachi/API/

        public ActionResult Index()
        {
            return View();
        }


        //
        // GET: /Mariachi/API/Lyrics

        public ActionResult Lyrics()
        {
            // Returns the list of all lyrics
            return View();
        }

    }
}
