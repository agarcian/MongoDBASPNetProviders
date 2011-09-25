using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;

namespace AltovientoSolutions.DAL
{
    public class MongoMediatorBase
    {
        protected string connectionString;

        [Obsolete("", true)]
        public MongoMediatorBase(string mongoDatabaseName, bool useSafeModeByDefault, string mongoCollectionName)
        {
            SafeMode safeMode = useSafeModeByDefault ? SafeMode.True : SafeMode.False;
            connectionString = ConfigurationManager.ConnectionStrings["MongoDBApplicationConnStr"].ConnectionString;

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(mongoDatabaseName, safeMode);

            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);
        
        }
    }
}
