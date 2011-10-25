using System;
using System.Web; // For the reference to HttpContext
using System.Web.Management;

namespace AltovientoSolutions.Common.Instrumentation
{
    // http://msdn.microsoft.com/en-us/library/ms998325.aspx

    public class RegistrationErrorEvent : WebErrorEvent
    {
        public RegistrationErrorEvent(string msg, object eventSource, int eventCode, Exception exception)
            : base(msg, eventSource, eventCode, exception)
        {
        }

        public RegistrationErrorEvent(string msg, object eventSource, int eventCode, int eventDetailCode, Exception exception)
            : base(msg, eventSource, eventCode, eventDetailCode, exception)
        {
        }
        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
            base.FormatCustomEventDetails(formatter);

            // Add custom data.
            formatter.AppendLine("");
            formatter.IndentationLevel += 1;
            formatter.AppendLine("******** Registration Error Start ********");
            formatter.AppendLine(string.Format("Request path: {0}", RequestInformation.RequestPath));
            formatter.AppendLine(string.Format("Request Url: {0}", RequestInformation.RequestUrl));

            // Display custom event message.
            formatter.AppendLine(Message);

            formatter.AppendLine("******** Registration Error End ********");

            formatter.IndentationLevel -= 1;
        }
    }
}
