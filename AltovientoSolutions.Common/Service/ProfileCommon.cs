using System;
using System.Web.Profile;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AltovientoSolutions.Common.Services
{
    
    public enum AuthenticationMethodEnum
    {
        None = 0,
        Forms,
        Facebook
    }


    public interface IProfileCommon
    {
        AuthenticationMethodEnum AuthenticationMethod { get; set; }
        string UserName { get; }
        string Birthday { get; set; }
        string City { get; set; }
        string Country { get; set; }
        string Email { get; set; }
        string FacebookID { get; set; }
        string FirstName { get; set; }
        string FlickerID { get; set; }
        string Gender { get; set; }
        string GeoLocation { get; set; }
        string LastName { get; set; }
        string Locale { get; set; }
        string State { get; set; }
        string Street { get; set; }
        string Zip { get; set; }
        string TwitterID { get; set; }
        string TwitterSessionKey { get; set; }
        string TwitterSessionSecret { get; set; }
        string ReferredBy { get; set; }
        void Save();
    }

    /// <summary>
    /// Used to handle the custom Profile Provider.  MVC does not automatically generate the code as regular ASP.NET, so it needs to be generated manually.
    /// </summary>
    public class ProfileCommon : ProfileBase, IProfileCommon
    {
        [SettingsAllowAnonymous(false), CustomProviderData("UserName;nvarchar")]
        public string UserName { get { return base.UserName; } }
        [SettingsAllowAnonymous(false), CustomProviderData("FirstName;nvarchar")]
        public string FirstName { get { return base["FirstName"] as string; } set { base["FirstName"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("LastName;nvarchar")]
        public string LastName { get { return base["LastName"] as string; } set { base["LastName"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("Email;nvarchar")]
        public string Email { get { return base["Email"] as string; } set { base["Email"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("Gender;nvarchar")]
        public string Gender { get { return base["Gender"] as string; } set { base["Gender"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("Birthday;nvarchar")]
        public string Birthday { get { return base["Birthday"] as string; } set { base["Birthday"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("GeoLocation;nvarchar")]
        public string GeoLocation { get { return base["GeoLocation"] as string; } set { base["GeoLocation"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("Street;nvarchar")]
        public string Street { get { return base["Street"] as string; } set { base["Street"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("City;nvarchar")]
        public string City { get { return base["City"] as string; } set { base["City"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("State;nvarchar")]
        public string State { get { return base["State"] as string; } set { base["State"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("Zip;nvarchar")]
        public string Zip { get { return base["Zip"] as string; } set { base["Zip"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("Country;nvarchar")]
        public string Country { get { return base["Country"] as string; } set { base["Country"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("Locale;nvarchar")]
        public string Locale { get { return base["Locale"] as string; } set { base["Locale"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("FacebookID;nvarchar")]
        public string FacebookID { get { return base["FacebookID"] as string; } set { base["FacebookID"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("TwitterID;nvarchar")]
        public string TwitterID { get { return base["TwitterID"] as string; } set { base["TwitterID"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("TwitterSessionKey;nvarchar")]
        public string TwitterSessionKey { get { return base["TwitterSessionKey"] as string; } set { base["TwitterSessionKey"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("TwitterSessionSecret;nvarchar")]
        public string TwitterSessionSecret { get { return base["TwitterSessionSecret"] as string; } set { base["TwitterSessionSecret"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("ReferredBy;nvarchar")]
        public string ReferredBy { get { return base["ReferredBy"] as string; } set { base["ReferredBy"] = value; } }
        [SettingsAllowAnonymous(false), CustomProviderData("FlickerID;nvarchar")]
        public string FlickerID { get { return base["FlickerID"] as string; } set { base["FlickerID"] = value; } }

        // Do not persist.  Only stored in the session.
        private AuthenticationMethodEnum authenticationMethod = AuthenticationMethodEnum.None;
        public AuthenticationMethodEnum AuthenticationMethod
        {
            get { return authenticationMethod; }
            set { authenticationMethod = value; }
        }

        public static ProfileCommon Create(string username)
        {
            
            return ProfileBase.Create(username) as ProfileCommon;
        }

        public static ProfileCommon Create(string username, bool isAuthenticated)
        {
            return ProfileBase.Create(username, isAuthenticated) as ProfileCommon;
        }

        public static ProfileCommon GetProfile(string username)
        {
            return Create(username) as ProfileCommon;
        }


    }
}
