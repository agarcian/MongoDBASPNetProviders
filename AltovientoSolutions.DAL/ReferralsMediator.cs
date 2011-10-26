using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Builders;

namespace AltovientoSolutions.DAL
{
    public class ReferralsMediator
    {
        private string connectionString;
        private MongoDatabase db;
        private string MongoCollectionName = "Referrals";
        private string MongoDatabaseName = "";

        public ReferralsMediator(string mongoDatabaseName)
        {
            connectionString = ConfigurationManager.ConnectionStrings["MongoAccountMgmtNonProviderConnStr"].ConnectionString;
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            db = server.GetDatabase(mongoDatabaseName);
        }

        /// <summary>
        /// Saves the referral request indicating who sent it and to whom it was sent.
        /// </summary>
        /// <param name="EmailReferrer">The email of the user sending the referral.</param>
        /// <param name="applicationName">Name of the application.</param>
        /// <param name="EmailOfInvitee">The email of invitee.</param>
        public void SaveReferralRequest(string EmailReferrer, string applicationName, string EmailOfInvitee, string Status)
        {
            if (String.IsNullOrEmpty(EmailOfInvitee))
                throw new ArgumentException("EmailOfInvitee cannot be empty");

            EmailOfInvitee = EmailOfInvitee.Trim().ToLower();


            MongoCollection<BsonDocument> referrals = db.GetCollection(MongoCollectionName);

            var query = Query.And(Query.EQ("ApplicationName", applicationName),
                    Query.EQ("EmailReferrer", EmailReferrer),
                    Query.EQ("Invitee", EmailOfInvitee));
 
            BsonDocument referralsRecord = referrals.FindOne(query);

            if (referralsRecord == null)
            {
                referralsRecord = new BsonDocument();
            }

            referralsRecord.Set("EmailReferrer", EmailReferrer)
                           .Set("ApplicationName", applicationName)
                           .Set("Invitee", EmailOfInvitee)
                           .Set("Timestamp", DateTime.Now)
                           .Set("Status", Status);
           
            referrals.Save(referralsRecord);


        }

        

    }
}
