using System;
using System.Web; // For the reference to HttpContext
using System.Web.Management;

namespace AltovientoSolutions.Common.Instrumentation
{
    
    // Use this class as a sample to create needed events.
    public class SimpleDBErrorEvent : System.Web.Management.WebErrorEvent
    {

        public SimpleDBErrorEvent(string msg, object eventSource, int eventCode, Exception exc)
            : base(msg, eventSource, eventCode, exc)
        {
            
        }

        public SimpleDBErrorEvent(string msg, object eventSource, int eventCode,
                               int eventDetailCode, Exception exc)
            : base(msg, eventSource, eventCode, eventDetailCode, exc)
        {
        }

        // Formats Web request event information.
        // This method is invoked indirectly by the provider using one of the
        // overloaded ToString methods. If buffering is enabled, this method is
        // called asynchronously on a non-Web request thread, where the 
        // HttpContext is not available.
        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
            base.FormatCustomEventDetails(formatter);
            formatter.AppendLine("Exception" + this.ErrorException.Message);
        }
    }
}
