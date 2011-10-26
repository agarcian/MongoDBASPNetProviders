using System;
using System.Web; // For the reference to HttpContext
using System.Web.Management;

namespace AltovientoSolutions.Common.Instrumentation
{
    // http://msdn.microsoft.com/en-us/library/ms998325.aspx

    public class InvitationEmailSentEvent : WebSuccessAuditEvent
    {

        private string fromUser;
        private string fromEmailAddress;
        private string toName;
        private string toEmailAddress;

        public InvitationEmailSentEvent(string fromUser, string fromEmailAddress, string toName, string toEmailAddress, string msg, object eventSource, int eventCode)
            : base(msg, eventSource, eventCode)
        {
            this.fromUser = fromUser;
            this.fromEmailAddress = fromEmailAddress;
            this.toName = toName;
            this.toEmailAddress = toEmailAddress;
        }

        public InvitationEmailSentEvent(string fromUser, string fromEmailAddress, string toName, string toEmailAddress, string msg, object eventSource, int eventCode, int eventDetailCode)
            : base(msg, eventSource, eventCode, eventDetailCode)
        {
        }
        public override void FormatCustomEventDetails(WebEventFormatter formatter)
        {
            base.FormatCustomEventDetails(formatter);

            // Add custom data.
            formatter.AppendLine("");
            formatter.IndentationLevel += 1;
            formatter.AppendLine("******** Email Invitation Sent Start ********");
            formatter.AppendLine(string.Format("Request path: {0}", RequestInformation.RequestPath));
            formatter.AppendLine(string.Format("Request Url: {0}", RequestInformation.RequestUrl));

            // Display custom event message.
            formatter.AppendLine("Invitation: " + Message);

            formatter.AppendLine("Yipeee!   A user has sent an invitation to a friend:");
            formatter.AppendLine(String.Format("From User: {0} <{1}>", this.fromUser, this.fromEmailAddress));
            formatter.AppendLine(String.Format("To User: {0} <{1}>", this.toName, this.toEmailAddress));

            formatter.AppendLine("******** Email Invitation Sent End ********");

            formatter.IndentationLevel -= 1;
        }
    }
}
