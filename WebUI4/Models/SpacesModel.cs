using System;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebUI4.Models
{
    public class SpacesModel
    {
        [ConfigurationProperty("urlDomain", IsRequired = false)]
        [RegexStringValidator(@"([a-z0-9][A-Z0-9])*")]
        [Display(Name="Site Name")]
        public string SpaceName
        {
            get;
            set;
        }
    }
}