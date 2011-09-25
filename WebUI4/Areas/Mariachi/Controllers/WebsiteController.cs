using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.DAL.Mariacheros;

namespace WebUI4.Areas.Mariachi.Controllers
{
    public class WebsiteController : Controller
    {
        //
        // GET: /Mariachi/Website/
        [HttpGet()]
        public ActionResult Index()
        {
                //Check if the user already has a site created.
                Dictionary<String, String> sites = new Dictionary<string,string>();

            if (User.Identity.IsAuthenticated)
            {
                
                //if (MariachiMediator.GetSitesForUser(User.Identity.Name, out sites))
                //{
                //    return RedirectToAction("Manage");

                //}

                // IF the site has been created, redirect to the manage site details page.

                // If not, allow to create a site in the view.

            }
                //return the default view for non authenticated useres.
                return View();
        }

        [Authorize()]
        [HttpGet]
        public ActionResult Create(string id)
        {
            // id should be the site's name for the current user.

            // Check if id is a valid site for the user, if not redirect to Index where site can be created.

            // Dispay all details about site and allow making changes.


            return View();
        }



        //private static string getETagForResource(string resourceId)
        //{

        //    return "XXXX";
        //}

        //private static bool CanReturn304BasedOnETag(string resourceId, HttpContextBase context)
        //{
        //    //context.Response
        //    string clientToken = context.Request.Headers["If-None-Match"];

        //    //returns a unique tag that identifies the version of this unique resource.
        //    string token = getETagForResource(resourceId);

        //    if (String.IsNullOrEmpty(clientToken) || token != clientToken)
        //    {
        //        context.Response.Headers.Add("ETag", token);
        //        return false;
        //    }
        //    else
        //    {
        //        context.Response.SuppressContent = true;
        //        context.Response.StatusCode = 304;
        //        context.Response.StatusDescription = "Not Modified";
        //        context.Response.Headers.Add("Content-Length", "0");
        //        return true;
        //    }
        //}




        [Authorize()]
        [HttpGet]
        [OutputCache(Location=System.Web.UI.OutputCacheLocation.Any, Duration=60)]
        public ActionResult Manage(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                // If not siteid provided, that means we need to list all the sites for the user.
                Dictionary<String, String> sites = new Dictionary<string, string>();

                if (User.Identity.IsAuthenticated)
                {
                    //if (MariachiMediator.GetSitesForUser(User.Identity.Name, out sites))
                    //{

                    //}
                }
                return View(sites);
            }

            else
            {
                // Display the details of the given site id.
                return View("ManageSite",(object) id);
            }
        }



        public static void ValidateCacheOutput(HttpContext context, Object data,
        ref HttpValidationStatus status)
{
    if (context.Request.QueryString["Status"] != null)
    {
        string pageStatus = context.Request.QueryString["Status"];

        if (pageStatus == "invalid")
            status = HttpValidationStatus.Invalid;
        else if (pageStatus == "ignore")
            status = HttpValidationStatus.IgnoreThisRequest;
        else
            status = HttpValidationStatus.Valid;
    }
    else
        status = HttpValidationStatus.Valid;
        }





    }
}
