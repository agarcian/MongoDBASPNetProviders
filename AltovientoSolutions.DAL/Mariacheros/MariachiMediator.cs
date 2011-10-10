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
using System.Text.RegularExpressions;

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

        #region Band Management

        public bool DoesBandExist(string name)
        {
            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);

            var query = Query.EQ("NameLowerCase", name);

            BsonDocument doc = collection.FindOne(query);

            return (doc != null);
        }




        public bool GetSitesForUser(string username, out Dictionary<String, String> sites)
        {
            sites = new Dictionary<string, string>();

            // To Do:  What to do with multilingual sites.

            sites.Add("maribelsalinas", "Maribel Salinas website");
            sites.Add("mariachiviajero", "Mariachi Viajero - The best mariachi in California");

            return true;

        }
        
        #endregion


        #region Lyrics Management
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

        public bool AddSong(string SongTitle, string Author, string Lyrics)
        {
            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);

            BsonDocument doc = new BsonDocument();
            doc.Set("SongTitle", SongTitle)
                .Set("Lyrics", Lyrics)
                .Set("Slug", CreateSlug(SongTitle));

            if (!String.IsNullOrWhiteSpace(Author))
                doc.Set("Author", Author.Trim());


            SafeModeResult result = collection.Insert(doc, SafeMode.True);

            return result.Ok;
        }

        public bool UpdateSong(string id, string SongTitle, string Author, string Lyrics)
        {
            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);

            ObjectId objId;

            if (ObjectId.TryParse(id, out objId))
            {
                var query = Query.EQ("_id", objId);
                var update = Update.Set("SongTitle", SongTitle.Trim())
                    .Set("Lyrics", Lyrics)
                    .Set("Slug", CreateSlug(SongTitle));

                if (String.IsNullOrWhiteSpace(Author))
                    update.Unset("Author");
                else
                    update.Set("Author", Author.Trim());
                                    
                SafeModeResult result = collection.Update(query, update, SafeMode.True);
                
                return (result.Ok && result.UpdatedExisting);
            }
            else
            {
                return false;
            }
        }

        public bool DeleteSong(string id)
        {
            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);

            ObjectId objId;

            if (ObjectId.TryParse(id, out objId))
            {
                var query = Query.EQ("_id", objId);
                FindAndModifyResult result = collection.FindAndRemove(query, SortBy.Null);

                return result.Ok;
            }
            else
            {
                return false;
            }
        }

        public bool SaveSong(string id, string SongTitle, string Author, string Lyrics)
        {

            bool success = false;

            if (String.IsNullOrWhiteSpace(id))
            {
                success = AddSong(SongTitle, Author, Lyrics);
            }
            else
            {
                success = UpdateSong(id, SongTitle, Author, Lyrics);
            }

            return success;
        }

        private string CreateSlug(string SongTitle)
        {
            string slug = String.Empty;

            if (!String.IsNullOrWhiteSpace(SongTitle))
            {
                slug += SongTitle.Trim().Trim('-');
            }

            slug = slug.Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n")
                .Replace("Á", "A")
                .Replace("É", "E")
                .Replace("Í", "I")
                .Replace("Ó", "O")
                .Replace("Ú", "U")
                .Replace("Ñ", "N");

            slug = new Regex("[^A-Za-z0-9-]+").Replace(slug, "-");
            
            return slug.ToLower().Trim('-');
        }
        
        #endregion
    }
}
