﻿using System;
using System.Web; // For the reference to HttpContext
using System.Web.Management;

namespace AltovientoSolutions.Common.Instrumentation
{
    // http://msdn.microsoft.com/en-us/library/ms998325.aspx

    public class PasswordResetEvent : WebSuccessAuditEvent
    {
        public PasswordResetEvent(string msg, object eventSource, int eventCode)
            : base(msg, eventSource, eventCode)
        {
        }

        public PasswordResetEvent(string msg, object eventSource, int eventCode, int eventDetailCode)
            : base(msg, eventSource, eventCode, eventDetailCode)
        {
        }
        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
            base.FormatCustomEventDetails(formatter);

            // Add custom data.
            formatter.AppendLine("");
            formatter.IndentationLevel += 1;
            formatter.AppendLine("******** PasswordResetEvent Start ********");
            formatter.AppendLine(string.Format("Request path: {0}", RequestInformation.RequestPath));
            formatter.AppendLine(string.Format("Request Url: {0}", RequestInformation.RequestUrl));

            // Display custom event message.
            formatter.AppendLine("Password Reset: " + Message);

            formatter.AppendLine("******** PasswordResetEvent End ********");

            formatter.IndentationLevel -= 1;
        }
    }
}
