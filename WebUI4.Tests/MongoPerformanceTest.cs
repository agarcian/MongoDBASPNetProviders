using System;
using System.Configuration;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Threading.Tasks;
using MongoDB.Driver.Builders;
namespace WebUI4.Tests
{
    [TestClass]
    public class MongoPerformanceTest
    {
        [TestMethod]
        public void TestWritesSafeModeFalse()
        {
            MongoServer server = MongoServer.Create(ConfigurationManager.ConnectionStrings["MongoUnitTestConnStr"].ConnectionString);

            MongoDatabase test = server.GetDatabase("unit_test_db");

            int totalDocuments = 100000;
            var list = Enumerable.Range(0, totalDocuments).ToList();

            long count = 0;
            DateTime start, end;

            using (server.RequestStart(test))
            {
                MongoCollection coll = test.GetCollection("testCollection");

                start = DateTime.Now;
                Parallel.ForEach(list, i =>
                {
                
                    var query = new QueryDocument("_id", i);
                    coll.Update(query, Update.Set("value" + i, 100),
                                 UpdateFlags.Upsert, SafeMode.False);
                
                    // Calling a count periodically (but sparsely) seems to do the trick.
                    if (i%10000 == 0)
                        count = coll.Count();
                });

                // Call count one last time to report in the test.
                count = coll.Count();
                end = DateTime.Now;
            }
            
            Console.WriteLine(String.Format("Execution Time:{0}.{1}.  Expected No of docs: {2}, Actual No of docs {3}", (end-start).TotalSeconds, (end-start).Milliseconds, count, totalDocuments));
        }

        [TestMethod]
        public void TestRead()
        {
            // Create a pseudo-random generator.
            System.Random rnd = new Random(DateTime.Now.Millisecond);

            // Connect to the server.
            MongoServer server = MongoServer.Create(ConfigurationManager.ConnectionStrings["MongoUnitTestConnStr"].ConnectionString);

            // Get the test collection
            MongoDatabase test = server.GetDatabase("unit_test_db");

            // Simulate defining the documents to retrieve.
            List<int> docsToRead = new List<int>();
            int numberOfDocsToRead = 20;
            for (int i = 0; i < numberOfDocsToRead; i++)
            {
                // Get an id representing one of the 100,000 documents.
                docsToRead.Add(rnd.Next(100000));
            }

            
            DateTime start, end;

            int returnedDocs = 0;

            // Begin a series of operations that will run under the same connection.
            using (server.RequestStart(test))
            {
                MongoCollection coll = test.GetCollection("testCollection");

                // Start timer.
                start = DateTime.Now;
                foreach(int id in docsToRead)
                {
                    var query = new QueryDocument("_id", id);
                    BsonDocument resultDoc = coll.FindOneAs<BsonDocument>(query);

                    if (resultDoc != null)
                        returnedDocs++;

                }

                // Call count one last time to report in the test.
                end = DateTime.Now;
            }

            Console.WriteLine(String.Format("Execution Time:{0}.  Documents to read: {1}, Actual No of docs read {2}", (end - start).TotalSeconds, numberOfDocsToRead, returnedDocs));
        
        }


    }
}
