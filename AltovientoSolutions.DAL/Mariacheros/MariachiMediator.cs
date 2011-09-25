using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace AltovientoSolutions.DAL.Mariacheros
{
    public class MariachiMediator
    {
        private const string MONGO_DATABASE_NAME = "Mariacheros";
        private MongoDatabase db;
        private string mongoCollectionName = "website";

        /// <summary>
        /// Creates an instance that connects to a specific MongoDB Collection.
        /// </summary>
        /// <param name="MongoCollectionName"></param>
        public MariachiMediator()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MongoMariacherosConnStr"].ConnectionString;

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            db = server.GetDatabase(MONGO_DATABASE_NAME);
        }

        public bool DoesWebsiteExist(string name)
        {
            MongoCollection<BsonDocument> mdbcolIPCs = db.GetCollection(mongoCollectionName);

            var query = Query.EQ("WebsiteName", name);

            BsonDocument ipcRecord = mdbcolIPCs.FindOne(query);

            return (ipcRecord != null);
        }



        public static bool GetSitesForUser(string username, out Dictionary<String, String> sites)
        {
            sites = new Dictionary<string, string>();

            // To Do:  What to do with multilingual sites.

            sites.Add("maribelsalinas", "Maribel Salinas website");
            sites.Add("mariachiviajero", "Mariachi Viajero - The best mariachi in California");

            return true;

        }
    }
}
