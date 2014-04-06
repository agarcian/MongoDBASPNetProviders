using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Configuration.Provider;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.SessionState;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace ASPNETProvidersForMongoDB
{
    /// <summary>
    /// Custom ASP.NET Session State Provider using MongoDB as the state store.
    /// For reference on this implementation see MSDN ref:
    ///     - http://msdn.microsoft.com/en-us/library/ms178587.aspx
    ///     - http://msdn.microsoft.com/en-us/library/ms178588.aspx - this sample provider was used as the basis for this
    ///       provider, with MongoDB-specific implementation swapped in, plus cosmetic changes like naming conventions.
    /// 
    /// Session state is stored in a "Sessions" collection within a "SessionState" database. Example session document:
    /// {
    ///    "_id" : "bh54lskss4ycwpreet21dr1h",
    ///    "ApplicationName" : "/",
    ///    "Created" : ISODate("2011-04-29T21:41:41.953Z"),
    ///    "Expires" : ISODate("2011-04-29T22:01:41.953Z"),
    ///    "LockDate" : ISODate("2011-04-29T21:42:02.016Z"),
    ///    "LockId" : 1,
    ///    "Timeout" : 20,
    ///    "Locked" : true,
    ///    "SessionItems" : "AQAAAP////8EVGVzdAgAAAABBkFkcmlhbg==",
    ///    "Flags" : 0
    /// }
    /// 
    /// Inline with the above MSDN reference:
    /// If the provider encounters an exception when working with the data source, it writes the details of the exception 
    /// to the Application Event Log instead of returning the exception to the ASP.NET application. This is done as a security 
    /// measure to avoid private information about the data source from being exposed in the ASP.NET application.
    /// The sample provider specifies an event Source property value of "MongoSessionStateStore." Before your ASP.NET 
    /// application will be able to write to the Application Event Log successfully, you will need to create the following registry key:
    ///     HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\Eventlog\Application\MongoSessionStateStore
    /// If you do not want the sample provider to write exceptions to the event log, then you can set the custom writeExceptionsToEventLog 
    /// attribute to false in the Web.config file.
    ///
    /// The session-state store provider does not provide support for the Session_OnEnd event, it does not automatically clean up expired session-item data. 
    /// You should have a job to periodically delete expired session information from the data store where Expires date is in the past, i.e.:
    ///     db.Sessions.remove({"Expires" : {$lt : new Date() }})
    /// 
    /// Example web.config settings:
    ///  
    ///  <connectionStrings>
    ///     <add name="MongoSessionServices"
    ///        connectionString="mongodb://xxxx" />
    ///  </connectionStrings>
    ///  <system.web>
    ///     <sessionState
    ///         mode="Custom"
    ///         customProvider="MongoDBSessionStateProvider">
    ///             <providers>
    ///                 <add name="MongoDBSessionStateProvider"
    ///                     type="ASPNETProvidersForMongoDB.MongoDBSessionStateStore"
    ///                     connectionStringName="MongoSessionServices"
    ///                     mongoProviderDatabaseName = "databasename"
    ///                     mongoProviderCollectionName = "SessionState"
    ///                     applicationName="MyApplication" />
    ///             </providers>
    ///     </sessionState>
    ///     ...
    /// </system.web>
    /// replicasToWrite setting is interpreted as the number of replicas to write to, in addition to the primary (in a replicaset environment).
    /// i.e. replicasToWrite = 0, will wait for the response from writing to the primary node. > 0 will wait for the response having written to 
    /// ({replicasToWrite} + 1) nodes
    /// </summary>
    public sealed class MongoDBSessionStateStore : SessionStateStoreProviderBase
    {
        #region private fields
        private string exceptionMessage = "An exception occurred. Please check the Event Log.";
        private string connectionString;
        private string pApplicationName;
        private TimeSpan pTimeout;
        private string pMongoProviderDatabaseName;
        private string pmongoProviderCollectionName;
        #endregion

        #region Properties

        /// <summary>
        /// Gets the name of the application using the custom membership provider.
        /// </summary>
        public string ApplicationName
        {
            get { return pApplicationName; }
        }

        /// <summary>
        /// Gets the timeout.
        /// </summary>
        public TimeSpan Timeout
        {
            get
            {
                return pTimeout;
            }
        }
        #endregion

        #region Constructor and initialization

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
                name = "MongoDBSessionStateStore";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "MongoDB SessionStore provider");
            }


            //
            // Initialize the abstract base class.
            //
            base.Initialize(name, config);

            //
            // Initialize applicationName
            //
            pApplicationName = GetConfigValue(config["applicationName"], System.Web.Hosting.HostingEnvironment.ApplicationVirtualPath);

            //
            // Initialize databaseName
            //
            if (String.IsNullOrWhiteSpace(config["mongoProviderDatabaseName"]))
            {
                throw new ProviderException("mongoProviderDatabaseName is not defined in the web.config under the membership section.");
            }
            else
            {
                pMongoProviderDatabaseName = config["mongoProviderDatabaseName"];
            }

            //
            // Initialize collectionName
            //
            pmongoProviderCollectionName = Convert.ToString(GetConfigValue(config["mongoProviderCollectionName"], "StateStore"));

            //
            // Initialize ConnectionString.
            //
            ConnectionStringSettings ConnectionStringSettings =
              ConfigurationManager.ConnectionStrings[config["connectionStringName"]];

            if (ConnectionStringSettings == null || String.IsNullOrWhiteSpace(ConnectionStringSettings.ConnectionString))
            {
                throw new ProviderException("Connection string cannot be blank.");
            }

            connectionString = ConnectionStringSettings.ConnectionString;

            //
            // Initialize Timeout
            //
            // Get <sessionState> configuration element.
            Configuration cfg = WebConfigurationManager.OpenWebConfiguration(HostingEnvironment.ApplicationVirtualPath);
            pTimeout = ((SessionStateSection)cfg.GetSection("system.web/sessionState")).Timeout;

            //
            // Ensures that the MongoDB collection has the proper indices defined.
            //
            EnsureIndices();
        }

        /// <summary>
        /// Runs a call to ensure that each MongoDB collection used here will set the indices as required.   
        /// That way it is not necessary to do it on the collection set up.
        /// </summary>
        private void EnsureIndices()
        {
            try
            {
                MongoClient client = new MongoClient(connectionString);
                MongoServer server = client.GetServer(); // connect to the mongoDB url.
                MongoDatabase ProviderDB = server.GetDatabase(pMongoProviderDatabaseName, WriteConcern.Acknowledged);

                //Build a query to find the user id and then update with new password.
                MongoCollection<BsonDocument> usersCollection = ProviderDB.GetCollection(pmongoProviderCollectionName);

                // Ensure Indices for ApplicationSpaces Collection.
                usersCollection.CreateIndex(IndexKeys.Ascending("_id", "ApplicationName"), IndexOptions.SetUnique(true));


            }
            catch (MongoAuthenticationException exc)
            {
                throw;
            }
            catch (Exception exc)
            {
                throw;
            }
        }
        #endregion

        #region Private Convenience Methods
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
        /// Returns a reference to the collection in MongoDB.
        /// data.
        /// </summary>
        /// <param name="conn">MongoDB server connection</param>
        /// <returns>MongoCollection</returns>
        private MongoCollection<BsonDocument> GetMongoDBCollection(MongoServer conn)
        {
            return conn.GetDatabase(pMongoProviderDatabaseName).GetCollection(pmongoProviderCollectionName);
        }

        /// <summary>
        /// Returns a connection to the MongoDB server.
        /// </summary>
        /// <returns>MongoServer</returns>
        private MongoServer GetMongoDBConnection()
        {
            var client = new MongoClient(connectionString);
            return client.GetServer();
        }

        #endregion

        #region Provider Implementation

        public override SessionStateStoreData CreateNewStoreData(HttpContext context, int timeout)
        {
            return new SessionStateStoreData(new SessionStateItemCollection(), SessionStateUtility.GetSessionStaticObjects(context), timeout);
        }

        /// <summary>
        /// SessionStateProviderBase.SetItemExpireCallback
        /// </summary>
        public override bool SetItemExpireCallback(SessionStateItemExpireCallback expireCallback)
        {
            return false;
        }

        /// <summary>
        /// Serialize is called by the SetAndReleaseItemExclusive method to 
        /// convert the SessionStateItemCollection into a Base64 string to    
        /// be stored in MongoDB.
        /// </summary>
        private string Serialize(SessionStateItemCollection items)
        {
            using (var ms = new MemoryStream())
            using (var writer = new BinaryWriter(ms))
            {
                if (items != null)
                    items.Serialize(writer);

                writer.Close();

                return Convert.ToBase64String(ms.ToArray());
            }
        }

        /// <summary>
        /// SessionStateProviderBase.SetAndReleaseItemExclusive
        /// </summary>
        public override void SetAndReleaseItemExclusive(HttpContext context, string id, SessionStateStoreData item, object lockId, bool newItem)
        {
            // Serialize the SessionStateItemCollection as a string.
            string sessItems = Serialize((SessionStateItemCollection)item.Items);

            MongoServer conn = GetMongoDBConnection();
            MongoCollection sessionCollection = GetMongoDBCollection(conn);

            try
            {
                if (newItem)
                {
                    var insertDoc = new BsonDocument
                        {
                            {"_id", id},
                            {"ApplicationName", ApplicationName},
                            {"Created", DateTime.Now.ToUniversalTime()},
                            {"Expires", DateTime.Now.AddMinutes(item.Timeout).ToUniversalTime()},
                            {"LockDate", DateTime.Now.ToUniversalTime()},
                            {"LockId", 0},
                            {"Timeout", item.Timeout},
                            {"Locked", false},
                            {"SessionItems", sessItems},
                            {"Flags", 0}
                        };

                    var query = Query.And(
                        Query.EQ("_id", id),
                        Query.EQ("ApplicationName", ApplicationName),
                        Query.LT("Expires", DateTime.Now.ToUniversalTime())
                        );
                    sessionCollection.Remove(query, WriteConcern.Acknowledged);
                    sessionCollection.Insert(insertDoc, WriteConcern.Acknowledged);
                }
                else
                {
                    var query = Query.And(
                        Query.EQ("_id", id),
                        Query.EQ("ApplicationName", ApplicationName),
                        Query.EQ("LockId", (Int32)lockId)
                        );
                    var update = Update.Set("Expires", DateTime.Now.AddMinutes(item.Timeout).ToUniversalTime());
                    update.Set("SessionItems", sessItems);
                    update.Set("Locked", false);
                    sessionCollection.Update(query, update, WriteConcern.Acknowledged);
                }
            }
            catch (Exception exc)
            {
                throw new ProviderException(exceptionMessage, exc);
            }
        }

        /// <summary>
        /// SessionStateProviderBase.GetItem
        /// </summary>
        public override SessionStateStoreData GetItem(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            return GetSessionStoreItem(false, context, id, out locked, out lockAge, out lockId, out actionFlags);
        }

        /// <summary>
        /// SessionStateProviderBase.GetItemExclusive
        /// </summary>
        public override SessionStateStoreData GetItemExclusive(HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            return GetSessionStoreItem(true, context, id, out locked, out lockAge, out lockId, out actionFlags);
        }

        /// <summary>
        /// GetSessionStoreItem is called by both the GetItem and 
        /// GetItemExclusive methods. GetSessionStoreItem retrieves the 
        /// session data from the data source. If the lockRecord parameter
        /// is true (in the case of GetItemExclusive), then GetSessionStoreItem
        /// locks the record and sets a new LockId and LockDate.
        /// </summary>
        private SessionStateStoreData GetSessionStoreItem(bool lockRecord, HttpContext context, string id, out bool locked, out TimeSpan lockAge, out object lockId, out SessionStateActions actionFlags)
        {
            // Initial values for return value and out parameters.
            SessionStateStoreData item = null;
            lockAge = TimeSpan.Zero;
            lockId = null;
            locked = false;
            actionFlags = 0;

            MongoServer conn = GetMongoDBConnection();
            MongoCollection sessionCollection = GetMongoDBCollection(conn);

            // DateTime to check if current session item is expired.
            // String to hold serialized SessionStateItemCollection.
            string serializedItems = "";
            // True if a record is found in the database.
            bool foundRecord = false;
            // True if the returned session item is expired and needs to be deleted.
            bool deleteData = false;
            // Timeout value from the data store.
            int timeout = 0;

            try
            {
                // lockRecord is true when called from GetItemExclusive and
                // false when called from GetItem.
                // Obtain a lock if possible. Ignore the record if it is expired.
                IMongoQuery query;
                if (lockRecord)
                {
                    query = Query.And(
                        Query.EQ("_id", id),
                        Query.EQ("ApplicationName", ApplicationName),
                        Query.EQ("Locked", false),
                        Query.GT("Expires", DateTime.Now.ToUniversalTime())
                        );

                    var update = Update.Set("Locked", true);
                    update.Set("LockDate", DateTime.Now.ToUniversalTime());
                    var result = sessionCollection.Update(query, update, WriteConcern.Acknowledged);

                    locked = result.DocumentsAffected == 0; // DocumentsAffected == 0 == No record was updated because the record was locked or not found.
                }

                // Retrieve the current session item information.
                query = Query.And(Query.EQ("_id", id), Query.EQ("ApplicationName", ApplicationName));
                var results = sessionCollection.FindOneAs<BsonDocument>(query);

                if (results != null)
                {
                    DateTime expires = results["Expires"].ToUniversalTime();

                    if (expires < DateTime.Now.ToUniversalTime())
                    {
                        // The record was expired. Mark it as not locked.
                        locked = false;
                        // The session was expired. Mark the data for deletion.
                        deleteData = true;
                    }
                    else
                    {
                        foundRecord = true;
                    }

                    serializedItems = results["SessionItems"].AsString;
                    lockId = results["LockId"].AsInt32;
                    lockAge = DateTime.Now.ToUniversalTime().Subtract(results["LockDate"].ToUniversalTime());
                    actionFlags = (SessionStateActions)results["Flags"].AsInt32;
                    timeout = results["Timeout"].AsInt32;
                }

                // If the returned session item is expired, 
                // delete the record from the data source.
                if (deleteData)
                {
                    query = Query.And(Query.EQ("_id", id), Query.EQ("ApplicationName", ApplicationName));
                    sessionCollection.Remove(query, WriteConcern.Acknowledged);
                }

                // The record was not found. Ensure that locked is false.
                if (!foundRecord)
                {
                    locked = false;
                }

                // If the record was found and you obtained a lock, then set 
                // the lockId, clear the actionFlags,
                // and create the SessionStateStoreItem to return.
                if (foundRecord && !locked)
                {
                    lockId = (int)lockId + 1;

                    query = Query.And(Query.EQ("_id", id), Query.EQ("ApplicationName", ApplicationName));
                    var update = Update.Set("LockId", (int)lockId);
                    update.Set("Flags", 0);
                    sessionCollection.Update(query, update, WriteConcern.Acknowledged);

                    // If the actionFlags parameter is not InitializeItem, 
                    // deserialize the stored SessionStateItemCollection.
                    item = actionFlags == SessionStateActions.InitializeItem ? CreateNewStoreData(context, (int)pTimeout.TotalMinutes) : Deserialize(context, serializedItems, timeout);
                }
            }
            catch (Exception exc)
            {
                throw new ProviderException(exceptionMessage, exc);
            }

            return item;
        }

        private SessionStateStoreData Deserialize(HttpContext context, string serializedItems, int timeout)
        {
            using (var ms = new MemoryStream(Convert.FromBase64String(serializedItems)))
            {

                var sessionItems =
                  new SessionStateItemCollection();

                if (ms.Length > 0)
                {
                    using (var reader = new BinaryReader(ms))
                    {
                        sessionItems = SessionStateItemCollection.Deserialize(reader);
                    }
                }

                return new SessionStateStoreData(sessionItems, SessionStateUtility.GetSessionStaticObjects(context), timeout);
            }
        }

        public override void CreateUninitializedItem(HttpContext context, string id, int timeout)
        {
            MongoServer conn = GetMongoDBConnection();
            MongoCollection sessionCollection = GetMongoDBCollection(conn);
            var doc = new BsonDocument
                {
                    {"_id", id},
                    {"ApplicationName", ApplicationName},
                    {"Created", DateTime.Now.ToUniversalTime()},
                    {"Expires", DateTime.Now.AddMinutes(timeout).ToUniversalTime()},
                    {"LockDate", DateTime.Now.ToUniversalTime()},
                    {"LockId", 0},
                    {"Timeout", timeout},
                    {"Locked", false},
                    {"SessionItems", ""},
                    {"Flags", 1}
                };

            try
            {
                var result = sessionCollection.Insert(doc, WriteConcern.Acknowledged);
                if (!result.Ok)
                {
                    throw new Exception(result.ErrorMessage);
                }
            }
            catch (Exception exc)
            {
                throw new ProviderException(exceptionMessage, exc);
            }
        }

        /// <summary>
        /// This is a helper function that writes exception detail to the 
        /// event log. Exceptions are written to the event log as a security
        /// measure to ensure private database details are not returned to 
        /// browser. If a method does not return a status or Boolean
        /// indicating the action succeeded or failed, the caller also 
        /// throws a generic exception.
        /// </summary>
        private void WriteToEventLog(Exception e, string action)
        {
            //using (var log = new EventLog())
            //{
            //    log.Source = EventSource;
            //    log.Log = EventLog;

            //    string message = String.Format("An exception occurred communicating with the data source.\n\nAction: {0}\n\nException: {1}", action, e);

            //    log.WriteEntry(message);
            //}
        }

        public override void Dispose()
        {
        }

        public override void EndRequest(HttpContext context)
        {

        }

        public override void InitializeRequest(HttpContext context)
        {

        }

        public override void ReleaseItemExclusive(HttpContext context, string id, object lockId)
        {
            MongoServer conn = GetMongoDBConnection();
            MongoCollection sessionCollection = GetMongoDBCollection(conn);

            var query = Query.And(Query.EQ("_id", id), Query.EQ("ApplicationName", ApplicationName), Query.EQ("LockId", (Int32)lockId));
            var update = Update.Set("Locked", false);
            update.Set("Expires", DateTime.Now.AddMinutes(pTimeout.TotalMinutes).ToUniversalTime());

            try
            {
                sessionCollection.Update(query, update, WriteConcern.Acknowledged);
            }
            catch (Exception exc)
            {
                throw new ProviderException(exceptionMessage);
            }
        }

        public override void RemoveItem(HttpContext context, string id, object lockId, SessionStateStoreData item)
        {
            MongoServer conn = GetMongoDBConnection();
            MongoCollection sessionCollection = GetMongoDBCollection(conn);

            var query = Query.And(Query.EQ("_id", id), Query.EQ("ApplicationName", ApplicationName), Query.EQ("LockId", (Int32)lockId));

            try
            {
                sessionCollection.Remove(query, WriteConcern.Acknowledged);
            }
            catch (Exception exc)
            {
                throw new ProviderException(exceptionMessage, exc);
            }
        }

        public override void ResetItemTimeout(HttpContext context, string id)
        {
            MongoServer conn = GetMongoDBConnection();
            MongoCollection sessionCollection = GetMongoDBCollection(conn);
            var query = Query.And(Query.EQ("_id", id), Query.EQ("ApplicationName", ApplicationName));
            var update = Update.Set("Expires", DateTime.Now.AddMinutes(pTimeout.TotalMinutes).ToUniversalTime());

            try
            {
                sessionCollection.Update(query, update, WriteConcern.Acknowledged);
            }
            catch (Exception exc)
            {
                throw new ProviderException(exceptionMessage, exc);
            }
        }
        
        #endregion
    }
}
