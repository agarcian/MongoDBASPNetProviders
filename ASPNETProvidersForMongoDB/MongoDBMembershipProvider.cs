using System.Web.Security;
using System.Configuration.Provider;
using System.Collections.Specialized;
using System;
using System.Data;
using System.Data.Odbc;
using System.Configuration;
using System.Diagnostics;
using System.Web;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Web.Configuration;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Driver.Builders;
using System.IO;

namespace ASPNETProvidersForMongoDB
{
    public sealed class MongoDBMembershipProvider : MembershipProvider
    {
        #region private fields
        private int newPasswordLength = 8;
        private string eventSource = "MongoDBMembershipProvider";
        private string eventLog = "Application";
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";
        private string connectionString;
        /// <summary>
        /// Used when determining encryption key values.
        /// </summary>
        private MachineKeySection machineKey;
        private bool pWriteExceptionsToEventLog;
        private string pApplicationName;
        private bool pEnablePasswordReset;
        private bool pEnablePasswordRetrieval;
        private bool pRequiresQuestionAndAnswer;
        private bool pRequiresUniqueEmail;
        private int pMaxInvalidPasswordAttempts;
        private int pPasswordAttemptWindow;
        private MembershipPasswordFormat pPasswordFormat;
        private int pMinRequiredNonAlphanumericCharacters;
        private int pMinRequiredPasswordLength;
        private string pPasswordStrengthRegularExpression;
        private string pMongoProviderDatabaseName;
        private string pMongoProviderUsersCollectionName;
        #endregion

