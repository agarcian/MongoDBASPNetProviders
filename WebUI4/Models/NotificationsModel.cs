using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI4.Models
{
    public class VerificationModel
    {
        public string Url
        {
            get;
            set;
        }
    }

    public class PasswordRetrievalModel
    {
        public string Url
        {
            get;
            set;
        }
        public string Email
        {
            get;
            set;
        }

    }
}