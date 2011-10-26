using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AltovientoSolutions.Security;
using AltovientoSolutions.DAL;
using AltovientoSolutions.Common.MVC;
using WebUI4.Models;

namespace WebUI4.Controllers
{
    public class ReferralsController : Controller
    {
        //protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        //{
        //    base.Initialize(requestContext);
        //    TempDataProvider = new Microsoft.Web.Mvc.CookieTempDataProvider(requestContext.HttpContext);
        //}


        //
        // GET: /Referrals/
        [Authorize]
        [CompressFilter]
        public ActionResult Index()
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            return RedirectToAction("Invitation");
        }


        [Authorize]
        [CompressFilter]
        [HttpGet]
        public ActionResult Invitation()
        {

            ViewBag.IgnoreCustomBodyStyle = true;
            return View();
        }


        [Authorize]
        [CompressFilter]
        [HttpPost]
        public ActionResult Invitation(InvitationModel model)
        {
            if (ModelState.IsValid)
            {

                ProfileCommon profile = ProfileCommon.GetProfile(User.Identity.Name);

                string fromEmail = profile.Email;
                string fromName = AccountHelper.GetProfileName(AccountHelper.ProfileNameFormat.FirstNameLastName);

                string token = AccountHelper.GetTokenForInvitation(fromEmail);
               
                // send email using an asynchronous call.
                //var t = Task.Factory.StartNew(
                //    () => NotificationsHelper.SendInvitationEmail(fromEmail, fromName, model.Email, model.FirstName, token, this.ControllerContext)
                //    );

                NotificationsHelper.SendInvitationEmail(fromEmail, fromName, model.Email, model.FirstName, token, this.ControllerContext);
                
                // save invitation record to db.
                ReferralsMediator mediator = new ReferralsMediator("WebUI4");
                mediator.SaveReferralRequest(fromEmail, "WebUI4", model.Email, "Invitation Sent");

                TempData["Message"] = Resources.Referrals.InvitationSent;

                model = new InvitationModel();
            }

            return View(model);
        }

        [CompressFilter]
        [HttpGet]
        public ActionResult Welcome(string token)
        {
            string emailReferrer;

            if (AccountHelper.GetEmailFromToken(token, out emailReferrer))
            {
                // identified the referrer.
#warning Using a session here
                Session["ReferrerEmail"] = emailReferrer;
            }

            return RedirectToAction("Register", "Account");
        }
    }
}
