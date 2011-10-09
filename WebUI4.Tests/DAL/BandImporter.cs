using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AltovientoSolutions.DAL.SimpleDBImport;
namespace WebUI4.Tests.DAL
{
    [TestClass]
    public class BandImporter
    {
        [TestMethod]
        [DeploymentItem("Files/SerenataMarichi.Bands.xml")]
        public void TestMethod1()
        {
            string path =  Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            string file = Path.Combine(path, @"Files\SerenataMarichi.Bands.xml");
            // Check if the created file exists in the deployment directory
            Assert.IsTrue(File.Exists(file), "deployment failed: " + file + " did not get deployed");

            string xmlContent = "";
            //Importer.ParseXimpleDbBands("MongoMariacherosConnStr", "unit_test_db", "BandImporter", xmlContent);
        }
    }
}
