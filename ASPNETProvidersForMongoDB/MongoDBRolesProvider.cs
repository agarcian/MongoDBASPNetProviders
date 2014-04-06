using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Web.Security;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace ASPNETProvidersForMongoDB
{
    /// <summary>
    /// Implementation of the ASPNet Roles Provider using MongoDB.
    /// </summary>
    public class MongoDBRolesProvider : RoleProvider 
    {

        private enum RecordType
        {
            RoleToUser,
            RoleDefinition
        }


        //
        // Global connection string, generated password length, generic exception message, event log info.
        //

        private string eventSource = "MongoDBRolesProvider";
        private string eventLog = "Application";
        //private string exceptionMessage = "An exception occurred. Please check the Event Log.";
        private string connectionString;

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

        private string pApplicationName;
        /// <summary>
        /// Gets or sets the name of the application to store and retrieve role information for.
        /// </summary>
        /// <returns>The name of the application to store and retrieve role information for.</returns>
        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        private string pMongoProviderDatabaseName;
        /// <summary>
        /// Gets the name of the Mongo database used to store the Provider data.
        /// </summary>
        public string MongoProviderDatabaseName
        {
            get { return pMongoProviderDatabaseName; }
        }

        private string pmongoProviderRolesCollectionName;
        /// <summary>
        /// Gets the name of the collection in the Mongo Database used to store the user data.
        /// </summary>
        public string mongoProviderCollectionName
        {
            get { return pmongoProviderRolesCollectionName; }
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

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        ///   
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        ///   
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"/> on a provider after the provider has already been initialized.</exception>
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
                config.Add("description", "Sample MongoDB Roles provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            pApplicationName = GetConfigValue(config["applicationName"],
                                            System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            pWriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));

            if (String.IsNullOrWhiteSpace(config["mongoProviderDatabaseName"]))
            {
                throw new ProviderException("mongoProviderDatabaseName is not defined in the web.config under the roles section.");
            }
            else
            {
                pMongoProviderDatabaseName = config["mongoProviderDatabaseName"];
            }
            pmongoProviderRolesCollectionName = Convert.ToString(GetConfigValue(config["mongoProviderCollectionName"], "Roles"));

            //
            // Initialize MongoDB ConnectionString.
            //

            ConnectionStringSettings ConnectionStringSettings =
              ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || String.IsNullOrWhiteSpace(ConnectionStringSettings.ConnectionString))
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = ConnectionStringSettings.ConnectionString;

        }

        /// <summary>
        /// Adds the users to roles.
        /// </summary>
        /// <param name="usernames">The usernames.</param>
        /// <param name="rolenames">The rolenames.</param>
        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);
            
            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);


            foreach (string rolename in rolenames)
            {
                if (rolename == null || rolename == "")
                    throw new ProviderException("Role name cannot be empty or null.");
                
                if (!RoleExists(rolename))
                    throw new ProviderException("Role name not found.");
            }

            foreach (string username in usernames)
            {
                if (username == null || username == "")
                    throw new ProviderException("User name cannot be empty or null.");
               
                if (username.Contains(","))
                    throw new ArgumentException("User names cannot contain commas.");

                foreach (string rolename in rolenames)
                {
                    if (IsUserInRole(username, rolename))
                        throw new ProviderException("User is already in role.");
                }
            }

            try
            {
                foreach (string username in usernames)
                {
                    foreach (string rolename in rolenames)
                    {
                        BsonDocument role_user_duple = new BsonDocument()
                            .Add("Rolename", rolename)
                            .Add("Username", username.ToLower())
                            .Add("ApplicationName", pApplicationName)
                            .Add("RolenameLowerCase", rolename.ToLower())
                            .Add("UsernameLowerCase", username)
                            .Add("ApplicationNameLowerCase", pApplicationName.ToLower())
                            .Add("RecordType", RecordType.RoleToUser.ToString());  // We will be using the same table.  Add a record type to store everything in the same table.

                        bool bSuccess = roles.Save(role_user_duple).Ok;

                        if (!bSuccess)
                            throw new ProviderException(String.Format("Failed to associate user '{0}' to role '{1}'", username, rolename));
                    }
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "AddUsersToRoles");
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Creates the role.
        /// </summary>
        /// <param name="rolename">The rolename.</param>
        public override void CreateRole(string rolename)
        {
            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);
 

            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);

           
            if (rolename == null || rolename == "")
                throw new ProviderException("Role name cannot be empty or null.");
            if (rolename.Contains(","))
                throw new ArgumentException("Role names cannot contain commas.");
            if (RoleExists(rolename))
                throw new ProviderException("Role name already exists.");
            if (rolename.Length > 255)
                throw new ProviderException("Role name cannot exceed 255 characters.");

            try
            {
                BsonDocument role = new BsonDocument()
                            .Add("Rolename", rolename)
                            .Add("ApplicationName", pApplicationName)
                            .Add("RolenameLowerCase", rolename.ToLower())
                            .Add("ApplicationNameLowerCase", pApplicationName.ToLower())
                            .Add("RecordType", RecordType.RoleDefinition.ToString());  // We will be using the same table.  Add a record type to store everything in the same table.

                bool bSuccess = roles.Save(role).Ok;

                if (!bSuccess) 
                    throw new ProviderException(String.Format("Failed to createnew role '{0}'", rolename));
                  
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "CreateRole");
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Deletes the role.
        /// </summary>
        /// <param name="rolename">The rolename.</param>
        /// <param name="throwOnPopulatedRole">if set to <c>true</c> [throw on populated role].</param>
        /// <returns></returns>
        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);
            
            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);
            
            if (!RoleExists(rolename))
            {
                throw new ProviderException("Role does not exist.");
            }

            if (throwOnPopulatedRole && GetUsersInRole(rolename).Length > 0)
            {
                throw new ProviderException("Cannot delete a populated role.");
            }

            bool bSuccess = false;
            try
            {
                // Remove the role.
                var query = Query.And(Query.EQ("ApplicationNameLowerCase", pApplicationName.ToLower()),
                    Query.EQ("RolenameLowerCase", rolename.ToLower()),
                    Query.EQ("RecordType", RecordType.RoleDefinition.ToString()));

                FindAndRemoveArgs args = new FindAndRemoveArgs();
                args.Query = query;
                args.SortBy = SortBy.Null;
                bSuccess = roles.FindAndRemove(args).Ok;

                if (bSuccess)
                {
                    // Remove users associated to the role.
                    var query2 = Query.And(Query.EQ("ApplicationNameLowerCase", pApplicationName.ToLower()),
                    Query.EQ("RolenameLowerCase", rolename.ToLower()),
                    Query.EQ("RecordType", RecordType.RoleToUser.ToString()));

                    bSuccess = roles.Remove(query2).Ok;
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteRole");
                }
            }
            finally
            {
            }

            return bSuccess;
        }

        /// <summary>
        /// Finds the users in role.
        /// </summary>
        /// <param name="rolename">The rolename.</param>
        /// <param name="usernameToMatch">The username to match.</param>
        /// <returns></returns>
        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            List<String> userNames = new List<string>();

            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);
            
            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);

            try
            {

                var query = Query.And(Query.EQ("ApplicationNameLowerCase", pApplicationName.ToLower()),
                     Query.EQ("RolenameLowerCase", rolename.ToLower()),
                     Query.EQ("RecordType", RecordType.RoleToUser.ToString()),
                     Query.Matches("UsernameLowerCase", new BsonRegularExpression(usernameToMatch.ToLower() + "*", "i")) );

                var cursor = roles.Find(query);
                
                foreach(var role in cursor)
                {
                    userNames.Add(role["Username"].AsString);
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "FindUsersInRole");
                }
            }
            finally
            {
            }

                
            return userNames.ToArray();
        }

        /// <summary>
        /// Gets a list of all the roles for the configured applicationName.
        /// </summary>
        /// <returns>
        /// A string array containing the names of all the roles stored in the data source for the configured applicationName.
        /// </returns>
        public override string[] GetAllRoles()
        {
            List<String> roleNames = new List<string>();

            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);
            
            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);




            try
            {

                var query = Query.And(Query.EQ("ApplicationNameLowerCase", pApplicationName.ToLower()),
                     Query.EQ("RecordType", RecordType.RoleDefinition.ToString()));

                var cursor = roles.Find(query);
                cursor.SetFields(new string[] { "Rolename" });
                
                foreach (var role in cursor)
                {
                    roleNames.Add(role["Rolename"].AsString);
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetAllRoles");
                }
            }
            finally
            {
            }

            return roleNames.ToArray();
        }

        /// <summary>
        /// Gets a list of the roles that a specified user is in for the configured applicationName.
        /// </summary>
        /// <param name="username">The user to return a list of roles for.</param>
        /// <returns>
        /// A string array containing the names of all the roles that the specified user is in for the configured applicationName.
        /// </returns>
        public override string[] GetRolesForUser(string username)
        {

            List<String> roleNames = new List<string>();

            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);
            
            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);


            if (String.IsNullOrWhiteSpace(username))
            {
                throw new ProviderException("User name cannot be empty or null.");
            }

            try
            {

                var query = Query.And(Query.EQ("ApplicationNameLowerCase", pApplicationName.ToLower()),
                     Query.EQ("UsernameLowerCase", username.ToLower()),
                     Query.EQ("RecordType", RecordType.RoleToUser.ToString()));

                var cursor = roles.Find(query);

                foreach (var role in cursor)
                {
                    roleNames.Add(role["Rolename"].AsString);
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetRolesForUser");
                }
            }
            finally
            {
            }

                
            return roleNames.ToArray();

        }

        /// <summary>
        /// Gets a list of users in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to get the list of users for.</param>
        /// <returns>
        /// A string array containing the names of all the users who are members of the specified role for the configured applicationName.
        /// </returns>
        public override string[] GetUsersInRole(string roleName)
        {
            List<String> userNames = new List<string>();

            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);

            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);

            if (String.IsNullOrWhiteSpace(roleName))
                throw new ProviderException("Role name cannot be empty or null.");
            
            if (!RoleExists(roleName))
                throw new ProviderException("Role does not exist.");

            try
            {
                var query = Query.And(Query.EQ("ApplicationNameLowerCase", pApplicationName.ToLower()),
                     Query.EQ("RolenameLowerCase", roleName.ToLower()),
                     Query.EQ("RecordType", RecordType.RoleToUser.ToString()));

                var cursor = roles.Find(query);

                foreach (var role in cursor)
                {
                    userNames.Add(role["Username"].AsString);
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUsersInRole");
                }
            }
            finally
            {
            }

                
            return userNames.ToArray();
        }

        /// <summary>
        /// Gets a value indicating whether the specified user is in the specified role for the configured applicationName.
        /// </summary>
        /// <param name="username">The user name to search for.</param>
        /// <param name="roleName">The role to search in.</param>
        /// <returns>
        /// true if the specified user is in the specified role for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool IsUserInRole(string username, string roleName)
        {
            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);


            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);


            if (username == null || username == "")
                throw new ProviderException("User name cannot be empty or null.");
            if (roleName == null || roleName == "")
                throw new ProviderException("Role name cannot be empty or null.");

            bool isUserinRole = false;
            try
            {
                var query = Query.And(Query.EQ("ApplicationNameLowerCase", pApplicationName.ToLower()),
                     Query.EQ("RolenameLowerCase", roleName.ToLower()),
                     Query.EQ("UsernameLowerCase", username.ToLower()),
                     Query.EQ("RecordType", RecordType.RoleToUser.ToString()));

                int count = (int) roles.Count(query);

                isUserinRole = count > 0;
               
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "IsUserInRole");
                }
            }
            finally
            {
            }

            return isUserinRole;
        }

        /// <summary>
        /// Removes the specified user names from the specified roles for the configured applicationName.
        /// </summary>
        /// <param name="usernames">A string array of user names to be removed from the specified roles.</param>
        /// <param name="roleNames">A string array of role names to remove the specified user names from.</param>
        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);

            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);

            foreach (string rolename in roleNames)
            {
                if (String.IsNullOrWhiteSpace(rolename))
                    throw new ProviderException("Role name cannot be empty or null.");
                if (!RoleExists(rolename))
                    throw new ProviderException("Role name not found.");
            }

            foreach (string username in usernames)
            {
                if (String.IsNullOrWhiteSpace(username))
                    throw new ProviderException("User name cannot be empty or null.");

                foreach (string rolename in roleNames)
                {
                    if (!IsUserInRole(username, rolename))
                        throw new ProviderException("User is not in role.");
                }
            }


            try
            {
                foreach (string username in usernames)
                {
                    foreach (string rolename in roleNames)
                    {
                        var query = Query.And(Query.EQ("ApplicationNameLowerCase", pApplicationName.ToLower()),
                            Query.EQ("UsernameLowerCase", username.ToLower()),
                            Query.EQ("RolenameLowerCase", rolename.ToLower()),
                            Query.EQ("RecordType", RecordType.RoleToUser.ToString()));

                       bool bSuccess = roles.Remove(query).Ok;

                       if (!bSuccess)
                           throw new ApplicationException(String.Format("Failed removing user '{0}' from role '{1}'", username, rolename));

                    }
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "RemoveUsersFromRoles");
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Gets a value indicating whether the specified role name already exists in the role data source for the configured applicationName.
        /// </summary>
        /// <param name="roleName">The name of the role to search for in the data source.</param>
        /// <returns>
        /// true if the role name already exists in the data source for the configured applicationName; otherwise, false.
        /// </returns>
        public override bool RoleExists(string roleName)
        {
            MongoClient client = new MongoClient(connectionString);
            MongoServer server = client.GetServer(); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);

            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);

            try
            {
                var query = Query.And(Query.EQ("ApplicationNameLowerCase", pApplicationName.ToLower()),
                     Query.EQ("RolenameLowerCase", roleName.ToLower()),
                     Query.EQ("RecordType", RecordType.RoleDefinition.ToString()));

                int count = (int) roles.Count(query);

                return count > 0;

            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "RoleExists");
                }
            }
            finally
            {
            }

            return false;
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
