using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace AltovientoSolutions.Common.MVC
{
    public class RequireBasicAuthentication : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var req = filterContext.HttpContext.Request;
            if (String.IsNullOrEmpty(req.Headers["Authorization"]))
            {
                var res = filterContext.HttpContext.Response;
                res.StatusCode = 401;
                res.AddHeader("WWW-Authenticate", "Basic realm=\"MyApplication\"");   //  <-- Change here for the name of your site.
                res.End();
                filterContext.Result = new EmptyResult();
            }
            else
            {
                // retrieves the user id and password and maps them to the appkey and apisecret.
                var cred = System.Text.ASCIIEncoding.ASCII.GetString(Convert.FromBase64String(req.Headers["Authorization"].Substring(6))).Split(':');
                filterContext.Controller.ViewData["basic_authentication_username"] = cred[0];
                filterContext.Controller.ViewData["basic_authentication_password"] = cred[1];
            }
        }
    }
}