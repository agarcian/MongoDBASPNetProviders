using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using WebUI4.Models;
using AltovientoSolutions.Common.Util;
using AltovientoSolutions.Common.Instrumentation;

namespace WebUI4.Models
{
    public class NotificationsHelper
    {


        /// <summary>
        /// Sends an email with a validation token to make sure the email is associated to the account for the user.  Used when registering users.
        /// </summary>
        /// <param name="email">The email associated to the user account</param>
        /// <param name="controllerContext">The context of the controller.  (Required for processing the request)</param>
        public static void SendEmailWithValidationToken(string email, System.Web.Mvc.ControllerContext controllerContext)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();

            string token = AccountHelper.GetTokenForValidation(email.Trim().ToLower());
            string authenticationUrl = String.Format("{0}?email={1}&token={2}", GetApplicationUrl(controllerContext) + "/Account/Validate", HttpUtility.UrlEncode(email), HttpUtility.UrlEncode(token));
            string body = String.Format(ViewHelper.RenderPartialToString("MailTemplates/VerificationEmail", new VerificationModel() {Url = authenticationUrl}, controllerContext));

            message.To.Add(email);
            message.Subject = String.Format(Resources.Notifications.SubjectRegistrationEmail, ConfigurationManager.AppSettings["ApplicationName"]);
            message.Body = HttpContext.Current.Server.HtmlDecode(body);
            message.IsBodyHtml = true;

            smtpClient.EnableSsl = true;
            smtpClient.Send(message);

        }

        /// <summary>
        /// Sends an email with a url so users can reset their password.
        /// </summary>
        /// <param name="email">The email associated to the user account</param>
        /// <param name="controllerContext">The context of the controller.  (Required for processing the request)</param>
        public static void SendPasswordRetrieval(string email, System.Web.Mvc.ControllerContext controllerContext)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();

            string token = AccountHelper.GetTokenForValidation(email.Trim().ToLower());
            string url = String.Format("{0}?email={1}&token={2}", GetApplicationUrl(controllerContext) + "/Account/ResetPassword", HttpUtility.UrlEncode(email), HttpUtility.UrlEncode(token));
            string body = ViewHelper.RenderPartialToString("MailTemplates/PasswordRetrievalEmail", new PasswordRetrievalModel() { Url = url }, controllerContext);

            message.To.Add(email);
            message.Subject = String.Format(Resources.Notifications.SubjectPasswordRetrievalEmail, ConfigurationManager.AppSettings["ApplicationName"]);
            message.Body = HttpContext.Current.Server.HtmlDecode(body);
            message.IsBodyHtml = true;

            smtpClient.EnableSsl = true;
            smtpClient.Send(message);
        }

        public static void SendRegistrationEmail(string email, string firstName, string password, System.Web.Mvc.ControllerContext controllerContext)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();

            //string email = user.Dictionary.ContainsKey("email") ? user.Dictionary["email"].String : null;
            //string firstName = user.Dictionary.ContainsKey("first_name") ? user.Dictionary["first_name"].String : null;


            string body = ViewHelper.RenderPartialToString("MailTemplates/RegistrationEmail", new RegistrationModel() { FirstName = firstName, Email = email, Password = password }, controllerContext);

            message.To.Add(email);
            message.Subject = Resources.Notifications.SubjectRegistrationEmail;
            message.Body = HttpContext.Current.Server.HtmlDecode(body);
            message.IsBodyHtml = true;

            smtpClient.EnableSsl = true;
            smtpClient.Send(message);
        }

        /// <summary>
        /// Generates an email that redirects a new user to register in this location /Referral/Welcome/token
        /// </summary>
        /// <param name="fromEmail"></param>
        /// <param name="fromName"></param>
        /// <param name="toEmail"></param>
        /// <param name="toName"></param>
        /// <param name="token"></param>
        /// <param name="controllerContext"></param>
        public static void SendInvitationEmail(string fromEmail, string fromName, string toEmail, string toName, string token, System.Web.Mvc.ControllerContext controllerContext)
        {
            MailMessage message = new MailMessage();
            SmtpClient smtpClient = new SmtpClient();

            string url = String.Format("{0}/Referrals/Welcome?token={1}", GetApplicationUrl(controllerContext), controllerContext.HttpContext.Server.UrlEncode(token));

            string body = ViewHelper.RenderPartialToString("MailTemplates/InvitationEmail", new InvitationMessageModel() { InviteeName = toName, ReferrerEmail = fromEmail, ReferrerFirstName = fromName, Url = url }, controllerContext);
            
            message.To.Add(toEmail);
            message.Subject = String.Format(Resources.Notifications.SubjectInvitationEmail, ConfigurationManager.AppSettings["ApplicationName"]);
            message.Body = HttpContext.Current.Server.HtmlDecode(body);
            message.IsBodyHtml = true;

            smtpClient.EnableSsl = true;
            smtpClient.Send(message);

            new InvitationEmailSentEvent(fromName, fromEmail, toName, toEmail, "", null, (int) CustomWebEventCodes.InvitationEmailEvent).Raise();
        }


        /// <summary>
        /// Retrieves the domain of the web application so the verification urls can be constructed.
        /// </summary>
        /// <param name="controllerContext">ControllerContext is required for the url analysis.</param>
        /// <returns>The base url for the web application.</returns>
        private static string GetApplicationUrl(System.Web.Mvc.ControllerContext controllerContext)
        {
            Uri url = controllerContext.RequestContext.HttpContext.Request.Url;
            return String.Format("{0}://{1}", url.Scheme, url.Authority);
        }


    }
}