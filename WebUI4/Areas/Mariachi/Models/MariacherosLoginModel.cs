using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI4.Areas.Mariachi.Models
{
    public class MariacherosLoginModel
    {
        [Required(ErrorMessageResourceName = "ErrorMessageRequiredField", ErrorMessageResourceType = typeof(Resources.Common))]
        [Display(Name = "EnterEmailAddress", ResourceType = typeof(Resources.Mariacheros))]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessageResourceName = "ErrorInvalidEmail", ErrorMessageResourceType = typeof(Resources.Account))]
        public string Email
        {
            get;
            set;
        }

        [RegularExpression("^[a-zA-Z0-9_-]*", ErrorMessageResourceName = "ValidatorOnlyAlphanumeric", ErrorMessageResourceType = typeof(WebUI4.Resources.Common))]
        [Required(ErrorMessageResourceName = "ErrorMessageRequiredField", ErrorMessageResourceType = typeof(Resources.Common))]
        [Display(Name = "PickASiteName", ResourceType = typeof(Resources.Mariacheros))]
        public string Sitename
        {
            get;
            set;
        }


        [DataType(DataType.Password)]
        [Display(Name = "ChooseAPassword", ResourceType = typeof(Resources.Mariacheros))]
        [Required(ErrorMessageResourceName = "ErrorMessageRequiredField", ErrorMessageResourceType = typeof(Resources.Common))]
        public string Password
        {
            get;
            set;
        }
    }
}