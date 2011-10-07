using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.Common.MVC;
using AltovientoSolutions.Security.Spaces;
using AltovientoSolutions.DAL.Security;
using System.Web.Profile;
using System.Web.Security;

namespace WebUI4.Controllers
{
    public class SpacesController : Controller
    {
        //
        // GET: /Spaces/
        [CompressFilter]
        [Authorize]
        [HttpGet]
        public ActionResult Index()
        {
            Models.SpacesModel model = new Models.SpacesModel();
            
            // Find the space associated to this account.
            MongoDBSpacesProvider provider = new MongoDBSpacesProvider();
            MembershipUser user = Membership.GetUser(User.Identity.Name, true);
            List<Space> spaces = provider.GetSpacesByOwner(user.Email);

            if (spaces.Count > 0)
            {
                return View("AllSpaces", spaces);
            }

            return View(model);
        }

        [CompressFilter]
        [Authorize]
        [HttpPost]
        public ActionResult Index(Models.SpacesModel model)
        {
            if (ModelState.IsValid)
            {
                MongoDBSpacesProvider provider = new MongoDBSpacesProvider();

                string name = model.SpaceName.Trim().ToLower();

                if (provider.NameExists(name))
                {
                    ModelState.AddModelError("Unavailable", String.Format("The work space name '{0}' is not available", name));
                }
                else
                {
                    MembershipUser user = Membership.GetUser(User.Identity.Name, true);
                    provider.CreateSpace(model.SpaceName, user.Email);
                }
            }
            return View(model);
        }

    }
}
