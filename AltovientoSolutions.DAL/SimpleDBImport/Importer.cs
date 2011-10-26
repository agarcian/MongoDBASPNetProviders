using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using AltovientoSolutions.DAL.Mariacheros.Model;
using AltovientoSolutions.DAL.SimpleDBImport;

namespace AltovientoSolutions.DAL.SimpleDBImport
{
    public class Importer
    {

        public static void ParseXimpleDbBands()

        {
            string connectionStringName = "MongoMariacherosConnStr",
                mongoDatabaseName = "mariacheros", 
                mongoCollectionName = "Bands";


            // Connect to MongoDB.
            string connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(mongoDatabaseName);
            MongoCollection collection =  db.GetCollection(mongoCollectionName);

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(@"C:\temp\SerenataMariachi.Bands.xml");



            foreach (XmlElement item in xmlDoc.DocumentElement.ChildNodes)
            {
                // parse each item.
                BandModel band = new BandModel();
                band.Location = new Location();

                band.Slug = item.Attributes["nm"].Value.ToLower().Trim().Trim('-');

                foreach (XmlElement attrElem in item.ChildNodes)
                {
                    string attr = attrElem.GetAttribute("nm");
                    string val = attrElem.InnerText;

                    switch (attr)
                    {
                        case "DisplayName":
                            band.Name = val;
                            band.NameLowerCase = val.ToLower();
                            break;
                        case "Description":
                            band.Description.Add("en", val);
                            break;
                        case "DescriptionES":
                            band.Description.Add("es", val);
                            break;
                        case "Phone":
                            band.Phone.Add(val);
                            break;
                        case "Email":
                            band.Email = val;
                            break;
                        case "Website":
                            band.Website = val;
                            break;
                        case "Contact":
                            band.Contact = val;
                            break;
                        case "Address":
                            band.Address = val;
                            break;
                        case "City":
                            band.City = val;
                            break;
                        case "State":
                            band.State = val;
                            break;
                        case "ZipCode":
                            band.ZipCode = val;
                            break;
                        case "Country":
                            band.Country = val;
                            break;
                        case "PreferredLanguage":
                            if (val == "English")
                                band.PreferredLanguage = "en";
                            else
                                band.PreferredLanguage = "es";
                            break;
                        case "Recommended":
                            bool isRecommended = false;
                            Boolean.TryParse(val, out isRecommended);
                            band.Recommended = isRecommended;
                            break;
                        case "IsFemaleOnly":
                            bool isFemaleOnly = false;
                            Boolean.TryParse(val, out isFemaleOnly);
                            band.IsFemaleOnly = isFemaleOnly;
                            break;
                        case "InterviewUrl":
                            band.InterviewUrl = val;
                            break;
                        case "MailChimpRating":
                            int rating = 0;
                            Int32.TryParse(val, out rating);
                            band.MailchimpRating = rating;
                            break;
                        case "Longitude":
                            double lon = 0;
                            if (!String.IsNullOrWhiteSpace(val))
                            {
                                Double.TryParse(val.Replace('.',','), out lon);
                                band.Location.Longitude = lon;
                            }
                                break;
                        case "Latitude":
                            double lat = 0;
                            if (!String.IsNullOrWhiteSpace(val))
                            {
                                Double.TryParse(val.Replace('.', ','), out lat);
                                band.Location.Latitude = lat;
                            }
                            break;
                    }

                }

                band.Id = ObjectId.GenerateNewId().ToString();
                BsonDocument doc = new BsonDocument();
                BsonWriter bsonWriter = BsonWriter.Create(doc, BsonDocumentWriterSettings.Defaults);
                BsonSerializer.Serialize<BandModel>(bsonWriter, band);

                SafeModeResult result = collection.Insert(doc);
            }
        }
    }
}
