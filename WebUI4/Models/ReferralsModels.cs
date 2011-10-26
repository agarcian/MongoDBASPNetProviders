using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace WebUI4.Models
{

    public class InvitationModel
    {
        [Required(ErrorMessageResourceName = "ErrorMessageRequiredField", ErrorMessageResourceType = typeof(Resources.Common))]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessageResourceName = "ErrorInvalidEmail", ErrorMessageResourceType = typeof(Resources.Account))]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resources.Account))]
        public string Email { get; set; }

        [Display(Name = "FirstName", ResourceType = typeof(Resources.Account))]
        public string FirstName { get; set; }
    }



    public class InvitationMessageModel
    {
        public string ReferrerEmail { get; set; }

        public string ReferrerFirstName { get; set; }

        public string Url { get; set; }

        public string InviteeName { get; set; }
    }

}