using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AltovientoSolutions.DAL.IPC;
using AltovientoSolutions.DAL.IPC.Model;
namespace WebUI4.Tests.IPC
{
    [TestClass]
    public class IPCDataAccessTestUnit
    {
        private static string DATABASE_NAME = "unit_test_db";
        private static string COLLECTION_NAME = "IPC_Test_Client_Area";


        [TestInitialize]
        public void TestInitialize()
        {
            IPCMediatorMongoDB mediator = new IPCMediatorMongoDB(COLLECTION_NAME, DATABASE_NAME);
            if (!mediator.DoesCatalogExist("1234567", "en"))
            {
                Generate("1234567");
            }
        }

       

        private void Generate(string ID)
        {
            int nChapters = 10;
            int nPages = 20;
            int nEntries = 45;

           
            Dictionary<string, byte[]> Illustrations;
            if (!String.IsNullOrWhiteSpace(ID))
            {
                string[] language = new string[] {"en", "es", "pt", "du", "fr", "de", "ar", "hb", "zn", "ru" };
                Dictionary<String, Catalog> catalogs = LoremIpsum.GenerateSampleCatalog(ID, nChapters, nPages, nEntries, language, out Illustrations);

                // commit the dummy data to db.

                IPCMediatorMongoDB db = new IPCMediatorMongoDB(COLLECTION_NAME, DATABASE_NAME);

                foreach (KeyValuePair<String, Catalog> pair in catalogs)
                {
                    string langCode = pair.Key;
                    Catalog catalog = pair.Value;
                    db.SaveCatalog(catalog, catalog.ID, langCode, true);
                }

                foreach (KeyValuePair<String, byte[]> pair in Illustrations)
                {
                    string md5 = pair.Key;
                    byte[] buffer = pair.Value;
                    db.SaveIllustration(buffer, md5, md5);
                }

            }
        }

        [TestMethod]
        public void GetLanguagesTest()
        {

            IPCMediatorMongoDB mediator = new IPCMediatorMongoDB(COLLECTION_NAME, DATABASE_NAME);
            List<string> languages = mediator.GetAvailableLanguagesForCatalog("1234567");

            List<string> validLanguages = new List<String>(new string[] {"en", "es", "pt", "du", "fr", "de", "ar", "hb", "zn", "ru"});

            foreach (string langCode in languages)
            {
                Assert.IsTrue(validLanguages.Contains(langCode), "'"+langCode + "' was expected as a valid Language for the test catalog");
            }


        }

        [TestMethod]
        public void IndexCatalogTest()
        {

            IPCMediatorMongoDB mediator = new IPCMediatorMongoDB(COLLECTION_NAME, DATABASE_NAME);

            mediator.IndexCatalog("1234567", "en");


        }


    }
}
