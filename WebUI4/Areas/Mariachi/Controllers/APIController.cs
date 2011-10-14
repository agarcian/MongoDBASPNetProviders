using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.DAL.Mariacheros;


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


        public ActionResult SitenameExists(string id)
        {
            bool sitenameExists = false;

            if (String.IsNullOrWhiteSpace(id))
            {
                sitenameExists = false;

            }
            else
            {

                List<String> reservedNames = new List<string>();
                reservedNames.AddRange(new string[] { "mariachi", "mariachero", "mariachera" });

                if (reservedNames.Contains(id.ToLower()))
                {
                    sitenameExists = true;
                }
                else
                {

                    MariachiMediator mediator = new MariachiMediator("Bands");
                    sitenameExists = mediator.DoesBandExist(id);
                }
            }

            var result = new {result= sitenameExists};
            
            return Json(result, JsonRequestBehavior.AllowGet);
        }





    }
}
