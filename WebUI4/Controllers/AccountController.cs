using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using System.Web.Profile;
using AltovientoSolutions.Common.Util;
using AltovientoSolutions.Common.Instrumentation;
using AltovientoSolutions.Security;
using AltovientoSolutions.Common.MVC;
using AltovientoSolutions.DAL;
using WebUI4.Models;

namespace WebUI4.Controllers
{
    public class AccountController : Controller
    {
        //protected override void Initialize(System.Web.Routing.RequestContext requestContext)
        //{
        //    base.Initialize(requestContext);
        //    TempDataProvider = new Microsoft.Web.Mvc.CookieTempDataProvider(requestContext.HttpContext);
        //}

        //
        // GET: /Account/LogIn
        /// <summary>
        /// 
        /// </summary>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        /// <remarks>
        /// The rules for the LogIn(GET) method are:
        /// 1. If the user is already logged in, redirect to the home page.
        /// 2. If not logged in, check if the user is already authenticated via facebook or twitter and log them in based on any of those external accounts.
        /// 3. If not logged in to any social network, show the view and allow the user to log in.
        /// </remarks>
        [CompressFilter]
        public ActionResult LogIn(string returnUrl)
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            if (User.Identity.IsAuthenticated)
            {
                // If the user is already authenticated redirect them to the home page.
                return RedirectToAction("Index", "Home");
            }


            // Not authenticated, check for social networks.
                // check if logged in to Facebook, Twitter.
                // If so, identify the user based on their fb/tw credentials and log them in.



            return View();
        }

