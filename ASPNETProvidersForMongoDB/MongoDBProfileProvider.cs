﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration.Provider;
using System.Text;
using System.Diagnostics;
using System.Web.Security;
using System.Collections.Specialized;
using System.Web.Profile;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Configuration;

namespace ASPNETProvidersForMongoDB
{
    public class MongoDBProfileProvider : ProfileProvider
    {

        //
        // Global connection string, generic exception message, event log info.
        //

        private string eventSource = "MongoDBProfileProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the event log.";
        private string connectionString;


        //
        // If false, exceptions are thrown to the caller. If true,
        // exceptions are written to the event log.
        //

        private bool pWriteExceptionsToEventLog;
        /// <summary>
        /// Gets or sets whether to errors will be recorded in the Event Log.
        /// </summary>
        /// <remarks>
        /// If false, exceptions are thrown to the caller. If true,
        /// exceptions are written to the event log.
        /// 
        /// 
        /// Using Regedit.exe or another Windows registry editing tool, create the following registry key:
        /// HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Eventlog\Application\MongoDBMembershipProvider
        /// </remarks>
        public bool WriteExceptionsToEventLog
        {
            get { return pWriteExceptionsToEventLog; }
            set { pWriteExceptionsToEventLog = value; }
        }


        private string pMongoProviderDatabaseName;
        /// <summary>
        /// Gets the name of the Mongo database used to store the Provider data.
        /// </summary>
        public string MongoProviderDatabaseName
        {
            get { return pMongoProviderDatabaseName; }
        }

        private string pMongoProviderProfileCollectionName;
        /// <summary>
        /// Gets the name of the collection in the Mongo Database used to store the user data.
        /// </summary>
        public string MongoProviderProfileCollectionName
        {
            get { return pMongoProviderProfileCollectionName; }
        }



        //
        // A helper function to retrieve config values from the configuration file.
        //

        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }


        //
        // System.Configuration.Provider.ProviderBase.Initialize Method
        //

