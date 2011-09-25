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
            mongoProvider = (MongoDBMembershipProvider) Membership.Provider;
        }

        [TestMethod]
        public void CreateUserTest()
        {
            DateTime startTest = DateTime.Now;

            string username = "username1";
            string email = "user1@example.com";
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

            Assert.IsNotNull(user);
            Assert.AreEqual(status, MembershipCreateStatus.Success);


            // Create a user with the same username.
            user = mongoProvider.CreateUser(username, password, "AnotherEmail@example.com",
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNull(user);
            Assert.AreEqual(status, MembershipCreateStatus.DuplicateUserName);


            // Create a user with the same username.
            user = mongoProvider.CreateUser("newUserName", password, email,
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNull(user);
            Assert.AreEqual(status, MembershipCreateStatus.DuplicateEmail);




            // Clean up after yourself, please...
            user = mongoProvider.GetUser(username, false); 
            if (user != null)
            {
                bool bSuccess = mongoProvider.DeleteUser(username, true);
                Assert.IsTrue(bSuccess);
            }

            double durationMiliseconds = DateTime.Now.Subtract(startTest).TotalMilliseconds;
            Console.WriteLine(String.Format("Test Duration: {0} miliseconds.", durationMiliseconds));
        }

        [TestMethod]
        public void GetAllUsersTest()
        {
            DateTime startTest = DateTime.Now;

            string username = "username";
            string email = "user1@example.com";
            string password = "12345678";
            string question = "What is MongoDB?";
            string answer = "A cool NoSQL database";
            bool isApproved = true;
            MembershipUser user;


            MembershipCreateStatus status;

            // Make sure no users where left behind in a previous failed test.
            bool bSuccess = mongoProvider.DeleteUser(username + "1", true);
            bSuccess = mongoProvider.DeleteUser(username + "2", true);
            bSuccess = mongoProvider.DeleteUser(username + "3", true);
            bSuccess = mongoProvider.DeleteUser(username + "4", true);
            bSuccess = mongoProvider.DeleteUser(username + "5", true);
            bSuccess = mongoProvider.DeleteUser(username + "6", true);
            bSuccess = mongoProvider.DeleteUser(username + "7", true);



            // No users at all expected in the database...
            int totalRecords = 0;
            MembershipUserCollection users = mongoProvider.GetAllUsers(1, 5, out totalRecords);
            Assert.AreEqual(0, totalRecords);




            // Create a user with the same username.

            user = mongoProvider.CreateUser(username + "1", password, "1" + email, question, answer, isApproved, Guid.NewGuid(), out status);
            Assert.IsNotNull(user); Assert.AreEqual(status, MembershipCreateStatus.Success);

            user = mongoProvider.CreateUser(username + "2", password, "2" + email, question, answer, isApproved, Guid.NewGuid(), out status);
            Assert.IsNotNull(user); Assert.AreEqual(status, MembershipCreateStatus.Success);

            user = mongoProvider.CreateUser(username + "3", password, "3" + email, question, answer, isApproved, Guid.NewGuid(), out status);
            Assert.IsNotNull(user); Assert.AreEqual(status, MembershipCreateStatus.Success);

            user = mongoProvider.CreateUser(username + "4", password, "4" + email, question, answer, isApproved, Guid.NewGuid(), out status);
            Assert.IsNotNull(user); Assert.AreEqual(status, MembershipCreateStatus.Success);

            user = mongoProvider.CreateUser(username + "5", password, "5" + email, question, answer, isApproved, Guid.NewGuid(), out status);
            Assert.IsNotNull(user); Assert.AreEqual(status, MembershipCreateStatus.Success);

            user = mongoProvider.CreateUser(username + "6", password, "6" + email, question, answer, isApproved, Guid.NewGuid(), out status);
            Assert.IsNotNull(user); Assert.AreEqual(status, MembershipCreateStatus.Success);

            user = mongoProvider.CreateUser(username + "7", password, "7" + email, question, answer, isApproved, Guid.NewGuid(), out status);
            Assert.IsNotNull(user); Assert.AreEqual(status, MembershipCreateStatus.Success);



            totalRecords = 0;
            users = mongoProvider.GetAllUsers(1, 5, out totalRecords);
            Assert.AreEqual(7, totalRecords);
            Assert.AreEqual(5, users.Count);
            
            totalRecords = 0;
            users = mongoProvider.GetAllUsers(2, 5, out totalRecords);
            Assert.AreEqual(7, totalRecords);
            Assert.AreEqual(2, users.Count);

            bSuccess = mongoProvider.DeleteUser(username + "1", true);
            Assert.IsTrue(bSuccess);
            bSuccess = mongoProvider.DeleteUser(username + "2", true);
            Assert.IsTrue(bSuccess);
            bSuccess = mongoProvider.DeleteUser(username + "3", true);
            Assert.IsTrue(bSuccess);
            bSuccess = mongoProvider.DeleteUser(username + "4", true);
            Assert.IsTrue(bSuccess);
            bSuccess = mongoProvider.DeleteUser(username + "5", true);
            Assert.IsTrue(bSuccess);
            bSuccess = mongoProvider.DeleteUser(username + "6", true);
            Assert.IsTrue(bSuccess);
            bSuccess = mongoProvider.DeleteUser(username + "7", true);
            Assert.IsTrue(bSuccess);

            double durationMiliseconds = DateTime.Now.Subtract(startTest).TotalMilliseconds;
            Console.WriteLine(String.Format("Test Duration: {0} miliseconds.", durationMiliseconds));
            
        }

    }
}