        //
        // POST: /Account/LogIn
        [HttpPost]
        [CompressFilter]
        public ActionResult LogIn(LogOnModel model, string returnUrl)
        {
            ViewBag.IgnoreCustomBodyStyle = true; 
            if (ModelState.IsValid)
            {
                // Try to validate as if the input is the username.
                bool success = Membership.ValidateUser(model.UserNameOrEmail, model.Password);

                if (!success)
                {
                    // If it fails to log in, check if the UsernameOrEmail field is a valid email and try to log the user with their email.
                    if (AltovientoSolutions.Common.Util.FormatHelper.IsEmail(model.UserNameOrEmail))
                    {
                        string username = Membership.GetUserNameByEmail(model.UserNameOrEmail);
                        if (!String.IsNullOrEmpty(username))
                            success = Membership.ValidateUser(username, model.Password);

                        if (success)
                            model.UserNameOrEmail = username;  // Use the actual username rather than the email for the authentication.
                    }
                }

                if (success)
                {
                    // Set the Forms Authentication cookie.
                    FormsAuthentication.SetAuthCookie(model.UserNameOrEmail, model.RememberMe);
                    if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                        && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                else
                {
                    ModelState.AddModelError("", Resources.Account.IncorrectUsernameOrPassword);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

       
        //
        // GET: /Account/Register
        [CompressFilter]
        public ActionResult Register()
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            return View();
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [CompressFilter]
        public ActionResult Register(RegistrationModel model)
        {
            ViewBag.IgnoreCustomBodyStyle = true;

            if (ModelState.IsValid)
            {
                // Attempt to register the user
                MembershipCreateStatus createStatus;
                MembershipUser membershipUser = Membership.CreateUser(model.UserName, model.Password, model.Email, null, null, true, Guid.NewGuid(), out createStatus);

                if (createStatus == MembershipCreateStatus.Success)
                {
                    
                    // Create a profile for the user.
                    ProfileCommon profile = (ProfileCommon)ProfileBase.Create(membershipUser.UserName);
                    // Transfer all the properties from the model into the profile.
                    profile.Email = membershipUser.Email;
                    profile.FirstName = model.FirstName;
                    profile.LastName = model.LastName;
                    // Saves the profile to the provider's repository.
                    profile.Save();


                    // Implement event.
#warning Using a session here
                    string referrerEmail = (string)Session["ReferrerEmail"];
                    if (referrerEmail != null)
                    {
                        // save invitation record to db.
                        ReferralsMediator mediator = new ReferralsMediator("WebUI4");
                        mediator.SaveReferralRequest(referrerEmail, "WebUI4", membershipUser.Email, "Registered");
                    }

                    // Set the cookie and redirect to home.                    
                    FormsAuthentication.SetAuthCookie(model.UserName, false /* createPersistentCookie */);
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", ErrorCodeToString(createStatus));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }



        // **************************************
        // URL: /Account/Profile
        // **************************************
        [HttpGet]
        [Authorize]
        [CompressFilter]
        public ActionResult Profile()
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            // TO DO:  In the view, add support for 'link to your facebook account...' functionality

            ProfileCommon Profile = ProfileCommon.GetProfile(User.Identity.Name);

            ProfileModel model = new ProfileModel();

            model.UserName = Profile.UserName;
            model.Email = Profile.Email;
            model.FirstName = Profile.FirstName;
            model.LastName = Profile.LastName;
            //model.Address = Profile.Street;
            //model.City = Profile.City;
            //model.State = Profile.State;
            model.Country = Profile.Country;

            return View(model);
        }

        [HttpPost]
        [Authorize]
        [CompressFilter]
        public ActionResult Profile(ProfileModel model)
        {
            ViewBag.IgnoreCustomBodyStyle = true;

            if (ModelState.IsValid)
            {
                // Attempt to register the user
                ProfileCommon Profile = ProfileCommon.GetProfile(User.Identity.Name);

                if (Profile != null)
                {
                    Profile.FirstName = model.FirstName;
                    Profile.LastName = model.LastName;
                    //Profile.Street = model.Address;
                    //Profile.City = model.City;
                    //Profile.State = model.State;
                    //Profile.Zip = model.Zip;
                    Profile.Country = model.Country;


                    // Update profile in database.
                    Profile.Save();

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", Resources.Account.ErrorProfileCouldNotBeSaved);
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.PasswordLength = Membership.MinRequiredPasswordLength;
            return View(model);
        }


        // **************************************
        // URL: /Account/LogOff
        // **************************************
        [Authorize]
        [CompressFilter()]
        public ActionResult LogOff()
        {
            ViewBag.IgnoreCustomBodyStyle = true;

            FormsAuthentication.SignOut();

            //if (Response != null)
            //    Response.Cookies["fbs_" + ConfigurationManager.AppSettings["FacebookAppId"]].Expires = DateTime.Now.Subtract(new TimeSpan(1, 0, 0));

            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        [CompressFilter]
        public ActionResult RetrievePassword()
        {
            ViewBag.IgnoreCustomBodyStyle = true;

            return View();
        }

        [HttpPost]
        [CompressFilter]
        public ActionResult RetrievePassword(PasswordRetrievalModel model)
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            ViewBag.PageTitle = String.Format(Resources.Account.RetrievePasswordFor, ConfigurationManager.AppSettings["ApplicationName"]);

            if (ModelState.IsValid)
            {
                string username = Membership.GetUserNameByEmail(model.Email);
                bool isSuccess = !String.IsNullOrEmpty(username);

                if (isSuccess)
                {
                    // send email using an asynchronous call.
                    //var t = Task.Factory.StartNew(
                    //() =>NotificationsHelper.SendPasswordRetrieval(model.Email, this.ControllerContext)
                    //);
                    NotificationsHelper.SendPasswordRetrieval(model.Email, this.ControllerContext);


                    new PasswordResetEvent(String.Format("User {0} retrieved their password.", username),
                        null, (int)CustomWebEventCodes.PasswordResetEvent).Raise();
                    
                    TempData["Message"] = Resources.Notifications.PasswordRetrievalConfirmation;
                }
                else
                {
                    new PasswordResetEvent(String.Format("*** WARNING:  A user tried to retrieve their password but the email address used '{0}' does not exist in the database.", model.Email),
                        null, (int) CustomWebEventCodes.PasswordResetEvent).Raise();

                    TempData["Message"] = Resources.Notifications.InvalidUser;
                }


                return RedirectToAction("Index", "Home");
            }

            return View(model);
        }

        [HttpGet]
        [CompressFilter]
        public ActionResult Validate(string email, string token)
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            bool isValid = false;

            if (AccountHelper.IsTokenValid(token, email))
            {
                string username = Membership.GetUserNameByEmail(email);
                if (!String.IsNullOrEmpty(username))
                {
                    // Get the user and approve it.
                    MembershipUser user = Membership.GetUser(username);
                    user.IsApproved = true;
                    Membership.UpdateUser(user);

                    isValid = true;

                    // Since it was a successful validation, authenticate the user.
                    FormsAuthentication.SetAuthCookie(username, false);
                }
                else
                {
                    isValid = false;
                }
            }

            return View(isValid);
        }

        [HttpGet]
        [CompressFilter]
        public ActionResult ResetPassword(string email, string token)
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            if (AccountHelper.IsTokenValid(token, email))
            {
                string username = Membership.GetUserNameByEmail(email);
                if (!String.IsNullOrEmpty(username))
                {
                    ResetPasswordModel resetPasswordModel = new ResetPasswordModel() { UserNameOrEmail = username };
                    return View(resetPasswordModel);
                }
                else
                {
                    // did not find a user.  Redirect to main page.
                    TempData["Message"] = Resources.Notifications.InvalidUser;
                    return RedirectToAction("Index", "Home");
                }
            }
            else
            {

                TempData["Message"] = Resources.Notifications.InvalidPasswordRetrievalToken;
                return RedirectToAction("RetrievePassword", "Account");
            }
        }

        [HttpPost]
        [CompressFilter]
        public ActionResult ResetPassword(ResetPasswordModel resetPasswordModel)
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            bool isSuccess = false;
            string username = resetPasswordModel.UserNameOrEmail;

            MembershipUser membershipUser = (MembershipUser)Membership.GetUser(resetPasswordModel.UserNameOrEmail, false  /* does not update latest activity*/ );

            if (membershipUser == null)
            {
                if (FormatHelper.IsEmail(resetPasswordModel.UserNameOrEmail))
                {
                    // Try to retrieve the user id using the email.

                    // Derive the username from the email.
                    username = Membership.GetUserNameByEmail(resetPasswordModel.UserNameOrEmail);
                    membershipUser = (MembershipUser)Membership.GetUser(username, false  /* does not update latest activity*/ );
                }
            }


            if (membershipUser != null)
            {
                // Reset password autogenerates a new password.  We are forced to use this.
                string autogeneratedPassword = membershipUser.ResetPassword();

                // Change the password to the one provided by the user.
                membershipUser.ChangePassword(autogeneratedPassword, resetPasswordModel.Password);
                isSuccess = true;
            }
            else
            {
                isSuccess = false;
            }



            if (isSuccess)
            {
                // Raise a WebEvent.
                new PasswordResetEvent(String.Format("User {0} successfully reset their password", username), this,
                                      (int)CustomWebEventCodes.PasswordChangedEvent).Raise();

                // log in the user.
                FormsAuthentication.SetAuthCookie(username, false /* createPersistentCookie */);
                    
                TempData["Message"] = Resources.Notifications.ChangePasswordConfirmation;
            }
            else
            {
                TempData["Message"] = Resources.Notifications.InvalidUser;
            }
           
            return RedirectToAction("Index", "Home");
        }

        //
        // GET: /Account/ChangePassword

        [Authorize]
        [CompressFilter]
        public ActionResult ChangePassword()
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            return View();
        }

        //
        // POST: /Account/ChangePassword

        [Authorize]
        [HttpPost]
        [CompressFilter]
        public ActionResult ChangePassword(ChangePasswordModel model)
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            if (ModelState.IsValid)
            {

                // ChangePassword will throw an exception rather
                // than return false in certain failure scenarios.
                bool changePasswordSucceeded;
                try
                {
                    MembershipUser currentUser = Membership.GetUser(User.Identity.Name, true /* userIsOnline */);
                    changePasswordSucceeded = currentUser.ChangePassword(model.OldPassword, model.NewPassword);
                }
                catch (Exception)
                {
                    changePasswordSucceeded = false;
                }

                if (changePasswordSucceeded)
                {
                    TempData["Message"] = Resources.Account.PasswordChangedSuccessfully;
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError("", Resources.Account.ErrorIncorrectPassword);
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }


        [HttpGet]
        [CompressFilter]
        [CacheFilter(Cacheability = HttpCacheability.NoCache, Duration = 0)]
        public ActionResult IsEmailAvailable(string email)
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            bool isAvailable = false;

            if (!String.IsNullOrWhiteSpace(email))
            {
                isAvailable = Membership.FindUsersByEmail(email).Count > 0;
            }

            var resultObj = new { result = isAvailable };

            return Json(resultObj, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [CompressFilter]
        [CacheFilter(Cacheability = HttpCacheability.NoCache, Duration = 0)]
        public ActionResult IsUsernameAvailable( string username)
        {
            ViewBag.IgnoreCustomBodyStyle = true;
            bool isAvailable = false;

            if (!String.IsNullOrWhiteSpace(username))
            {
                List<String> reservedNames = new List<string>();
                reservedNames.AddRange(ConfigurationManager.AppSettings["ReservedUsernames"].Split(','));

                if (reservedNames.Contains(username.Trim().ToLower()))
                {
                    isAvailable = false;
                }
                else
                {
                    isAvailable = Membership.FindUsersByName(username).Count == 0;
                }
                
            }

            var resultObj = new { result = isAvailable };

            return Json(resultObj, JsonRequestBehavior.AllowGet);
        }





        //
        // GET: /Account/ChangePasswordSuccess

        #region Status Codes
        internal static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return Resources.Account.ErrorCodeUsernameAlreadyExists;

                case MembershipCreateStatus.DuplicateEmail:
                    return Resources.Account.ErrorCodeUsernameForEmailAlreadyExists;

                case MembershipCreateStatus.InvalidPassword:
                    return Resources.Account.ErrorCodePasswordProvidedIsInvalid;

                case MembershipCreateStatus.InvalidEmail:
                    return Resources.Account.ErrorCodeEmailIsInvalid;

                case MembershipCreateStatus.InvalidAnswer:
                    return Resources.Account.ErrorCodeInvalidAnswer;

                case MembershipCreateStatus.InvalidQuestion:
                    return Resources.Account.ErrorCodeInvalidQuestion;

                case MembershipCreateStatus.InvalidUserName:
                    return Resources.Account.ErrorCodeInvalidUsername;

                case MembershipCreateStatus.ProviderError:
                    return Resources.Account.ErrorCodeProviderError;

                case MembershipCreateStatus.UserRejected:
                    return Resources.Account.ErrorCodeUserRejected;

                default:
                    return Resources.Account.ErrorCodeUnknown;
            }
        }
        #endregion
    }
}
