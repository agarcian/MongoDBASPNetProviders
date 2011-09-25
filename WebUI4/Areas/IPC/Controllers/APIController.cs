using System;
using System.Drawing;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.Common.MVC;
using AltovientoSolutions.DAL.IPC;
using AltovientoSolutions.DAL.IPC.Model;
using Newtonsoft.Json;

namespace WebUI4.Areas.IPC.Controllers
{
    public class APIController : Controller
    {
        //
        // GET: /IPCViewer/API/

        public ActionResult Index()
        {
            return View();
        }

        //
        // GET: /IPCViewer/API/Catalog
        [HttpGet]
        [CompressFilter]
        public ActionResult Catalog(string id, string LangCode)
        {
            // To Do: This should be enclosed in a server cache management so it resides in memory.


            if (String.IsNullOrWhiteSpace(id))
                throw new HttpException(404, "Resource not available");


            IPCMediatorMongoDB db = new IPCMediatorMongoDB("space_00010");

            Catalog catalog = db.GetCatalog("space_00010", id, LangCode);

            if (catalog == null)
            {
                Response.StatusCode = 404;
                return new ContentResult() { Content = "Resource not available" };
            }

            string content = JsonConvert.SerializeObject(catalog);


            // Generate a navigation map.
            List<NavigationPosition> navigationMap = new List<NavigationPosition>();

            foreach (Chapter chapter in catalog.Chapter)
            {
                foreach (Page page in chapter.Page)
                {
                    navigationMap.Add(new NavigationPosition() { ChapterId = chapter.ID, PageId = page.ID });
                }
            }

            // Calculate metadata
            if ("ar,hb".Contains(catalog.LanguageCode))
            {
                catalog.IpcMetadata.RTL = true;
            }

            var result = new ContentResult
            {
                Content = content,
                ContentType = "application/json"
            };

            return result;
        }

        [HttpGet]
        [CompressFilter]
        public ActionResult Illustration(string id, string width, string height)
        {
            int nWidth = 800;
            int nHeight = 600;

            Int32.TryParse(width, out nWidth);
            Int32.TryParse(height, out nHeight);

            List<Callout> callouts = new List<Callout>();

            Bitmap bitmap = IllustrationIpsum.GenerateRandomImage(nWidth, nHeight, 10, true, out callouts);

            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                
                ms.Seek(0, SeekOrigin.Begin);
                byte[] buffer = ms.ToArray();

                FileContentResult result = new FileContentResult(buffer, "image/png");
                result.FileDownloadName = id+ ".png";
                return result;
            }

        }

    }
}
