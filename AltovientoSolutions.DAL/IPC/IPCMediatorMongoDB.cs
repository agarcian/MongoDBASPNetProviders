using System;
using System.Drawing.Drawing2D;
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
using MongoDB.Bson.Serialization.IdGenerators;
using AltovientoSolutions.DAL.IPC;
using AltovientoSolutions.DAL.IPC.Model;
using System.IO;
using System.Drawing;

namespace AltovientoSolutions.DAL.IPC
{
    public class IPCMediatorMongoDB : IIPCMediator
    {
        private const string MONGO_DATABASE_NAME = "illustrated_parts_catalogs";
        private MongoDatabase db;
        private string mongoCollectionName;

        /// <summary>
        /// Creates an instance that connects to a specific MongoDB Collection.
        /// </summary>
        /// <param name="MongoCollectionName"></param>
        public IPCMediatorMongoDB(string MongoCollectionName)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MongoIPCViewerConnStr"].ConnectionString;
            mongoCollectionName = MongoCollectionName;

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
            MongoCollection<BsonDocument> collection = db.GetCollection(mongoCollectionName);

            var query = Query.And(Query.EQ("SpaceId", spaceId),
                    Query.EQ("CatalogId", catalogId),
                    Query.EQ("LangCode", langCode));


            BsonDocument ipcRecord = collection.FindOne(query);
            
            if (ipcRecord == null)
            {
                // the record does not exist.
                ipcRecord = new BsonDocument();
            }


            BsonDocument bsonDocCatalog = new BsonDocument();
            BsonWriter bsonWriter = BsonWriter.Create(bsonDocCatalog, BsonDocumentWriterSettings.Defaults);
            BsonSerializer.Serialize<Catalog>(bsonWriter, catalog);


            ipcRecord.Set("SpaceId", spaceId)
                .Set("CatalogId", catalogId)
                .Set("LangCode", langCode)
                .Set("Catalog", bsonDocCatalog);

            collection.Save(ipcRecord);

            return;
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

                BsonElement elCatalog = ipcRecord.GetElement("Catalog");



                //if (!BsonClassMap.IsClassMapRegistered(typeof(Catalog)))
                //{
                //    BsonClassMap.RegisterClassMap<Catalog>(cm =>
                //    {
                //        cm.AutoMap();
                //        var idMember = cm.GetMemberMap(c => c.ID);
                //        cm.SetIdMember(idMember);
                //        idMember.SetRepresentation(BsonType.String);
                //    });
                //}

               catalog = BsonSerializer.Deserialize<Catalog>(new BsonDocument((BsonDocument)elCatalog.Value));

               //To DO: Could keep track of reads to see what documents are the most visited.  Incrementing a counter of reads.
            }

            return catalog;
        }


        public void SaveIllustration(byte[] buffer, string spaceId, string md5, string fileName)
        {
            MemoryStream ms = new MemoryStream(buffer);


            MongoGridFS gridFS = new MongoGridFS(db);
            MongoGridFSFileInfo fileInfo = gridFS.Upload(ms, fileName);

            //gridFS.SetMetadata(fileInfo, 
            // to do:  how to add metadata to the file.

        }

        public Bitmap GetIllustration(string id)
        {
            MongoGridFS gridFS = new MongoGridFS(db);

            MongoGridFSFileInfo fileInfo = gridFS.FindOne(id);

            if (fileInfo == null || !fileInfo.Exists)
                return null;

            MongoGridFSStream stream = fileInfo.OpenRead();

            //// Read the source file into a byte array.
            //byte[] buffer = new byte[stream.Length];
            //int numBytesToRead = (int)stream.Length;
            //int numBytesRead = 0;

            //while (numBytesToRead > 0)
            //{
            //    // Read may return anything from 0 to numBytesToRead.
            //    int n = stream.Read(buffer, numBytesRead, numBytesToRead);

            //    // Break when the end of the file is reached.
            //    if (n == 0)
            //        break;

            //    numBytesRead += n;
            //    numBytesToRead -= n;
            //}
            //numBytesToRead = buffer.Length;

            if (stream == null || stream.Length == 0)
                return null;
            else
            {
                Bitmap bitmap = new Bitmap(stream);
                return bitmap;
            }
        }
    }
}
