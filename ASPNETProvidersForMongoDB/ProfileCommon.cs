using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Web.Profile;


namespace ASPNETProvidersForMongoDB
{

    /// <summary>
    /// Used to handle the custom Profile Provider.  MVC does not automatically generate the code as regular ASP.NET, so it needs to be generated manually.
    /// </summary>
    public class ProfileCommon : ProfileBase
    {

        #region Custom properties

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
        /// <summary>
        /// Gets or sets the SampleListString
        /// </summary>
        [SettingsAllowAnonymous(false)]
        public List<String> SampleListString { get { return base["SampleListString"] as List<String>; } set { base["SampleListString"] = value; } }
        #endregion

        public static new ProfileCommon Create(string applicationSpace, string groupId, string username)
        {
            // Check if the profile exists in the database.

            ProfileProvider profileProvider = System.Web.Profile.ProfileManager.Provider;
            if (profileProvider != null)
            {
                // defaults isAuthenticated to true.
                return Create(applicationSpace, groupId, username, true);
            }
            else
            {
                return ProfileBase.Create(username) as ProfileCommon;
            }
        }

        public static new ProfileCommon Create(string applicationSpace, string groupId, string username, bool isAuthenticated)
        {
            ProfileProvider profileProvider = System.Web.Profile.ProfileManager.Provider;
            if (profileProvider != null)
            {
                ProfileCommon profileCommon = new ProfileCommon();
                profileCommon.Initialize(username, isAuthenticated);

                SettingsContext settingsContext = new SettingsContext();
                settingsContext.Add("UserName", username);
                settingsContext.Add("IsAuthenticated", isAuthenticated);

                SettingsPropertyValueCollection pvc = profileProvider.GetPropertyValues(settingsContext, ProfileCommon.Properties);

                foreach (SettingsPropertyValue pv in pvc)
                {
                    //Only basic types and a List<String> are supported.
                    if (pv.PropertyValue != null && pv.Property.PropertyType == typeof(List<String>))
                    {
                        if (pv.PropertyValue is IEnumerable<String>)
                        {
                            profileCommon.SetPropertyValue(pv.Property.Name, new List<String>((IEnumerable<String>)pv.PropertyValue));
                        }
                        else
                        {
                            // Write an event. Assume a trace source 
                            new TraceSource("Default").TraceEvent(TraceEventType.Warning, -1, "Could not parse the property '{0}' in the ProfileCommon object.", pv.Property.Name);

                            // Something went wrong while setting this property.
                            System.Diagnostics.Debugger.Break();
                        }
                    }
                    else
                    {
                        profileCommon.SetPropertyValue(pv.Property.Name, pv.PropertyValue);
                    }
                }

                return profileCommon;
            }
            else
            {
                return ProfileBase.Create(username, isAuthenticated) as ProfileCommon;
            }
        }

        public static ProfileCommon GetProfile(string username)
        {
            // casts the result to ProfileCommon
            return Create(username) as ProfileCommon;

        }

    }
}

