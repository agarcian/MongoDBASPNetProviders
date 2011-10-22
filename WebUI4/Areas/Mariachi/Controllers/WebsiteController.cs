using System;
using System.Web.Security;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.DAL.Mariacheros;
using WebUI4.Areas.Mariachi.Models;
using AltovientoSolutions.Security;
using System.Web.Profile;


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
                return Redirect(String.Format("~/{0}/Create", User.Identity.Name));
                //if (MariachiMediator.GetSitesForUser(User.Identity.Name, out sites))
                //{
                //    return RedirectToAction("Manage");

                //}

                // IF the site has been created, redirect to the manage site details page.

                // If not, allow to create a site in the view.

            }
                //return the default view for non authenticated useres.
            WebUI4.Areas.Mariachi.Models.MariacherosLoginModel model = new Models.MariacherosLoginModel();
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(MariacherosLoginModel model)
        {

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Err", "There is something wrong.  Please try again.");
                return View(new MariacherosLoginModel());
            }


            // Check if the sitename (username) is available or not, or it is in the reserved words.
            bool isUsernameAvailable = false;
            // Same as AccountController.IsUsernameAvailable
            if (!String.IsNullOrWhiteSpace(model.Sitename))
            {
                List<String> reservedNames = new List<string>();
                reservedNames.AddRange(ConfigurationManager.AppSettings["ReservedUsernames"].Split(','));

                if (reservedNames.Contains(model.Sitename.Trim().ToLower()))
                {
                    isUsernameAvailable = false;
                }
                else
                {
                    isUsernameAvailable = Membership.FindUsersByName(model.Sitename).Count == 0;
                }
            }


            if (!isUsernameAvailable)
            {
                ModelState.AddModelError("sitename", "This name has already been taken");
                return View(new MariacherosLoginModel());
            }


            // At this point the input is valid.  Create a new user.
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                MembershipUser membershipUser = Membership.CreateUser(model.Sitename, model.Password, model.Email, null, null, true, Guid.NewGuid(), out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {

                    // Create a profile for the user.
                    ProfileCommon profile = (ProfileCommon)ProfileBase.Create(membershipUser.UserName);
                    // Transfer all the properties from the model into the profile.
                    profile.Email = membershipUser.Email;

                    // Saves the profile to the provider's repository.
                    profile.Save();


                    // Set the cookie and redirect to home.                    
                    FormsAuthentication.SetAuthCookie(model.Sitename, false /* createPersistentCookie */);
                    return Redirect(String.Format("~/{0}/Manage/", model.Sitename)); 
                }
                else
                {
#warning Send notification here since there should not be errors at this stage.
                    ModelState.AddModelError("Membership", "There has been an error creating your account.  Please try again.");
                    return View(new MariacherosLoginModel());
                }
            }
        }


        /// <summary>
        /// This action will present the user profile as viewed by external users.  Called when the url is /{username}
        /// </summary>
        /// <param name="user"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Profile(string user, string id)
        {
            ViewBag.User = user;

            MariachiMediator mediator = new MariachiMediator("Bands");


            // If the profile for the given user does not exist ...
            if (!mediator.DoesProfileExist(user))
            {
                // And if the user is authenticated, 
                // and the url is for the authenticated user.. 
                // redirect to the create page, so the logged in user can create their profile.
                if (User.Identity.IsAuthenticated && String.Compare(User.Identity.Name, user, true) == 0)
                {
                    return Redirect(String.Format("~/{0}/Create", user));
                }
                else
                {
                    // if no logged in user, 
                    return new HttpNotFoundResult("Profile not found.");
                }
            }
            else
            {
                // Display the profile of the user...
                return View();
            }
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
        [OutputCache(Location = System.Web.UI.OutputCacheLocation.None, Duration = 0)]
        public ActionResult Create(string user)
        {
            if (String.Compare(user, User.Identity.Name, true) == 0)
            {

                MariachiMediator mediator = new MariachiMediator("Bands");

                if (mediator.DoesProfileExist(user))
                {
                    return Redirect(String.Format("~/{0}/Manage", user));
                }
            }
            else
            {
                // If the logged in user is not the same as the user in the url.
                return Redirect(String.Format("~/{0}", user));
            }

            return View();
        }


        [Authorize()]
        [HttpPost]
        public ActionResult Create(string user, string profileType)
        {
            if (String.Compare(user, User.Identity.Name, true) == 0)
            {
                MariachiMediator mediator = new MariachiMediator("Bands");

                if (mediator.DoesProfileExist(user))
                {
                    return Redirect(String.Format("~/{0}/Manage", user));
                }
                else
                {
                   // Create an empty profile setting the profileType.
                    bool success = mediator.CreateProfile(user, profileType);
                    return Redirect(String.Format("~/{0}/Manage", user));
                }
            }
            else
            {
                // If the logged in user is not the same as the user in the url.
                return Redirect("~/");
            }
        }


        [Authorize()]
        [HttpGet]
        [OutputCache(Location=System.Web.UI.OutputCacheLocation.None, Duration=00)]
        public ActionResult Manage(string user)
        {

            MariachiMediator mediator = new MariachiMediator("Bands");
            
            if ((String.Compare(user, User.Identity.Name, true) == 0) &&
                mediator.DoesProfileExist(user))
            {
               // If the user is the currently logged in user, and their profile exists...  

                return View();
            }
            else
            {
                // if the user does not exist or the logged in users is not the user requested
                return new HttpStatusCodeResult(401);
                // a 401 result will result in a redirection as the browser tries to reauthorize again with the root url.
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