        #region Properties
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

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName
        {
            get { return pApplicationName; }
            set { pApplicationName = value; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
        public override bool EnablePasswordReset
        {
            get { return pEnablePasswordReset; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.</returns>
        public override bool EnablePasswordRetrieval
        {
            get { return pEnablePasswordRetrieval; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>
        public override bool RequiresQuestionAndAnswer
        {
            get { return pRequiresQuestionAndAnswer; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>
        public override bool RequiresUniqueEmail
        {
            get { return pRequiresUniqueEmail; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>
        public override int MaxInvalidPasswordAttempts
        {
            get { return pMaxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>
        public override int PasswordAttemptWindow
        {
            get { return pPasswordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"/> values indicating the format for storing passwords in the data store.</returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return pPasswordFormat; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <returns>The minimum number of special characters that must be present in a valid password.</returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return pMinRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <returns>The minimum length required for a password. </returns>
        public override int MinRequiredPasswordLength
        {
            get { return pMinRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <returns>A regular expression used to evaluate a password.</returns>
        public override string PasswordStrengthRegularExpression
        {
            get { return pPasswordStrengthRegularExpression; }
        }
        
        /// <summary>
        /// Gets the name of the Mongo database used to store the Provider data.
        /// </summary>
        public string MongoProviderDatabaseName
        {
            get { return pMongoProviderDatabaseName; }
        }

        /// <summary>
        /// Gets the name of the collection in the Mongo Database used to store the user data.
        /// </summary>
        public string MongoProviderUsersCollectionName
        {
            get { return pMongoProviderUsersCollectionName; }
        }

        /// <summary>
        /// Changes the password.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="oldPwd">The old PWD.</param>
        /// <param name="newPwd">The new PWD.</param>
        /// <returns></returns>
        public override bool ChangePassword(string username, string oldPwd, string newPwd)
        {
            if (!ValidateUser(username, oldPwd))
                return false;

            ValidatePasswordEventArgs args = new ValidatePasswordEventArgs(username, newPwd, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Change password canceled due to new password validation failure.");

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);
            var query = Query.And(
                Query.EQ("Username", username),
                Query.EQ("ApplicationName", pApplicationName)
            );
            var update = Update
                .Set("Password", EncodePassword(newPwd))
                .Set("LastPasswordChangedDate", DateTime.Now);
           
            bool bSuccess = false;
            try
            {
                bSuccess = users.Update(query, update).UpdatedExisting;

            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ChangePassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            if (bSuccess)
            {
                return true;
            }

            return false;
        }
        #endregion

        
        
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
                name = "MongoDBMembershipProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Sample MongoDB Membership provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            pApplicationName = GetConfigValue(config["applicationName"],
                                            System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            pMaxInvalidPasswordAttempts = Convert.ToInt32(GetConfigValue(config["maxInvalidPasswordAttempts"], "5"));
            pPasswordAttemptWindow = Convert.ToInt32(GetConfigValue(config["passwordAttemptWindow"], "10"));
            pMinRequiredNonAlphanumericCharacters = Convert.ToInt32(GetConfigValue(config["minRequiredNonAlphanumericCharacters"], "1"));
            pMinRequiredPasswordLength = Convert.ToInt32(GetConfigValue(config["minRequiredPasswordLength"], "7"));
            pPasswordStrengthRegularExpression = Convert.ToString(GetConfigValue(config["passwordStrengthRegularExpression"], ""));
            pEnablePasswordReset = Convert.ToBoolean(GetConfigValue(config["enablePasswordReset"], "true"));
            pEnablePasswordRetrieval = Convert.ToBoolean(GetConfigValue(config["enablePasswordRetrieval"], "true"));
            pRequiresQuestionAndAnswer = Convert.ToBoolean(GetConfigValue(config["requiresQuestionAndAnswer"], "false"));
            pRequiresUniqueEmail = Convert.ToBoolean(GetConfigValue(config["requiresUniqueEmail"], "true"));
            pWriteExceptionsToEventLog = Convert.ToBoolean(GetConfigValue(config["writeExceptionsToEventLog"], "true"));


            pMongoProviderDatabaseName = Convert.ToString(GetConfigValue(config["mongoProviderDatabaseName"], "ASPNetProviderDB"));
            pMongoProviderUsersCollectionName = Convert.ToString(GetConfigValue(config["mongoProviderUsersCollectionName"], "Users"));

            string temp_format = config["passwordFormat"];
            if (temp_format == null)
            {
                temp_format = "Hashed";
            }

            switch (temp_format)
            {
                case "Hashed":
                    pPasswordFormat = MembershipPasswordFormat.Hashed;
                    break;
                case "Encrypted":
                    pPasswordFormat = MembershipPasswordFormat.Encrypted;
                    break;
                case "Clear":
                    pPasswordFormat = MembershipPasswordFormat.Clear;
                    break;
                default:
                    throw new ProviderException("Password format not supported.");
            }



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


            Configuration cfg = null;


            if (Directory.Exists(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath))
                // When running from a web app
                cfg = WebConfigurationManager.OpenWebConfiguration(System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);
            else
                // when running from a test case.
                cfg = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            // Get encryption and decryption key information from the configuration.
            machineKey = (MachineKeySection)cfg.GetSection("system.web/machineKey");

            if (machineKey.ValidationKey.Contains("AutoGenerate"))
                if (PasswordFormat != MembershipPasswordFormat.Clear)
                    throw new ProviderException("Hashed or Encrypted passwords " +
                                                "are not supported with auto-generated keys.");
        }

        /// <summary>
        ///  A helper function to retrieve config values from the configuration file.
        /// </summary>
        /// <param name="configValue">The config value.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns></returns>
        private string GetConfigValue(string configValue, string defaultValue)
        {
            if (String.IsNullOrEmpty(configValue))
                return defaultValue;

            return configValue;
        }

        /// <summary>
        /// Changes the password question and answer.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="newPwdQuestion">The new PWD question.</param>
        /// <param name="newPwdAnswer">The new PWD answer.</param>
        /// <returns></returns>
        public override bool ChangePasswordQuestionAndAnswer(string username,
                      string password,
                      string newPwdQuestion,
                      string newPwdAnswer)
        {
            if (!ValidateUser(username, password))
                return false;

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);
            var query = Query.And(
                Query.EQ("Username", username),
                Query.EQ("ApplicationName", pApplicationName)
            );
            var update = Update
                .Set("PasswordQuestion", EncodePassword(newPwdQuestion))
                .Set("PasswordAnswer", EncodePassword(newPwdAnswer));


            bool bSuccess = false; 
            
            try
            {
                bSuccess = users.Update(query, update).UpdatedExisting; 
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ChangePasswordQuestionAndAnswer");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {

            }

            if (bSuccess)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"/> enumeration value indicating whether the user was created successfully.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the information for the newly created user.
        /// </returns>
        public override MembershipUser CreateUser(string username,
                 string password,
                 string email,
                 string passwordQuestion,
                 string passwordAnswer,
                 bool isApproved,
                 object providerUserKey,
                 out MembershipCreateStatus status)
        {
            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, password, true);

            OnValidatingPassword(args);

            if (args.Cancel)
            {
                status = MembershipCreateStatus.InvalidPassword;
                return null;
            }



            if (RequiresUniqueEmail && GetUserNameByEmail(email) != "")
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            MembershipUser u = GetUser(username, false);

            if (u == null)
            {
                DateTime createDate = DateTime.Now;

                if (providerUserKey == null)
                {
                    providerUserKey = Guid.NewGuid();
                }
                else
                {
                    if (!(providerUserKey is Guid))
                    {
                        status = MembershipCreateStatus.InvalidProviderUserKey;
                        return null;
                    }
                }


                MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
                MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

                //Build a query to find the user id and then update with new password.
                MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);
                
                bool bSuccess = false;

                try
                {
                    BsonDocument user = new BsonDocument()
                        .Add("PKID", providerUserKey.ToString())
                        .Add("Username", username)
                        .Add("Password", EncodePassword(password))
                        .Add("Email", email)
                        .Add("PasswordQuestion", passwordQuestion)
                        .Add("PasswordAnswer", EncodePassword(passwordAnswer))
                        .Add("IsApproved", isApproved)
                        .Add("Comment", "")
                        .Add("CreationDate", createDate)
                        .Add("LastPasswordChangedDate", createDate)
                        .Add("LastLoginDate", createDate)
                        .Add("LastActivityDate", createDate)
                        .Add("ApplicationName", pApplicationName)
                        .Add("IsLockedOut", false)
                        .Add("LastLockedOutDate", createDate)
                        .Add("FailedPasswordAttemptCount", 0)
                        .Add("FailedPasswordAttemptWindowStart", createDate)
                        .Add("FailedPasswordAnswerAttemptCount", createDate)
                        .Add("FailedPasswordAnswerAttemptWindowStart", createDate);

                    bSuccess = users.Save(user).Ok;

                    if (bSuccess)
                    {
                        status = MembershipCreateStatus.Success;
                    }
                    else
                    {
                        status = MembershipCreateStatus.UserRejected;
                    }
                }
                catch (ApplicationException e)
                {
                    if (WriteExceptionsToEventLog)
                    {
                        WriteToEventLog(e, "CreateUser");
                    }

                    status = MembershipCreateStatus.ProviderError;
                }
                finally
                {
                }


                return GetUser(username, false);
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
            }


            return null;
        }

        /// <summary>
        /// Removes a user from the membership data source.
        /// </summary>
        /// <param name="username">The name of the user to delete.</param>
        /// <param name="deleteAllRelatedData">true to delete data related to the user from the database; false to leave data related to the user in the database.</param>
        /// <returns>
        /// true if the user was successfully deleted; otherwise, false.
        /// </returns>
        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);

            var query = Query.And(
               Query.EQ("Username", username),
               Query.EQ("ApplicationName", pApplicationName)
            );

            var sortBy = SortBy.Ascending("Username");


            bool bSuccess = false;
            
            try
            {
                bSuccess = users.FindAndRemove(query, sortBy).Ok;

                if (deleteAllRelatedData)
                {
                    // Process commands to delete all data for the user in the database.
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "DeleteUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            if (bSuccess)
                return true;

            return false;
        }

        /// <summary>
        /// Gets a collection of all the users in the data source in pages of data.
        /// </summary>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection usersCollection = new MembershipUserCollection();

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);

            var query = Query.EQ("ApplicationName", ApplicationName);
            var cursor = users.Find(query).SetSortOrder(new string[] { "Username" });
            cursor.Skip = Math.Max(0, pageSize * (pageIndex - 1));
            cursor.SetFields(new string[] {"PKID", "Username", "Email", "PasswordQuestion", "Comment", "IsApproved", "IsLockedOut", "CreationDate", "LastLoginDate", "LastActivityDate", "LastPasswordChangedDate", "LastLockedOutDate" });
            cursor.Limit = pageSize;

            try
            {
                totalRecords = cursor.Count();

                if (totalRecords <= 0) { return usersCollection; }

                foreach (var user in cursor)
                {
                    MembershipUser u = GetUserFromReader(user);
                    usersCollection.Add(u);
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetAllUsers ");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            return usersCollection;
        }

        /// <summary>
        /// Gets the number of users currently accessing the application.
        /// </summary>
        /// <returns>
        /// The number of users currently accessing the application.
        /// </returns>
        public override int GetNumberOfUsersOnline()
        {

            TimeSpan onlineSpan = new TimeSpan(0, Membership.UserIsOnlineTimeWindow, 0);
            DateTime compareTime = DateTime.Now.Subtract(onlineSpan);


            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);


            int numOnline = 0;
            try
            {

                var query = Query.And(
                    Query.EQ("ApplicationName", pApplicationName),
                    Query.GTE("LastActivityDate", compareTime)
                );


            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetNumberOfUsersOnline");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            return numOnline;
        }

        /// <summary>
        /// Gets the password for the specified user name from the data source.
        /// </summary>
        /// <param name="username">The user to retrieve the password for.</param>
        /// <param name="answer">The password answer for the user.</param>
        /// <returns>
        /// The password for the specified user name.
        /// </returns>
        public override string GetPassword(string username, string answer)
        {

            if (!EnablePasswordRetrieval)
            {
                throw new ProviderException("Password Retrieval Not Enabled.");
            }

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                throw new ProviderException("Cannot retrieve Hashed passwords.");
            }

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);

            string password = "";
            string passwordAnswer = "";
           
            try
            {

                var query = Query.And(
                    Query.EQ("ApplicationName", pApplicationName),
                    Query.GTE("Username", username)
                );

                var user = users.FindOne(query);
               
                
                if (user != null)
                {
                        if (user["sLockedOut"].AsBoolean)
                        {
                            throw new MembershipPasswordException("The supplied user is locked out.");
                        }
                    
                        password = user["Password"].AsString;
                        passwordAnswer = user["Password"].AsString;

                }
                else
                {
                    throw new MembershipPasswordException("The supplied user name is not found.");
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetPassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }


            if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new MembershipPasswordException("Incorrect password answer.");
            }


            if (PasswordFormat == MembershipPasswordFormat.Encrypted)
            {
                password = UnEncodePassword(password);
            }

            return password;
        }

        /// <summary>
        /// Gets information from the data source for a user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="username">The name of the user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(string username, bool userIsOnline)
        {

            MembershipUser u = null;

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);
            try
            {

                var query = Query.And(
                    Query.EQ("ApplicationName", ApplicationName),
                    Query.EQ("Username", username)
                    );

                var user = users.FindOne(query);

                if (user != null)
                {
                    if (user["IsLockedOut"].AsBoolean)
                    {
                        throw new MembershipPasswordException("The supplied user is locked out.");
                    }

                    u = GetUserFromReader(user);

                    if (userIsOnline)
                    {
                        // Updates the lastactivitydate for the user.
                        var updateQuery = Query.And(
                            Query.EQ("ApplicationName", ApplicationName),
                            Query.EQ("Username", username));

                        var update = Update.Set("LastActivityDate", DateTime.Now);

                        users.Update(updateQuery, update);
                    }
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUser(String, Boolean)");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            return u;
        }

        /// <summary>
        /// Gets user information from the data source based on the unique identifier for the membership user. Provides an option to update the last-activity date/time stamp for the user.
        /// </summary>
        /// <param name="providerUserKey">The unique identifier for the membership user to get information for.</param>
        /// <param name="userIsOnline">true to update the last-activity date/time stamp for the user; false to return user information without updating the last-activity date/time stamp for the user.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"/> object populated with the specified user's information from the data source.
        /// </returns>
        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {

            MembershipUser u = null;

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);
            try
            {

                var query = Query.EQ("PKID", BsonValue.Create(providerUserKey));

                var user = users.FindOne(query);

                if (user != null)
                {
                    if (user["sLockedOut"].AsBoolean)
                    {
                        throw new MembershipPasswordException("The supplied user is locked out.");
                    }

                    u = GetUserFromReader(user);

                    if (userIsOnline)
                    {
                        // Updates the lastactivitydate for the user.
                        var updateQuery = Query.EQ("ApplicationName", ApplicationName);
                        var update = Update.Set("LastActivityDate", DateTime.Now);

                        users.Update(updateQuery, update);
                    }

                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUser(Object, Boolean)");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            return u;
        }

        /// <summary>
        /// Gets the user from reader.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <returns></returns>
        private MembershipUser GetUserFromReader(BsonDocument doc)
        {
            object providerUserKey = Guid.Parse(doc["PKID"].AsString);
            string username = doc["Username"].AsString;
            string email = doc["Email"].AsString; ;
            string passwordQuestion = doc.Contains("PasswordQuestion") ? doc["PasswordQuestion"].AsString : null;
            string comment = doc.Contains("Comment") ? doc["Comment"].AsString : null;

            bool isApproved = doc["IsApproved"].AsBoolean;
            bool isLockedOut = doc["IsLockedOut"].AsBoolean;
            DateTime creationDate = doc["CreationDate"].AsDateTime;
            DateTime lastLoginDate = doc["LastLoginDate"].AsDateTime;
            DateTime lastActivityDate = doc["LastActivityDate"].AsDateTime;
            DateTime lastPasswordChangedDate = doc["LastPasswordChangedDate"].AsDateTime;
            DateTime lastLockedOutDate = doc["LastLockedOutDate"].AsDateTime;

            MembershipUser u = new MembershipUser(this.Name,
                                                  username,
                                                  providerUserKey,
                                                  email,
                                                  passwordQuestion,
                                                  comment,
                                                  isApproved,
                                                  isLockedOut,
                                                  creationDate,
                                                  lastLoginDate,
                                                  lastActivityDate,
                                                  lastPasswordChangedDate,
                                                  lastLockedOutDate);

            return u;
        }

        /// <summary>
        /// Unlocks the user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns></returns>
        public override bool UnlockUser(string username)
        {

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);


            bool bSuccess = false;

            try
            {

            var updateQuery = Query.And(
                Query.EQ("Username", username),
                Query.EQ("ApplicationName", ApplicationName));

            var update = Update.Set("IsLockedOut", false)
                .Set("LastLockedOutDate", DateTime.Now);

            bSuccess = users.Update(updateQuery, update).Ok;

            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UnlockUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            if (bSuccess)
                return true;

            return false;
        }

        /// <summary>
        /// Gets the user name associated with the specified e-mail address.
        /// </summary>
        /// <param name="email">The e-mail address to search for.</param>
        /// <returns>
        /// The user name associated with the specified e-mail address. If no match is found, return null.
        /// </returns>
        public override string GetUserNameByEmail(string email)
        {

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);

            string username = "";

            try
            {
                var query = Query.And(
                Query.EQ("Email", email),
                Query.EQ("ApplicationName", ApplicationName));

                var usr = users.FindOne(query);

                if (usr != null)
                    username = usr["Username"].AsString;
                
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "GetUserNameByEmail");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            if (username == null)
                username = "";

            return username;
        }

        /// <summary>
        /// Resets a user's password to a new, automatically generated password.
        /// </summary>
        /// <param name="username">The user to reset the password for.</param>
        /// <param name="answer">The password answer for the specified user.</param>
        /// <returns>
        /// The new password for the specified user.
        /// </returns>
        public override string ResetPassword(string username, string answer)
        {
            if (!EnablePasswordReset)
            {
                throw new NotSupportedException("Password reset is not enabled.");
            }

            if (answer == null && RequiresQuestionAndAnswer)
            {
                UpdateFailureCount(username, "passwordAnswer");

                throw new ProviderException("Password answer required for password reset.");
            }

            string newPassword =
              System.Web.Security.Membership.GeneratePassword(newPasswordLength, MinRequiredNonAlphanumericCharacters);


            ValidatePasswordEventArgs args =
              new ValidatePasswordEventArgs(username, newPassword, true);

            OnValidatingPassword(args);

            if (args.Cancel)
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                else
                    throw new MembershipPasswordException("Reset password canceled due to password validation failure.");

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);


            string passwordAnswer = "";
            bool bSuccess = false;

            try
            {
                var query = Query.And(
                Query.EQ("Username", username),
                Query.EQ("ApplicationName", ApplicationName));

                var usr = users.FindOne(query);

                if (usr != null)
                {
                    passwordAnswer = usr.Contains("PasswordAnswer") ? usr["PasswordAnswer"].AsString : null;

                    bool isLockedOut = usr["IsLockedOut"].AsBoolean;

                    if (isLockedOut)
                        throw new MembershipPasswordException("The supplied user is locked out.");
                }
                else
                {
                    throw new MembershipPasswordException("The supplied user name is not found.");
                }

                if (RequiresQuestionAndAnswer && !CheckPassword(answer, passwordAnswer))
                {
                    UpdateFailureCount(username, "passwordAnswer");

                    throw new MembershipPasswordException("Incorrect password answer.");
                }

                var query2 = Query.And(
                Query.EQ("Username", username),
                Query.EQ("ApplicationName", ApplicationName),
                Query.EQ("IsLockedOut", false));

                var updateQuery = Update.Set("Password", EncodePassword(newPassword))
                     .Set("LastPasswordChangedDate", DateTime.Now);


                bSuccess = users.Update(query2, updateQuery).Ok;
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ResetPassword");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            if (bSuccess)
            {
                return newPassword;
            }
            else
            {
                throw new MembershipPasswordException("User not found, or user is locked out. Password not Reset.");
            }
        }

        /// <summary>
        /// Updates information about a user in the data source.
        /// </summary>
        /// <param name="user">A <see cref="T:System.Web.Security.MembershipUser"/> object that represents the user to update and the updated information for the user.</param>
        public override void UpdateUser(MembershipUser user)
        {

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);

            try
            {

                var query = Query.And(
                    Query.EQ("Username", user.UserName),
                    Query.EQ("ApplicationName", pApplicationName));

                var updateQuery = Update.Set("Email", user.Email)
                    .Set("Comment", user.Comment)
                    .Set("IsApproved", user.IsApproved);

                 users.Update(query, updateQuery);

            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UpdateUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Verifies that the specified user name and password exist in the data source.
        /// </summary>
        /// <param name="username">The name of the user to validate.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <returns>
        /// true if the specified username and password are valid; otherwise, false.
        /// </returns>
        public override bool ValidateUser(string username, string password)
        {
            bool isValid = false;
            bool isApproved = false;
            string pwd = "";

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);

            try
            {
                var query = Query.And(
                    Query.EQ("Username", username),
                    Query.EQ("ApplicationName", pApplicationName),
                    Query.EQ("IsLockedOut", false));

                var user = users.FindOne(query);
                if (user != null)
                {
                    pwd = user["Password"].AsString;
                    isApproved = user["IsApproved"].AsBoolean;
                }
                else
                {
                    return false;
                }

                if (CheckPassword(password, pwd))
                {
                    if (isApproved)
                    {
                        var query2 = Query.And(
                            Query.EQ("Username", username),
                            Query.EQ("ApplicationName", pApplicationName));

                        var updateQuery = Update.Set("LastLoginDate", DateTime.Now);

                        users.Update(query2, updateQuery);
                        
                        isValid = true;
                    }
                }
                else
                {
                    UpdateFailureCount(username, "password");
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "ValidateUser");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            return isValid;
        }

        /// <summary>
        /// Updates the failure count.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="failureType">Type of the failure.</param>
        private void UpdateFailureCount(string username, string failureType)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);

            DateTime windowStart = new DateTime();
            int failureCount = 0;

            try
            {

                var query = Query.And(
                        Query.EQ("Username", username),
                        Query.EQ("ApplicationName", pApplicationName));

                var user = users.FindOne(query);

                if (user != null)
                {
                    if (failureType == "password")
                    {
                        failureCount = user["FailedPasswordAttemptCount"].AsInt32;
                        windowStart = user["FailedPasswordAttemptWindowStart"].AsDateTime;
                    }

                    if (failureType == "passwordAnswer")
                    {
                        failureCount = user["FailedPasswordAnswerAttemptCount"].AsInt32;
                        windowStart = user["FailedPasswordAnswerAttemptWindowStart"].AsDateTime;
                    }
                }


                DateTime windowEnd = windowStart.AddMinutes(PasswordAttemptWindow);

                if (failureCount == 0 || DateTime.Now > windowEnd)
                {
                    // First password failure or outside of PasswordAttemptWindow. 
                    // Start a new password failure count from 1 and a new window starting now.

                    UpdateBuilder queryUpdate = null;
                    if (failureType == "password")
                    {
                        queryUpdate = Update.Set("FailedPasswordAttemptCount", 1)
                            .Set("FailedPasswordAttemptWindowStart", DateTime.Now);
                    }
                    if (failureType == "passwordAnswer")
                    {
                        queryUpdate = Update.Set("FailedPasswordAnswerAttemptCount", 1)
                           .Set("FailedPasswordAnswerAttemptWindowStart", DateTime.Now);
                    }

                    bool bSuccess = users.Update(query, queryUpdate).Ok;
                    
                    if (!bSuccess)
                        throw new ProviderException("Unable to update failure count and window start.");
                }
                else
                {
                    if (failureCount++ >= MaxInvalidPasswordAttempts)
                    {
                        // Password attempts have exceeded the failure threshold. Lock out
                        // the user.

                        UpdateBuilder queryUpdate = Update.Set("IsLockedOut", true)
                            .Set("LastLockedOutDate", DateTime.Now);

                        bool bSuccess = users.Update(query, queryUpdate).Ok;
                        
                        if (!bSuccess)
                            throw new ProviderException("Unable to lock out user.");
                    }
                    else
                    {
                        // Password attempts have not exceeded the failure threshold. Update
                        // the failure counts. Leave the window the same.

                        UpdateBuilder queryUpdate = null;
                        if (failureType == "password")
                        {
                            queryUpdate = Update.Set("FailedPasswordAttemptCount", failureCount);
                        }
                        if (failureType == "passwordAnswer")
                        {
                            queryUpdate = Update.Set("FailedPasswordAnswerAttemptCount", failureCount);
                        }

                        bool bSuccess = users.Update(query, queryUpdate).Ok;
                    
                        if (!bSuccess)
                            throw new ProviderException("Unable to update failure count.");
                    }
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "UpdateFailureCount");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }
        }

        /// <summary>
        /// Compares password values based on the MembershipPasswordFormat.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="dbpassword">The dbpassword.</param>
        /// <returns></returns>
        private bool CheckPassword(string password, string dbpassword)
        {
            string pass1 = password;
            string pass2 = dbpassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    pass2 = UnEncodePassword(dbpassword);
                    break;
                case MembershipPasswordFormat.Hashed:
                    pass1 = EncodePassword(password);
                    break;
                default:
                    break;
            }

            if (pass1 == pass2)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Encrypts, Hashes, or leaves the password clear based on the PasswordFormat..
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        private string EncodePassword(string password)
        {
            if (String.IsNullOrEmpty(password))
                return null; 
            
            string encodedPassword = password;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                      Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    HMACSHA1 hash = new HMACSHA1();
                    hash.Key = HexToByte(machineKey.ValidationKey);
                    encodedPassword =
                      Convert.ToBase64String(hash.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return encodedPassword;
        }

        /// <summary>
        /// Decrypts or leaves the password clear based on the PasswordFormat.
        /// </summary>
        /// <param name="encodedPassword">The encoded password.</param>
        /// <returns></returns>
        private string UnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;

            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password =
                      Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }

            return password;
        }

        /// <summary>
        /// Converts a hexadecimal string to a byte array. Used to convert encryption
        /// key values from the configuration.
        /// </summary>
        /// <param name="hexString">The hex string.</param>
        /// <returns></returns>
        private byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }

        /// <summary>
        /// Gets a collection of membership users where the user name contains the specified user name to match.
        /// </summary>
        /// <param name="usernameToMatch">The user name to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection usersCollection = new MembershipUserCollection();

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);

            var query = Query.And(Query.EQ("Username", usernameToMatch),
                Query.EQ("ApplicationName", pApplicationName));

            try
            {
                var cursor = users.Find(query);
                totalRecords = users.Count();

                if (totalRecords == 0) { return usersCollection; }

                var query1 = Query.And(Query.EQ("ApplicationName", ApplicationName),
                    Query.Matches("Username", new BsonRegularExpression(usernameToMatch + "*", "i")));

                var cursor1 = users.Find(query1).SetSortOrder(new string[] { "Username" });
                cursor1.Skip = Math.Max(0, pageSize * (pageIndex - 1));
                cursor1.SetFields(new string[] { "PKID", "Username", "Email", "PasswordQuestion", "Comment", "IsApproved", "IsLockedOut", "CreationDate", "LastLoginDate", "LastActivityDate", "LastPasswordChangedDate", "LastLockedOutDate" });
                cursor1.Limit = pageSize;

                foreach (var user in cursor1)
                {
                    MembershipUser u = GetUserFromReader(user);
                    usersCollection.Add(u);
                }
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "FindUsersByName");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            return usersCollection;
        }

        /// <summary>
        /// Gets a collection of membership users where the e-mail address contains the specified e-mail address to match.
        /// </summary>
        /// <param name="emailToMatch">The e-mail address to search for.</param>
        /// <param name="pageIndex">The index of the page of results to return. <paramref name="pageIndex"/> is zero-based.</param>
        /// <param name="pageSize">The size of the page of results to return.</param>
        /// <param name="totalRecords">The total number of matched users.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUserCollection"/> collection that contains a page of <paramref name="pageSize"/><see cref="T:System.Web.Security.MembershipUser"/> objects beginning at the page specified by <paramref name="pageIndex"/>.
        /// </returns>
        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            MembershipUserCollection usersCollection = new MembershipUserCollection();

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, SafeMode.True);

            //Build a query to find the user id and then update with new password.
            MongoCollection<BsonDocument> users = ProviderDB.GetCollection(pMongoProviderUsersCollectionName);

           


            totalRecords = 0;

            try
            {
                var query = Query.And(
                    Query.Matches("Email",new BsonRegularExpression(emailToMatch + "*", "i")),
                Query.EQ("ApplicationName", pApplicationName));

                var cursor = users.Find(query).SetSortOrder(new string[] { "Username" });
                cursor.Skip = Math.Max(0, pageSize * (pageIndex - 1));
                cursor.SetFields(new string[] { "PKID", "Username", "Email", "PasswordQuestion", "Comment", "IsApproved", "IsLockedOut", "CreationDate", "LastLoginDate", "LastActivityDate", "LastPasswordChangedDate", "LastLockedOutDate" });
                cursor.Limit = pageSize;

                foreach (var user in cursor)
                {
                    MembershipUser u = GetUserFromReader(user);
                    usersCollection.Add(u);
                }
                
            }
            catch (ApplicationException e)
            {
                if (WriteExceptionsToEventLog)
                {
                    WriteToEventLog(e, "FindUsersByEmail");

                    throw new ProviderException(exceptionMessage);
                }
                else
                {
                    throw e;
                }
            }
            finally
            {
            }

            return usersCollection;
        }

        /// <summary>
        /// Writes to event log.
        /// </summary>
        /// <param name="e">The e.</param>
        /// <param name="action">The action.</param>
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