using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;

namespace AltovientoSolutions.Security.Spaces
{
    public class MongoDBSpacesProvider : SpacesProvider
    {
        // Static fields.
        private static string connectionString = ConfigurationManager.ConnectionStrings[SpacesConfiguration.Settings.ConnectionString].ConnectionString;
        private static string databaseName = SpacesConfiguration.Settings.DatabaseName;
        private static string collectionName = SpacesConfiguration.Settings.CollectionName;


        public MongoDBSpacesProvider()
            : base()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Space)))
            {
                BsonClassMap.RegisterClassMap<Space>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id);
                    cm.IdMemberMap.SetIdGenerator(CombGuidGenerator.Instance);
                });
            }
        }

        public MongoDBSpacesProvider(string ApplicationName)
            : base(ApplicationName)
        {
        }


        public override string Description
        {
            get
            {
                return "MongoDB Spaces Provider";
            }
        }
        public override string Name
        {
            get
            {
                return "An implementation of the Spaces Provider";
            }
        }

        public override bool DoesNameExists(string Name)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            var query = Query.And(Query.EQ("Name", Name),
                Query.EQ("ApplicationName", this.applicationName));

            var doc = collection.FindOneAs(typeof(Space), query);

            return (doc != null);
        }

        public override Space CreateSpace(string Name)
        {
            Space space = new Space();

            // Create in the database the new space.

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            if (DoesNameExists(Name))
            {
                throw new SpaceProviderException(String.Format("A Space with name '{0}' for the ApplicationName '{1}' already exists.", Name, this.applicationName));
            }

            space.Name = Name;
            space.ApplicationName = this.applicationName;
            space.ApiSecret = Guid.NewGuid().ToString();

            SafeModeResult result = collection.Save(space, SafeMode.True);

            if (!result.Ok)
            {
                throw new SpaceProviderException();
            }

            return GetSpace(Name);
        }

        public override Space GetSpace(string Name)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            var query = Query.And(Query.EQ("Name", Name),
                Query.EQ("ApplicationName", this.applicationName));


            Space doc = (Space)collection.FindOneAs(typeof(Space), query);
            return doc;

        }

        public override bool SaveSpace(Space Space)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            BsonDocument doc = BsonDocument.Create(Space);

            // Removes the ApiSecret so it cannot be changed on a Save.  See reset ApiSecret.
            doc.Remove("ApiSecret");

            SafeModeResult result = collection.Save(doc, SafeMode.True);

            return result.Ok;
        }

        public override bool DeleteSpace(string Name)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            var query = Query.EQ("Name", Name);

            SafeModeResult result = collection.Remove(query, RemoveFlags.None, SafeMode.True);

            return result.Ok;
        }

        public override void ResetApiSecret(string Name)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            var query = Query.And(Query.EQ("Name", Name),
                Query.EQ("ApplicationName", this.applicationName));

            collection.FindAndModify(query, SortBy.Null, Update.Set("ApiSecret", Guid.NewGuid().ToString()), true, false);

        }
    
        
    
    }
}
