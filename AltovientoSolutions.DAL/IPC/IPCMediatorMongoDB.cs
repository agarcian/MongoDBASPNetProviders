using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Driver.GridFS;
using AltovientoSolutions.DAL.IPC;
using AltovientoSolutions.DAL.IPC.Model;
using System.IO;

namespace AltovientoSolutions.DAL.IPC
{
    public class IPCMediatorMongoDB : IIPCMediator
    {
        private const string MONGO_DATABASE_NAME = "IllustratedPartsCatalogs";
        private MongoDatabase db;
        private string mongoCollectionName;

        /// <summary>
        /// Creates an instance that connects to a specific MongoDB Collection.
        /// </summary>
        /// <param name="MongoCollectionName"></param>
        public IPCMediatorMongoDB(string MongoCollectionName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MongoIPCViewerConnStr"].ConnectionString;
            string mongoCollectionName = MongoCollectionName;

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            db = server.GetDatabase(MONGO_DATABASE_NAME);
        }

        public bool DoesCatalogExist(string spaceId, string catalogId, string langCode)
        {
            MongoCollection<BsonDocument> mdbcolIPCs = db.GetCollection(mongoCollectionName);

            var query = Query.And(Query.EQ("SpaceId", spaceId),
                    Query.EQ("CatalogId", catalogId),
                    Query.EQ("LangCode", langCode));


            BsonDocument ipcRecord = mdbcolIPCs.FindOne(query);

            return (ipcRecord != null);
        }

        public void SaveCatalog(Catalog catalog, string spaceId, string catalogId, string langCode, bool overwrite)
        {
            MongoCollection<BsonDocument> mdbcolIPCs = db.GetCollection(mongoCollectionName);

            var query = Query.And(Query.EQ("SpaceId", spaceId),
                    Query.EQ("CatalogId", catalogId),
                    Query.EQ("LangCode", langCode));


            BsonDocument ipcRecord = mdbcolIPCs.FindOne(query);
            
            if (ipcRecord == null)
            {
                // the record does not exist.
                ipcRecord = new BsonDocument();
            }


            BsonDocument bsonDocCatalog = new BsonDocument();
            BsonWriter bsonWriter = BsonWriter.Create(bsonDocCatalog, BsonDocumentWriterSettings.Defaults);
            BsonSerializer.Serialize<Catalog>(bsonWriter, catalog);


            ipcRecord.Set("SpaceId", spaceId)
                .Set("CatalogId", spaceId)
                .Set("LangCode", langCode)
                .Set("Catalog", bsonDocCatalog);


            return;
        }

        public void SaveIllustration(byte[] buffer, string spaceId, string md5, string fileName)
        {
            MemoryStream ms = new MemoryStream(buffer);
            

            MongoGridFS gridFS = new MongoGridFS(db);
            MongoGridFSFileInfo fileInfo = gridFS.Upload(ms, fileName); 
           
            //gridFS.SetMetadata(fileInfo, 
            // to do:  how to add metadata to the file.

        }

        public Catalog GetCatalog(string spaceId, string catalogId, string langCode)
        {
            Catalog catalog;

            MongoCollection<BsonDocument> mdbcolIPCs = db.GetCollection(mongoCollectionName);

            var query = Query.And(Query.EQ("SpaceId", spaceId),
                    Query.EQ("CatalogId", catalogId),
                    Query.EQ("LangCode", langCode));

            BsonDocument ipcRecord = mdbcolIPCs.FindOne(query);

            if (ipcRecord == null)
            {
                // the record does not exist.
                ipcRecord = new BsonDocument();
                catalog = null;
            }
            else
            {
                BsonDocument bsonDocCatalog = new BsonDocument();
                BsonWriter bsonWriter = BsonWriter.Create(bsonDocCatalog, BsonDocumentWriterSettings.Defaults);
                catalog = BsonSerializer.Deserialize<Catalog>(bsonDocCatalog);

               //To DO: Could keep track of reads to see what documents are the most visited.  Incrementing a counter of reads.
            }

            return catalog;
        }

    }
}
