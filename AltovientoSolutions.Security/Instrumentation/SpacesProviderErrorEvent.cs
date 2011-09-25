using System;
using System.Web; // For the reference to HttpContext
using System.Web.Management;

namespace AltovientoSolutions.Security.Instrumentation
{
    // http://msdn.microsoft.com/en-us/library/ms998325.aspx

    public class SpacesProviderErrorEvent : WebErrorEvent
    {
        public SpacesProviderErrorEvent(string msg, object eventSource, int eventCode, Exception e)
            : base(msg, eventSource, eventCode, e)
        {
            
        }

        public SpacesProviderErrorEvent(string msg, object eventSource, int eventCode, int eventDetailCode, Exception e)
            : base(msg, eventSource, eventCode, eventDetailCode, e)
        {
        }
        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
            base.FormatCustomEventDetails(formatter);

            // Add custom data.
            formatter.AppendLine("");
            formatter.IndentationLevel += 1;
            formatter.AppendLine("******** SpacesProvider Event Start ********");
            formatter.AppendLine(string.Format("Request path: {0}", RequestInformation.RequestPath));
            formatter.AppendLine(string.Format("Request Url: {0}", RequestInformation.RequestUrl));

            // Display custom event message.
            formatter.AppendLine("There was an exception in the Spaces Provider: ");
            formatter.AppendLine(Message);

            formatter.AppendLine("******** SpacesProvider Event End ********");

            formatter.IndentationLevel -= 1;
        }

    }
}
