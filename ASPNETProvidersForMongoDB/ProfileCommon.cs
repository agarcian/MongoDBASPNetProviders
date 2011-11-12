using System;
using System.Web.Profile;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ASPNETProvidersForMongoDB
{
    /// <summary>
    /// Used to handle the custom Profile Provider.  
    /// MVC does not automatically generate the code as regular ASP.NET, 
    /// so it needs to be generated manually.
    /// </summary>
    public class ProfileCommon : ProfileBase
    {
        /// <summary>
        /// Gets the user name for the profile.
        /// </summary>
        /// <returns>The user name for the profile or the anonymous-user identifier assigned to the profile.</returns>
        [SettingsAllowAnonymous(false)]
        public string UserName { get { return base.UserName; } }
        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        [SettingsAllowAnonymous(false)]
        public string FirstName { get { return base["FirstName"] as string; } set { base["FirstName"] = value; } }
        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        [SettingsAllowAnonymous(false)]
        public string LastName { get { return base["LastName"] as string; } set { base["LastName"] = value; } }
        /// <summary>
        /// Gets or sets the email.
        /// </summary>
        /// <value>
        /// The email.
        /// </value>
        [SettingsAllowAnonymous(false)]
        public string Email { get { return base["Email"] as string; } set { base["Email"] = value; } }
        /// <summary>
        /// Gets or sets the street.
        /// </summary>
        /// <value>
        /// The street.
        /// </value>
        [SettingsAllowAnonymous(false)]
        public string Street { get { return base["Street"] as string; } set { base["Street"] = value; } }
        /// <summary>
        /// Gets or sets the city.
        /// </summary>
        /// <value>
        /// The city.
        /// </value>
        [SettingsAllowAnonymous(false)]
        public string City { get { return base["City"] as string; } set { base["City"] = value; } }
        /// <summary>
        /// Gets or sets the state.
        /// </summary>
        /// <value>
        /// The state.
        /// </value>
        [SettingsAllowAnonymous(false)]
        public string State { get { return base["State"] as string; } set { base["State"] = value; } }
        /// <summary>
        /// Gets or sets the zip.
        /// </summary>
        /// <value>
        /// The zip.
        /// </value>
        [SettingsAllowAnonymous(false)]
        public string Zip { get { return base["Zip"] as string; } set { base["Zip"] = value; } }
        /// <summary>
        /// Gets or sets the country.
        /// </summary>
        /// <value>
        /// The country.
        /// </value>
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