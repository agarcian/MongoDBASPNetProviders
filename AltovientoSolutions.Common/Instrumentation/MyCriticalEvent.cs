using System;
using System.Web; // For the reference to HttpContext
using System.Web.Management;

namespace AltovientoSolutions.Common.Instrumentation
{
    
    // Use this class as a sample to create needed events.
    public class MyCriticalEvent : WebAuditEvent
    {
        private string userID;
        private string authType;
        private bool isAuthenticated;

        public MyCriticalEvent(string msg, object eventSource, int eventCode)
            : base(msg, eventSource, eventCode)
        {
            // Obtain the HTTP Context and store authentication details
            userID = HttpContext.Current.User.Identity.Name;
            authType = HttpContext.Current.User.Identity.AuthenticationType;
            isAuthenticated = HttpContext.Current.User.Identity.IsAuthenticated;
        }

        public MyCriticalEvent(string msg, object eventSource, int eventCode,
                               int eventDetailCode)
            : base(msg, eventSource, eventCode, eventDetailCode)
        {
            // Obtain the HTTP Context and store authentication details
            userID = HttpContext.Current.User.Identity.Name;
            authType = HttpContext.Current.User.Identity.AuthenticationType;
            isAuthenticated = HttpContext.Current.User.Identity.IsAuthenticated;
        }

        // Formats Web request event information.
        // This method is invoked indirectly by the provider using one of the
        // overloaded ToString methods. If buffering is enabled, this method is
        // called asynchronously on a non-Web request thread, where the 
        // HttpContext is not available.
        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
            base.FormatCustomEventDetails(formatter);
            formatter.AppendLine("User ID: " + userID);
            formatter.AppendLine("Authentication Type: " + authType);
            formatter.AppendLine("User Authenticated: " + isAuthenticated.ToString());
            formatter.AppendLine("Activity Description: Critical Operation");
        }
    }
}
