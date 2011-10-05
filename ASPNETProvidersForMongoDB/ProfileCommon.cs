using System;
using System.Web.Profile;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyCompany.Security.Profile
{
    /// <summary>
    /// Used to handle the custom Profile Provider.  
    /// MVC does not automatically generate the code as regular ASP.NET, 
    /// so it needs to be generated manually.
    /// </summary>
    public class ProfileCommon : ProfileBase
    {
        [SettingsAllowAnonymous(false)]
        public string UserName { get { return base.UserName; } }
        [SettingsAllowAnonymous(false)]
        public string FirstName { get { return base["FirstName"] as string; } set { base["FirstName"] = value; } }
        [SettingsAllowAnonymous(false)]
        public string LastName { get { return base["LastName"] as string; } set { base["LastName"] = value; } }
        [SettingsAllowAnonymous(false)]
        public string Email { get { return base["Email"] as string; } set { base["Email"] = value; } }
        [SettingsAllowAnonymous(false)]
        public string Street { get { return base["Street"] as string; } set { base["Street"] = value; } }
        [SettingsAllowAnonymous(false)]
        public string City { get { return base["City"] as string; } set { base["City"] = value; } }
        [SettingsAllowAnonymous(false)]
        public string State { get { return base["State"] as string; } set { base["State"] = value; } }
        [SettingsAllowAnonymous(false)]
        public string Zip { get { return base["Zip"] as string; } set { base["Zip"] = value; } }
        [SettingsAllowAnonymous(false)]
        public string Country { get { return base["Country"] as string; } set { base["Country"] = value; } }
        
        // Add as many properties you want for your application.


        /// <summary>
        /// Creates the specified ProfileCommon based on the given username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>An instance of <see cref="ProfileCommon"/></returns>
        public static ProfileCommon Create(string username)
        {
            return ProfileBase.Create(username) as ProfileCommon;
        }

        /// <summary>
        /// Creates a profile based on the specified username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="isAuthenticated">if set to <c>true</c> indicates that the user is authenticated.</param>
        /// <returns>An instance of <see cref="ProfileCommon"/></returns>
        public static ProfileCommon Create(string username, bool isAuthenticated)
        {
            return ProfileBase.Create(username, isAuthenticated) as ProfileCommon;
        }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>An instance of <see cref="ProfileCommon"/></returns>
        public static ProfileCommon GetProfile(string username)
        {
            return Create(username) as ProfileCommon;
        }
    }
}