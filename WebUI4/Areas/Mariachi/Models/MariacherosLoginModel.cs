using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI4.Areas.Mariachi.Models
{
    public class MariacherosLoginModel
    {
        [Required(ErrorMessage = "*")]
        [Display(Name = "EnterEmailAddress", ResourceType = typeof(Resources.Mariacheros))]
        public string Email
        {
            get;
            set;
        }

        [Required(ErrorMessage = "*")]
        [Display(Name = "PickASiteName", ResourceType = typeof(Resources.Mariacheros))]
        public string Sitename
        {
            get;
            set;
        }

        [Required(ErrorMessage = "*")]
        [DataType(DataType.Password)]
        [Display(Name = "ChooseAPassword", ResourceType = typeof(Resources.Mariacheros))]
        public string Password
        {
            get;
            set;
        }
    }
}