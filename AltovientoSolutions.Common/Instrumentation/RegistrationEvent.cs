using System;
using System.Web; // For the reference to HttpContext
using System.Web.Management;

namespace AltovientoSolutions.Common.Instrumentation
{
    // http://msdn.microsoft.com/en-us/library/ms998325.aspx

    public class RegistrationEvent : WebAuditEvent
    {
        public RegistrationEvent(string msg, object eventSource, int eventCode)
            : base(msg, eventSource, eventCode)
        {
        }

        public RegistrationEvent(string msg, object eventSource, int eventCode, int eventDetailCode)
            : base(msg, eventSource, eventCode, eventDetailCode)
        {
        }
        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
            base.FormatCustomEventDetails(formatter);

            // Add custom data.
            formatter.AppendLine("");
            formatter.IndentationLevel += 1;
            formatter.AppendLine("******** Registration Start ********");
            formatter.AppendLine(string.Format("Request path: {0}", RequestInformation.RequestPath));
            formatter.AppendLine(string.Format("Request Url: {0}", RequestInformation.RequestUrl));

            // Display custom event message.
            formatter.AppendLine(Message);

            formatter.AppendLine("******** Registration End ********");

            formatter.IndentationLevel -= 1;
        }
    }
}
