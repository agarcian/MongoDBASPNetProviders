using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.DAL.IPC;
using AltovientoSolutions.DAL.IPC.Model;

namespace WebUI4.Areas.IPC.Controllers
{
    public class PublisherController : Controller
    {
        //
        // GET: /IPC/Publisher/

        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Generate()
        {


            return View();
        }

        [HttpPost]
        public ActionResult Generate(string ID, string NoOfChapters, string MaxNoOfPages, string MaxNoOfEntries, string[] Language)
        {

            int nChapters = 0;
            int nPages = 0;
            int nEntries = 0;

            Int32.TryParse(NoOfChapters, out nChapters);
            Int32.TryParse(MaxNoOfPages, out nPages);
            Int32.TryParse(MaxNoOfEntries, out nEntries);
            Dictionary<string, byte[]> Illustrations;
            if (!String.IsNullOrWhiteSpace(ID))
            {
                Dictionary<String, Catalog> catalogs = LoremIpsum.GenerateSampleCatalog(ID, nChapters, nPages, nEntries, Language, out Illustrations);


                // commit the dummy data to db.

                IPCMediatorMongoDB db = new IPCMediatorMongoDB("space_00010");

                foreach (KeyValuePair<String, Catalog> pair in catalogs)
                {
                    string langCode = pair.Key;
                    Catalog catalog = pair.Value;
                    db.SaveCatalog(catalog,catalog.ID, langCode, true);
                }

                foreach (KeyValuePair<String, byte[]> pair in Illustrations)
                {
                    string md5 = pair.Key;
                    byte[] buffer = pair.Value;
                    db.SaveIllustration(buffer,md5, md5);
                }

            }

            return View();
        }
    }
}
