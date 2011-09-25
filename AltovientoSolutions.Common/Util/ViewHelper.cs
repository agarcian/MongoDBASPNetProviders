using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Web.Mvc.Html;
using System.Text;
using System.IO;
using MvcContrib.UI;
using MvcContrib.ActionResults;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;


namespace AltovientoSolutions.Common.Util
{
    public class ViewHelper
    {

               
        public static string RenderPartialToString(string userControl, object model, ControllerContext controllerContext)
        {

            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);

            HtmlHelper h = new HtmlHelper(new ViewContext(controllerContext, new WebFormView(controllerContext, "omg"), new ViewDataDictionary(), new TempDataDictionary(), writer), new ViewPage());
            var blockRenderer = new BlockRenderer(controllerContext.HttpContext);

            RenderPartialExtensions.RenderPartial(h, userControl, model);

            return sb.ToString();
        }

        public static ActionResult GetFormatedActionResult(object result)
        {
            return GetFormatedActionResult(result, "xml");
        }

        public static ActionResult GetFormatedActionResult(object result, string format)
        {
            if (String.IsNullOrEmpty(format))
                format = "xml";

            if (result == null)
                return new EmptyResult();


            switch (format.ToLower())
            {
                case "xml":
                    return new XmlResult(result);
                case "json":
                    JsonResult jsonResult = new JsonResult();
                    jsonResult.Data = result;
                    jsonResult.JsonRequestBehavior = JsonRequestBehavior.AllowGet;
                    return jsonResult;
                default:
                    goto case "xml";
            }
        }

        public static List<SelectListItem> GetTimesOfDay()
        {
            SelectListItem li;

            List<SelectListItem> timesOfDay = new List<SelectListItem>();


            li = new SelectListItem();
            li.Text = String.Format("00:00 (12:00 am)");
            li.Value = String.Format("12:00 am");

            timesOfDay.Add(li);


            for (int i = 1; i < 24; i++)
            {
                li = new SelectListItem();
                li.Text = String.Format("{1:00}:00 {2}  ({0:00}:00)", i, i > 12 ? i - 12 : i, i < 12 ? "am" : "pm");
                li.Value = String.Format("{0:00}:00 {1}", i > 12 ? i - 12 : i, i < 12 ? "am" : "pm");

                timesOfDay.Add(li);
            }

            return timesOfDay;
        }

    }
}
