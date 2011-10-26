using System;
using System.Text;
using System.Configuration;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ASPNETProvidersForMongoDB;
using MongoDB.Bson;
using MongoDB.Driver;


namespace TestProjectForMongoDBASPNETProviders
{
    [TestClass]
    public class MongoDBMembershipProviderTest
    {
        static string mongoDBConnString = ConfigurationManager.ConnectionStrings["MongoProvidersDBConnStr"].ConnectionString;
        static MongoDBMembershipProvider mongoProvider = new MongoDBMembershipProvider();


        [ClassInitialize]
        public static void InitializeTestClass(TestContext context)
        {
            // Use the App.config to set the properties of the MongoDB database.
            mongoProvider = (MongoDBMembershipProvider)Membership.Provider;


            // Initializes the database with 10 sample users.
            MembershipCreateStatus status;
            for (int i = 1; i <= 10; i++)
            {
                mongoProvider.CreateUser("username" + i, "password" + i, "user" + i + "@example.com",
                    "What is MongoDB?",
                    "A cool NoSQL database", true, Guid.NewGuid(), out status);
            }
        }


        [ClassCleanup]
        public static void CleanupTestClass()
        {
            // Use the App.config to set the properties of the MongoDB database.
            mongoProvider = (MongoDBMembershipProvider)Membership.Provider;

            // Removes from the database the 10 sample users.
            for (int i = 1; i <= 10; i++)
            {
                mongoProvider.DeleteUser("username" + i, true);
            }
        }

        //[TestInitialize]
        //public void InitializeTest()
        //{
        //}


        //[TestCleanup]
        //public void CleanupTest()
        //{
        //}



        [TestMethod]
        public void CreateAndDeleteUserTest()
        {
            DateTime startTest = DateTime.Now;

            string username = "usernamexyz";
            string email = "userxyz@example.com";
            string password = "12345678";
            string question = "What is MongoDB?";
            string answer = "A cool NoSQL database";
            bool isApproved = true;

            MembershipUser user = mongoProvider.GetUser(username, false);

            // the user already exists.  Let's remove it.
            if (user != null)
            {
                bool bSuccess = mongoProvider.DeleteUser(username, true);
                Assert.IsTrue(bSuccess);
            }


            MembershipCreateStatus status;

            // Create a user.  This is a valid call.
            user = mongoProvider.CreateUser(username, password, email,
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNotNull(user, "A user should have been created");
            Assert.AreEqual(status, MembershipCreateStatus.Success, "The creation should have been successful");


            // Create a user with the same username.
            user = mongoProvider.CreateUser(username, password, "AnotherEmail@example.com",
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNull(user, "No user should have been created since the username is already taken");
            Assert.AreEqual(status, MembershipCreateStatus.DuplicateUserName, "Expecting a DuplicateUserName status");


            // Create a user with a different same username but the same password.
            user = mongoProvider.CreateUser("newUserName", password, email,
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNull(user, "No user should have been created since the email is already taken"); 
            Assert.AreEqual(status, MembershipCreateStatus.DuplicateEmail, "Expecting DuplicateEmail status");

            // Clean up after yourself, please...
            user = mongoProvider.GetUser(username, false); 
            if (user != null)
            {
                bool bSuccess = mongoProvider.DeleteUser(username, true);
                Assert.IsTrue(bSuccess, "Should have deleted the user");
            }

            double durationMiliseconds = DateTime.Now.Subtract(startTest).TotalMilliseconds;
            Console.WriteLine(String.Format("Test Duration: {0} miliseconds.", durationMiliseconds));
        }

        [TestMethod]
        public void GetAllUsersTest()
        {
            DateTime startTest = DateTime.Now;
            
            // All defined users are expected ...
            int totalRecords = 0;
            MembershipUserCollection users = mongoProvider.GetAllUsers(1, 5, out totalRecords);
            Assert.AreEqual(10, totalRecords, "Should return a full count of all users in the database");
            Assert.AreEqual(5, users.Count, "Expecting a limited number of users from the search (page size)");
            
            users = mongoProvider.GetAllUsers(2, 7, out totalRecords);
            Assert.AreEqual(10, totalRecords);
            Assert.AreEqual(3, users.Count, "Expecting the reminder of records if pagesize is 7 and retrieving 2nd page.");

            double durationMiliseconds = DateTime.Now.Subtract(startTest).TotalMilliseconds;
            Console.WriteLine(String.Format("Test Duration: {0} miliseconds.", durationMiliseconds));
        }

    }
}