        public override void Initialize(string name, NameValueCollection config)
        {

            //
            // Initialize values from web.config.
            //

            if (config == null)
                throw new ArgumentNullException("config");

            if (name == null || name.Length == 0)
                name = "MongoDBProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Sample MongoDB Profile provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);


            if (config["applicationName"] == null || config["applicationName"].Trim() == "")
            {
                pApplicationName = System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath;
            }
            else
            {
                pApplicationName = config["applicationName"];
            }

            pMongoProviderDatabaseName = Convert.ToString(GetConfigValue(config["mongoProviderDatabaseName"], "ASPNetProviderDB"));
            pMongoProviderProfileCollectionName = Convert.ToString(GetConfigValue(config["mongoProviderUsersCollectionName"], "Users"));

            //
            // Initialize connection string.
            //
            
            ConnectionStringSettings pConnectionStringSettings = ConfigurationManager.
                ConnectionStrings[config["connectionStringName"]];

            if (pConnectionStringSettings == null ||
                pConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = pConnectionStringSettings.ConnectionString;
        }


        //
        // System.Configuration.SettingsProvider.ApplicationName
        //

        private string pApplicationName;

        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        public override int DeleteInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            int deleteCount = 0;

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> profiles = ProviderDB.GetCollection(pMongoProviderProfileCollectionName);


            try
            {
                var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                    Query.LTE("LastActivityDate", userInactiveSinceDate));

                switch (authenticationOption)
                {
                    case ProfileAuthenticationOption.Anonymous:
                        query = Query.And(query, Query.EQ("IsAnonymous", true));
                        break;
                    case ProfileAuthenticationOption.Authenticated:
                        query = Query.And(query, Query.EQ("IsAnonymous", false));
                        break;
                    default:
                        break;
                }

            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteInactiveProfiles");
                    throw new ProviderException(exceptionMessage, e);
                }
                else
                {
                    throw e;
                }
            }

            return deleteCount;
        }

        public override int DeleteProfiles(string[] usernames)
        {
            int deleteCount = 0;

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> profiles = ProviderDB.GetCollection(pMongoProviderProfileCollectionName);


            try
            {
             
                foreach (string user in usernames)
                {

                    var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                     Query.EQ("Username", user));

                    profiles.Remove(query, SafeMode.False);  // Makes it fast.

                    deleteCount++;
                }

            }
            catch (Exception e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteProfiles(String())");
                    throw new ProviderException(exceptionMessage, e);
                }
                else
                {
                    throw e;
                }
            }

            return deleteCount;
        }

        public override int DeleteProfiles(ProfileInfoCollection profiles)
        {
            List<string> usernames = new List<string>();
            foreach (ProfileInfo p in profiles)
            {
                usernames.Add(p.UserName);
            }

            return DeleteProfiles(usernames.ToArray());
        }

        public override ProfileInfoCollection FindInactiveProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            CheckParameters(pageIndex, pageSize);

            return GetProfileInfo(authenticationOption, usernameToMatch, userInactiveSinceDate,
                  pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection FindProfilesByUserName(ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            CheckParameters(pageIndex, pageSize);

            return GetProfileInfo(authenticationOption, usernameToMatch,
                null, pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection GetAllInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            CheckParameters(pageIndex, pageSize);

            return GetProfileInfo(authenticationOption, null, userInactiveSinceDate,
                  pageIndex, pageSize, out totalRecords);
        }

        public override ProfileInfoCollection GetAllProfiles(ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            CheckParameters(pageIndex, pageSize);

            return GetProfileInfo(authenticationOption, null, null,
                  pageIndex, pageSize, out totalRecords);
        }

        public override int GetNumberOfInactiveProfiles(ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
           
            int inactiveProfiles = 0;

            ProfileInfoCollection profiles =
              GetProfileInfo(authenticationOption, null, userInactiveSinceDate, 0, 0, out inactiveProfiles);

            return inactiveProfiles;

        }

        //
        // GetProfileInfo
        // Retrieves a count of profiles and creates a 
        // ProfileInfoCollection from the profile data in the 
        // database. Called by GetAllProfiles, GetAllInactiveProfiles,
        // FindProfilesByUserName, FindInactiveProfilesByUserName, 
        // and GetNumberOfInactiveProfiles.
        // Specifying a pageIndex of 0 retrieves a count of the results only.
        //

        private ProfileInfoCollection GetProfileInfo(
          ProfileAuthenticationOption authenticationOption,
          string usernameToMatch,
          object userInactiveSinceDate,
          int pageIndex,
          int pageSize,
          out int totalRecords)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> profiles = ProviderDB.GetCollection(pMongoProviderProfileCollectionName);


            var query = Query.EQ("ApplicationName", pApplicationName);
            

            // If searching for a user name to match, add the command text and parameters.
            
            if (usernameToMatch != null)
            {
                query = Query.And(query, Query.EQ("Username", usernameToMatch));
            }

            // If searching for inactive profiles, 
            // add the command text and parameters.

            if (userInactiveSinceDate != null)
            {
                query = Query.And(query, Query.LT("LastActivityDate", (DateTime)userInactiveSinceDate));
            }

            // If searching for a anonymous or authenticated profiles,    
            // add the command text and parameters.

            switch (authenticationOption)
            {
                case ProfileAuthenticationOption.Anonymous:
                    query = Query.And(query, Query.LT("IsAnonymous", true));
                    break;
                case ProfileAuthenticationOption.Authenticated:
                    query = Query.And(query, Query.LT("IsAnonymous", false));
                    break;
                default:
                    break;
            }


            ProfileInfoCollection profilesCollection = new ProfileInfoCollection();

            try
            {
                // Get the profile count.
                totalRecords =profiles.Count(query);
                
                // No profiles found.
                if (totalRecords == 0) { return profilesCollection; }
               
                // Count profiles only.
                if (pageSize == 0) { return profilesCollection; }

                var cursor = profiles.Find(query);
                cursor.SetFields(new string[] {"Username", "LastActivityDate", "LastUpdatedDate", "IsAnonymous"});
                cursor.Skip =  Math.Max(0, pageSize * (pageIndex - 1));
                cursor.Limit = pageSize;

                foreach(var profile in cursor)
                {
                    ProfileInfo p = GetProfileInfoFromReader(profile);
                    profilesCollection.Add(p);
                }

            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetProfileInfo");
                    throw new ProviderException(exceptionMessage, e);
                }
                else
                {
                    throw e;
                }
            }

            return profilesCollection;
        }


        //
        // CheckParameters
        // Verifies input parameters for page size and page index. 
        // Called by GetAllProfiles, GetAllInactiveProfiles, 
        // FindProfilesByUserName, and FindInactiveProfilesByUserName.
        //

        private void CheckParameters(int pageIndex, int pageSize)
        {
            if (pageIndex < 0)
                throw new ArgumentException("Page index must 0 or greater.");
            if (pageSize < 1)
                throw new ArgumentException("Page size must be greater than 0.");
        }

        //
        // GetProfileInfoFromReader
        //  Takes the current row from the MongoDB document
        // and populates a ProfileInfo object from the values. 
        //

        private ProfileInfo GetProfileInfoFromReader(BsonDocument profile)
        {
            string username = profile["Username"].AsString;

            DateTime lastActivityDate = new DateTime();
            if (profile["LastActivityDate"] != BsonNull.Value)
                lastActivityDate = profile["LastActivityDate"].AsDateTime;
           
            DateTime lastUpdatedDate = new DateTime();
            if (profile["LastUpdatedDate"] != BsonNull.Value)
                lastUpdatedDate = profile["LastUpdatedDate"].AsDateTime;

            bool isAnonymous = profile["IsAnonymous"].AsBoolean;

            ProfileInfo p = new ProfileInfo(username,
                isAnonymous, lastActivityDate, lastUpdatedDate, 0);

            return p;
        }


        public override System.Configuration.SettingsPropertyValueCollection GetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyCollection ppc)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> profiles = ProviderDB.GetCollection(pMongoProviderProfileCollectionName);


            string username = (string)context["UserName"];
            bool isAuthenticated = (bool)context["IsAuthenticated"];

            // The serializeAs attribute is ignored in this provider implementation.

            SettingsPropertyValueCollection svc = new SettingsPropertyValueCollection();

            try
            {
                foreach (SettingsProperty prop in ppc)
                {
                    SettingsPropertyValue pv = new SettingsPropertyValue(prop);

                    var query = Query.And(Query.EQ("Username", username),
                        Query.EQ("ApplicationName", pApplicationName),
                        Query.EQ("IsAnonymous", !isAuthenticated));

                    var profile = profiles.FindOne(query);

                    if (profile != null)
                    {
                        //prop.PropertyType;
                        var obj = profile[prop.Name];

                        object returnValue;
                        switch (obj.BsonType)
                        {
                            case BsonType.Binary:
                                returnValue = obj.AsByteArray;
                                break;
                            case BsonType.Boolean:
                                returnValue = obj.AsBoolean;
                                break;
                            case BsonType.DateTime:
                                returnValue = obj.AsDateTime;
                                break;
                            case BsonType.Double:
                                returnValue = obj.AsDouble;
                                break;
                            case BsonType.Int32:
                                returnValue = obj.AsInt32;
                                break;
                            case BsonType.Int64:
                                returnValue = obj.AsInt64;
                                break;
                            case BsonType.Null:
                                returnValue = null;
                                break;
                            case BsonType.String:
                                returnValue = obj.AsString;
                                break;
                            case BsonType.Undefined:
                                throw new ProviderException("Unsupported Property");
                            default:
                                goto case BsonType.Undefined;
                        }

                        pv.PropertyValue = returnValue;

                    }
                    svc.Add(pv);
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "AddUsersToRoles");
                }
            }

            UpdateActivityDates(username, isAuthenticated, true);

            return svc;
        }

        public override void SetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyValueCollection ppvc)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> profiles = ProviderDB.GetCollection(pMongoProviderProfileCollectionName);


            string username = (string)context["UserName"];
            bool isAuthenticated = (bool)context["IsAuthenticated"];

            // Create profile if does not exist.
            ObjectId existingProfileId = ObjectId.Empty;


            var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                Query.EQ("Username", username));

            var existingProfile = profiles.FindOne(query);

            if (existingProfile != null)
                existingProfileId = existingProfile["_id"].AsObjectId;

            var profile = new BsonDocument();

            profile.Add("ApplicationName", pApplicationName)
                .Add("Username", username)
                .Add("LastActivityDate", DateTime.Now)
                .Add("LastUpdatedDate", DateTime.Now)
                .Add("IsAnonymous", !isAuthenticated);

            if (existingProfileId != ObjectId.Empty)
                profile.Add("_id", existingProfileId);

            foreach (SettingsPropertyValue pv in ppvc)
            {
                profile.Add(pv.Name, BsonValue.Create(pv.PropertyValue));
            }

            bool bSuccess = profiles.Save(profile).Ok;
        }

        private void UpdateActivityDates(string username, bool isAuthenticated, bool activityOnly)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> profiles = ProviderDB.GetCollection(pMongoProviderProfileCollectionName);
            try
            {
                var query = Query.And(Query.EQ("Username", username),
                    Query.EQ("ApplicationName", pApplicationName),
                    Query.EQ("IsAnonymous", !isAuthenticated));

                var updateQuery = Update.Set("LastActivityDate", DateTime.Now);

                if (!activityOnly)
                {
                    updateQuery.Set("LastUpdatedDate", DateTime.Now);
                }

                profiles.Update(query, updateQuery, SafeMode.False);
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "AddUsersToRoles");
                }
            }

        }

        //
        // WriteToEventLog
        //   A helper function that writes exception detail to the event log. Exceptions
        // are written to the event log as a security measure to avoid private database
        // details from being returned to the browser. If a method does not return a status
        // or boolean indicating the action succeeded or failed, a generic exception is also 
        // thrown by the caller.
        //
        private void WriteToEventLog(Exception e, string action)
        {
            EventLog log = new EventLog();
            log.Source = eventSource;
            log.Log = eventLog;

            string message = "An exception occurred communicating with the data source.\n\n";
            message += "Action: " + action + "\n\n";
            message += "Exception: " + e.ToString();

            log.WriteEntry(message);
        }


    }
}
