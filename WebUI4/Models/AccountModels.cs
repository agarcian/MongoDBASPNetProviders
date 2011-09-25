using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Security;

namespace WebUI4.Models
{

    public class ChangePasswordModel
    {
        [Required(ErrorMessageResourceName="ErrorMessageRequiredField", ErrorMessageResourceType=typeof(Resources.Common))]
        [DataType(DataType.Password)]
        [Display(Name = "CurrentPassword", ResourceType = typeof(Resources.Account))]
        public string OldPassword { get; set; }

        [Required(ErrorMessageResourceName="ErrorMessageRequiredField", ErrorMessageResourceType=typeof(Resources.Common))]
        [StringLength(100, ErrorMessageResourceName = "ErrorMustBeXCharactersLong", ErrorMessageResourceType = typeof(Resources.Account), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resources.Account))]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(Resources.Account))]
        [Compare("NewPassword", ErrorMessageResourceName = "ErrorNewPasswordAndConfirmationDoNotMatch", ErrorMessageResourceType = typeof(Resources.Account))]
        public string ConfirmPassword { get; set; }
    }

    public class LogOnModel
    {
        [Required(ErrorMessageResourceName="ErrorMessageRequiredField", ErrorMessageResourceType=typeof(Resources.Common))]
        [Display(Name = "UsernameOrEmail", ResourceType = typeof(Resources.Account))]
        public string UserNameOrEmail { get; set; }

        [Required(ErrorMessageResourceName="ErrorMessageRequiredField", ErrorMessageResourceType=typeof(Resources.Common))]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resources.Account))]
        public string Password { get; set; }

        [Display(Name = "RememberMe", ResourceType = typeof(Resources.Account))]
        public bool RememberMe { get; set; }
    }

    public class RegistrationModel
    {
        [Required(ErrorMessageResourceName="ErrorMessageRequiredField", ErrorMessageResourceType=typeof(Resources.Common))]
        [Display(Name = "Username", ResourceType = typeof(Resources.Account))]
        public string UserName { get; set; }

        [Required(ErrorMessageResourceName="ErrorMessageRequiredField", ErrorMessageResourceType=typeof(Resources.Common))]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessageResourceName="ErrorInvalidEmail", ErrorMessageResourceType=typeof(Resources.Account))]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resources.Account))]
        public string Email { get; set; }

        //[Required(ErrorMessageResourceName="ErrorMessageRequiredField", ErrorMessageResourceType=typeof(Resources.Common))]
        [StringLength(100, ErrorMessageResourceName = "ErrorMustBeXCharactersLong", ErrorMessageResourceType = typeof(Resources.Account), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password", ResourceType = typeof(Resources.Account))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmPassword", ResourceType = typeof(Resources.Account))]
        [Compare("Password", ErrorMessageResourceName = "ErrorPasswordAndConfirmationDoNotMatch", ErrorMessageResourceType = typeof(Resources.Account))]
        public string ConfirmPassword { get; set; }


        [Display(Name = "FirstName", ResourceType = typeof(Resources.Account))]
        public string FirstName { get; set; }

        [Display(Name = "LastName", ResourceType = typeof(Resources.Account))]
        public string LastName { get; set; }


    }

    public class ProfileModel
    {
        [Display(Name = "Username", ResourceType = typeof(Resources.Account))]
        public string UserName { get; set; }

        [DataType(DataType.EmailAddress)]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resources.Account))]
        public string Email { get; set; }

        [Display(Name = "FirstName", ResourceType = typeof(Resources.Account))]
        public string FirstName { get; set; }

        [Display(Name = "LastName", ResourceType = typeof(Resources.Account))]
        public string LastName { get; set; }

        [Display(Name = "Country", ResourceType = typeof(Resources.Account))]
        public string Country { get; set; }



    }

    public class ResetPasswordModel
    {
        [Required(ErrorMessageResourceName = "ErrorMessageRequiredField", ErrorMessageResourceType = typeof(Resources.Common))]
        [Display(Name = "UsernameOrEmail", ResourceType = typeof(Resources.Account))]
        public string UserNameOrEmail { get; set; }

        [Required(ErrorMessageResourceName = "ErrorMessageRequiredField", ErrorMessageResourceType = typeof(Resources.Common))]
        [StringLength(100, ErrorMessageResourceName = "ErrorMustBeXCharactersLong", ErrorMessageResourceType = typeof(Resources.Account), MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "NewPassword", ResourceType = typeof(Resources.Account))]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "ConfirmNewPassword", ResourceType = typeof(Resources.Account))]
        [Compare("Password", ErrorMessageResourceName = "ErrorNewPasswordAndConfirmationDoNotMatch", ErrorMessageResourceType = typeof(Resources.Account))]
        public string ConfirmPassword { get; set; }
    }

    public class ConfirmationModel
    {

        public string Title
        {
            get;
            set;
        }

        public string Message
        {
            get;
            set;
        }

        public bool IsSuccess
        {
            get;
            set;
        }
    }

    public class RetrievePasswordModel
    {
        [Required(ErrorMessageResourceName = "ErrorMessageRequiredField", ErrorMessageResourceType = typeof(Resources.Common))]
        [DataType(DataType.EmailAddress)]
        [RegularExpression(@"[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?", ErrorMessageResourceName = "ErrorInvalidEmail", ErrorMessageResourceType = typeof(Resources.Account))]
        [Display(Name = "EmailAddress", ResourceType = typeof(Resources.Account))]
        public string Email { get; set; }
    }

}
