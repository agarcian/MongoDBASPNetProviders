using System;
using System.Collections.Generic;
using System.Linq;
using System.Configuration.Provider;
using System.Text;
using System.Diagnostics;
using System.Web.Security;
using System.Collections.Specialized;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using System.Configuration;

namespace ASPNETProvidersForMongoDB
{
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
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";
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

            pMongoProviderDatabaseName = Convert.ToString(GetConfigValue(config["mongoProviderDatabaseName"], "ASPNetProviderDB"));
            pmongoProviderRolesCollectionName = Convert.ToString(GetConfigValue(config["mongoProviderCollectionName"], "Roles"));

            //
            // Initialize MongoDB ConnectionString.
            //

            ConnectionStringSettings ConnectionStringSettings =
              ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || ConnectionStringSettings.ConnectionString.Trim() == "")
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = ConnectionStringSettings.ConnectionString;

        }

        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            
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
                        BsonDocument role_user_duple = new BsonDocument().Add("Rolename", rolename)
                            .Add("Username", username)
                            .Add("ApplicationName", pApplicationName)
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
        
        public override void CreateRole(string rolename)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            
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
                BsonDocument role = new BsonDocument().Add("Rolename", rolename)
                            .Add("ApplicationName", pApplicationName)
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

        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            
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
                var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                    Query.EQ("Rolename", rolename),
                    Query.EQ("RecordType", RecordType.RoleDefinition.ToString()));

                bSuccess = roles.FindAndRemove(query, SortBy.Null).Ok;

                if (bSuccess)
                {
                    // Remove users associated to the role.
                    var query2 = Query.And(Query.EQ("ApplicationName", pApplicationName),
                    Query.EQ("Rolename", rolename),
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

        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            List<String> userNames = new List<string>();

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            
            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);

            try
            {
                
                var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                     Query.EQ("Rolename", rolename),
                     Query.EQ("RecordType", RecordType.RoleToUser.ToString()),
                     Query.Matches("Username", new BsonRegularExpression(usernameToMatch + "*", "i")) );

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

        public override string[] GetAllRoles()
        {
            List<String> roleNames = new List<string>();

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            
            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);




            try
            {

                var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
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

        public override string[] GetRolesForUser(string username)
        {

            List<String> roleNames = new List<string>();

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            
            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);


            if (username == null || username == "")
                throw new ProviderException("User name cannot be empty or null.");


            try
            {

                var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                     Query.EQ("Username", username),
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

        public override string[] GetUsersInRole(string roleName)
        {
            List<String> userNames = new List<string>();

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);

            if (roleName == null || roleName == "")
                throw new ProviderException("Role name cannot be empty or null.");
            if (!RoleExists(roleName))
                throw new ProviderException("Role does not exist.");

            try
            {
                var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                     Query.EQ("Rolename", roleName),
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

        public override bool IsUserInRole(string username, string roleName)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);


            if (username == null || username == "")
                throw new ProviderException("User name cannot be empty or null.");
            if (roleName == null || roleName == "")
                throw new ProviderException("Role name cannot be empty or null.");

            bool isUserinRole = false;
            try
            {
                var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                     Query.EQ("Rolename", roleName),
                     Query.EQ("Username", username),
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

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);

            foreach (string rolename in roleNames)
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
                        var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                            Query.EQ("Username", username),
                            Query.EQ("Rolename", rolename),
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

        public override bool RoleExists(string roleName)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            MongoCollection<BsonDocument> roles = ProviderDB.GetCollection(pmongoProviderRolesCollectionName);

            try
            {
                var query = Query.And(Query.EQ("ApplicationName", pApplicationName),
                     Query.EQ("Rolename", roleName),
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
