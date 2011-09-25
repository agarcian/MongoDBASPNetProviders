using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using System.Text;
using System.IO;
using MvcContrib.ActionResults;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;


namespace AltovientoSolutions.Common.Util
{
    public class FormatHelper
    {
        public static bool IsEmail(string inputEmail)
        {
            if (String.IsNullOrEmpty(inputEmail))
                return false;

            inputEmail = inputEmail.Trim();

            string strRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}" +
                  @"\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\" +
                  @".)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(inputEmail))
                return (true);
            else
                return (false);
        }
    }
}
