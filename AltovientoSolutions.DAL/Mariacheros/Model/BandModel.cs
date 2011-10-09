using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Attributes;

namespace AltovientoSolutions.DAL.Mariacheros.Model
{
    public class BandModel
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; }
       
        public string Name { get; set; }
        public string NameLowerCase { get; set; }

        public string Slug { get; set; }

        private Dictionary<string, string> description = new Dictionary<string, string>();
        public Dictionary<string, string> Description
        {
            get { return description; }
            set { description = value; }
        }

        private List<string> phone = new List<string>();
        public List<string> Phone
        {
            get { return phone; }
            set { phone = value; }
        }
        //private List<string> video = new List<string>();
        //public List<string> Video
        //{
        //    get { return video; }
        //    set { video = value; }
        //}
        [BsonIgnoreIfNull]
        public string Contact { get; set; }
        [BsonIgnoreIfNull]
        public string Website { get; set; }
        [BsonIgnoreIfNull]
        public string Address { get; set; }
        [BsonIgnoreIfNull]
        public string City { get; set; }
        [BsonIgnoreIfNull]
        public string ZipCode { get; set; }
        [BsonIgnoreIfNull]
        public string State { get; set; }
        [BsonIgnoreIfNull]
        public string Country { get; set; }
        [BsonIgnoreIfNull]
        public string Email { get; set; }
        [BsonIgnoreIfNull]
        public string PreferredLanguage { get; set; }
        public bool Recommended { get; set; }
        public bool IsFemaleOnly { get; set; }
        public Location Location { get; set; }
        [BsonIgnoreIfNull]
        public string InterviewUrl { get; set; }
        public int MailchimpRating { get; set; }
    }
    public class Location{
        public double Longitude { get; set; }
        public double Latitude { get; set; }
    }
}
