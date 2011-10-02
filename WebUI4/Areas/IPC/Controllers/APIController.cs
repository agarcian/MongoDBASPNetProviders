using System;
using System.Drawing;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;
using System.Web.Mvc;
using AltovientoSolutions.Common.MVC;
using AltovientoSolutions.DAL.IPC;
using AltovientoSolutions.DAL.IPC.Model;
using Newtonsoft.Json;

namespace WebUI4.Areas.IPC.Controllers
{
    public class APIController : Controller
    {
        private static bool CACHE_ENABLED = true;
        
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
        [CacheFilter(Cacheability=HttpCacheability.Public, Duration=3600)]
        public ActionResult Catalog(string id, string LangCode, string ignoreCache)
        {
            // To Do: This should be enclosed in a server cache management so it resides in memory.

            if (String.IsNullOrWhiteSpace(id))
                throw new HttpException(404, "Resource not available");

            Catalog catalog = LoadCatalog(id, LangCode, ignoreCache);

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

        private Catalog LoadCatalog(string id, string LangCode, string ignoreCache)
        {
            // Implement Server side caching...   Ideally Memcached.
            Cache cache = this.HttpContext.Cache;

            string cacheKey = String.Format("CATALOG_###_{0}_###_{1}", id, LangCode);

            Catalog catalog = (Catalog)cache.Get(cacheKey);
            if (catalog == null || !String.IsNullOrWhiteSpace(ignoreCache) || !CACHE_ENABLED)
            {
                this.HttpContext.Trace.Write("Caching", "Cache miss for key: " + cacheKey);
                IPCMediatorMongoDB db = new IPCMediatorMongoDB("space_00010");
                catalog = db.GetCatalog(id, LangCode);

                cache.Add(cacheKey, catalog, null, Cache.NoAbsoluteExpiration, new TimeSpan(1, 0, 0), CacheItemPriority.Normal, null);
            }
            else
            {
                this.HttpContext.Trace.Write("Caching", "Cache hit for key: " + cacheKey);
            }
            return catalog;
        }

        [HttpGet]
        [CompressFilter]
        public ActionResult RandomIllustration(string id, string width, string height)
        {
            int nWidth = 800;
            int nHeight = 600;


            if (!String.IsNullOrWhiteSpace(width))
                Int32.TryParse(width, out nWidth);
            
            if (!String.IsNullOrWhiteSpace(height))
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


        [HttpGet]
        [CompressFilter]
        [CacheFilter(Duration=3600, Cacheability=HttpCacheability.Public)]
        public ActionResult Illustration(string id, string width, string height)
        {
           
            // To Do:  Handle requested width and height.



            IPCMediatorMongoDB db = new IPCMediatorMongoDB("space_00010");
            Bitmap bitmap = db.GetIllustration(id);

            if (bitmap == null)
            {
                Response.StatusCode = 404;
                return new ContentResult() { Content = "Resource not available" };
            }
            
            using (MemoryStream ms = new MemoryStream())
            {
                bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                ms.Seek(0, SeekOrigin.Begin);
                byte[] buffer = ms.ToArray();

                FileContentResult result = new FileContentResult(buffer, "image/png");
                result.FileDownloadName = id + ".png";
                return result;
            }

        }

        [HttpGet]
        [CompressFilter]
        [CacheFilter(Duration = 0, Cacheability = HttpCacheability.Private)]
        public JsonResult GetAvailableLanguageCodes(string id)
        {
            IPCMediatorMongoDB db = new IPCMediatorMongoDB("space_00010");

            List<string> langCodes = db.GetAvailableLanguagesForCatalog(id);

            return Json(langCodes, JsonRequestBehavior.AllowGet);
        }


        public ActionResult EnableCache()
        {
            CACHE_ENABLED = true;
            return new ContentResult() { Content = "Caching is Enabled for the API" };
        }

        public ActionResult DisableCache()
        {
            CACHE_ENABLED = false;
            return new ContentResult() { Content = "Caching is Disabled for the API" };
        }



        public ActionResult PrimeCacheForIllustration(string ID)
        {
            DateTime start = DateTime.Now;
            // Get all illustration ids.

            Catalog catalog = LoadCatalog(ID, "en", "false");

            if (!CACHE_ENABLED)
            {
                return Content("Caching is not enabled.  No resources were cached");
            }

            


            List<String> illustrations = new List<string>();
            foreach (Chapter chapter in catalog.Chapter)
            {
                foreach (Page pg in chapter.Page)
                {
                    if (!String.IsNullOrEmpty(pg.IllustrationID) || !illustrations.Contains(pg.IllustrationID))
                    {
                        illustrations.Add(pg.IllustrationID);
                    }
                }
            }

            Cache cache = HttpContext.Cache;

           

            // Do the caching here....

            return Content(String.Format("Caching of resources for catalog {0} took {1} seconds.", ID, DateTime.Now.Subtract(start).TotalSeconds));
            




        }

    }
}
