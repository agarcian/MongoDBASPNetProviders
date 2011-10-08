using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver.Builders;

namespace AltovientoSolutions.DAL.Mariacheros
{
    public class MariachiMediator
    {
        private const string MONGO_DATABASE_NAME = "mariacheros";
        private MongoDatabase db;
        private string mongoCollectionName;

        ///// <summary>
        ///// Creates an instance that connects to a specific MongoDB Collection.
        ///// </summary>
        ///// <param name="MongoCollectionName"></param>
        //public MariachiMediator()
        //{
        //    string connectionString = ConfigurationManager.ConnectionStrings["MongoMariacherosConnStr"].ConnectionString;

        //    MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
        //    db = server.GetDatabase(MONGO_DATABASE_NAME);

        //    InitializeMappings();
        //}


        /// <summary>
        /// Creates an instance that connects to a specific MongoDB Collection.
        /// </summary>
        /// <param name="MongoCollectionName"></param>
        public MariachiMediator(string collectionName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MongoMariacherosConnStr"].ConnectionString;
            mongoCollectionName = collectionName;

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            db = server.GetDatabase(MONGO_DATABASE_NAME);

            InitializeMappings();
        }

        private void InitializeMappings()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Model.LyricsModel)))
            {
                BsonClassMap.RegisterClassMap<Model.LyricsModel>(cm =>
                {
                    cm.AutoMap();
                    var idMember = cm.GetMemberMap(c => c.Id);
                    cm.SetIdMember(idMember);
                    idMember.SetRepresentation(BsonType.ObjectId);
                });
            }
        }




        public bool DoesWebsiteExist(string name)
        {
            MongoCollection<BsonDocument> mdbcolIPCs = db.GetCollection(mongoCollectionName);

            var query = Query.EQ("WebsiteName", name);

            BsonDocument ipcRecord = mdbcolIPCs.FindOne(query);

            return (ipcRecord != null);
        }



        public bool GetSitesForUser(string username, out Dictionary<String, String> sites)
        {
            sites = new Dictionary<string, string>();

            // To Do:  What to do with multilingual sites.

            sites.Add("maribelsalinas", "Maribel Salinas website");
            sites.Add("mariachiviajero", "Mariachi Viajero - The best mariachi in California");

            return true;

        }



        public List<Model.LyricsModel> GetAllLyrics()
        {
            List<Model.LyricsModel> allLyrics = new List<Model.LyricsModel>();

            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);

            var cursor = collection.FindAll()
                .SetFields(new string[] { "Author", "SongTitle" })
                .SetSortOrder(new string[] { "SongTitle" });

            if (cursor != null)
            {
                foreach (BsonDocument doc in cursor)
                {
                    Model.LyricsModel model = (Model.LyricsModel) BsonSerializer.Deserialize<Model.LyricsModel>(doc);
                    allLyrics.Add(model);
                }
            }

            return allLyrics;
        }


        public Model.LyricsModel GetLyrics(string id)
        {
            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);

            Model.LyricsModel model = null;

            if (!String.IsNullOrWhiteSpace(id))
            {
                var query = Query.EQ("_id", id);
                model = collection.FindOneByIdAs<Model.LyricsModel>(new ObjectId(id));
            }

            return model;
        }

        public void AddSong(string SongTitle, string Author, string Lyrics)
        {
            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);

            BsonDocument doc = new BsonDocument();
            doc.Set("SongTitle", SongTitle)
                .Set("Lyrics", Lyrics);

            if (!String.IsNullOrWhiteSpace(Author))
                doc.Set("Author", Author);



            SafeModeResult result = collection.Insert(doc, SafeMode.True);

        }

        public void UpdateSong(string id, string SongTitle, string Author, string Lyrics)
        {
            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);

            var query = Query.EQ("_id", id);
            var update = Update.Set("SongTitle", SongTitle.Trim())
                .Set("Author", Author.Trim())
                .Set("Lyrics", Lyrics);

            SafeModeResult result = collection.Update(query, update, SafeMode.True);
        }

        public void SaveSong(string id, string SongTitle, string Author, string Lyrics)
        {
            if (String.IsNullOrWhiteSpace(id))
            {
                AddSong(SongTitle, Author, Lyrics);
            }
            else
            {
                UpdateSong(id, SongTitle, Author, Lyrics);
            }
        }

    }
}
