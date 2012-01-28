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
    public class MongoDBProfileProviderTest
    {
        static string mongoDBConnString = ConfigurationManager.ConnectionStrings["MongoProvidersDBConnStr"].ConnectionString;
        static MongoDBMembershipProvider mongoProvider = new MongoDBMembershipProvider();

        static string[] commonUsernamesUsedInTests = new string[] { "usernamexyz" };

        /// <summary>
        /// Runs once before any test in this class runs and prepopulates the database with some values to be used in the tests.
        /// </summary>
        /// <remarks>
        /// Initializes a number of users in the form of usernamei:passwordi where i is a number.   These are going to be users in the database.
        /// </remarks>
        [ClassInitialize]
        public static void InitializeTestClass(TestContext context)
        {
            
            // Clean up common usernames used accross the different tests.
            foreach (string username in commonUsernamesUsedInTests)
            {
                ProfileCommon profile = ProfileCommon.GetProfile(username);

                if (profile != null)
                {
                    System.Web.Profile.ProfileManager.DeleteProfile(username);
                }
            }
        }

        /// <summary>
        /// Runs once after all test in this class runs and destroys any prepopulated values from the database.
        /// </summary>
        [ClassCleanup]
        public static void CleanupTestClass()
        {
            // Removes from the database the 10 sample users.
            for (int i = 1; i <= 10; i++)
            {
                System.Web.Profile.ProfileManager.DeleteProfile("username" + i);
            }
        }

        /// <summary>
        /// Destroys users that need to be non-existant in the individual tests.
        /// </summary>
        /// <remarks>
        /// The objects being removed should not exist.  They exist only if a previous test failed.
        /// </remarks>
        [TestInitialize]
        public void InitializeTest()
        {
            // Initializes the database with 10 sample users.
            for (int i = 1; i <= 10; i++)
            {
                ProfileCommon.Create("username" + i, true);
            }
        }

        /// <summary>
        /// Destroys users that should no longer exist.
        /// </summary>
        [TestCleanup]
        public void CleanupTest()
        {
            foreach (string username in commonUsernamesUsedInTests)
            {
                if (!String.IsNullOrWhiteSpace(username))
                {
                    System.Web.Profile.ProfileManager.DeleteProfile(username);
                }
            }
        }

        [TestMethod]
        public void CreateUserTest()
        {
            DateTime startTest = DateTime.Now;

            string username = "usernamexyz";
            string upperCaseUsername = "UsernameXYZ";
            string email = "userxyz@example.com";
            string upperCaseEmail = "UserXYZ@example.com";
            string password = "12345678";
            string question = "What is MongoDB?";
            string answer = "A cool NoSQL database";
            bool isApproved = true;

            MembershipUser user;
            MembershipCreateStatus status;

            // Create a user.  This is a valid call.
            user = mongoProvider.CreateUser(username, password, email,
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNotNull(user, "A user should have been created");
            Assert.AreEqual(username, user.UserName, "Username property does not coincide");
            Assert.AreEqual(question, user.PasswordQuestion, "Question property does not coincide");


            Assert.AreEqual(status, MembershipCreateStatus.Success, "The creation should have been successful");


            // Create a user with the same username.
            user = mongoProvider.CreateUser(username, password, "AnotherEmail@example.com",
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNull(user, "No user should have been created since the username is already taken");
            Assert.AreEqual(status, MembershipCreateStatus.DuplicateUserName, "Expecting a DuplicateUserName status");

            // Create a user with the same username but this time with different case
            user = mongoProvider.CreateUser(upperCaseUsername, password, "AnotherEmail@example.com",
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNull(user, "No user should have been created since the username is already taken");
            Assert.AreEqual(status, MembershipCreateStatus.DuplicateUserName, "Expecting a DuplicateUserName status");


            // Create a user with a different same username but the same email.
            user = mongoProvider.CreateUser("newUserName", password, email,
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNull(user, "No user should have been created since the email is already taken");
            Assert.AreEqual(status, MembershipCreateStatus.DuplicateEmail, "Expecting DuplicateEmail status");

            // Create a user with a different same username but the same email.
            user = mongoProvider.CreateUser("newUserName", password, upperCaseEmail,
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNull(user, "No user should have been created since the email is already taken");
            Assert.AreEqual(status, MembershipCreateStatus.DuplicateEmail, "Expecting DuplicateEmail status");








            double durationMiliseconds = DateTime.Now.Subtract(startTest).TotalMilliseconds;
            Console.WriteLine(String.Format("Test Duration: {0} miliseconds.", durationMiliseconds));
        }

        [TestMethod]
        public void DeleteUserTest()
        {
            DateTime startTest = DateTime.Now;
            bool success;
            MembershipUser user;

            ///// Delete a user that does not exist ////////////////////////////////////////////////////////////
            user = mongoProvider.GetUser("unknownUser", false);
            Assert.IsNull(user, "Should not be able to retrieve this user");

            success = mongoProvider.DeleteUser("unknownUser", true);
            Assert.IsTrue(success, "By definition deleting a non existing user should succeed.");
            ///////////////////////////////////////////////////////////////////////////////////////////////////

            ///// Delete an existing user /////////////////////////////////////////////////////////////////////
            success = mongoProvider.DeleteUser("username1", true);
            Assert.IsTrue(success, "Should have been able to delete the user");

            user = mongoProvider.GetUser("username1", false);
            Assert.IsNull(user, "Should not be able to retrieve this user");
            ///// Delete an existing user /////////////////////////////////////////////////////////////////////


            ///// Delete an existing user with alternate casing ////////////////////////////////////////////////
            success = mongoProvider.DeleteUser("USERName2", true);
            Assert.IsTrue(success, "Should have been able to delete the user");

            user = mongoProvider.GetUser("username2", false);
            Assert.IsNull(user, "Should not be able to retrieve this user");

            user = mongoProvider.GetUser("username2", true);
            Assert.IsNull(user, "Should not be able to retrieve this user");

            user = mongoProvider.GetUser("USERName2", false);
            Assert.IsNull(user, "Should not be able to retrieve this user");

            user = mongoProvider.GetUser("USERName2", true);
            Assert.IsNull(user, "Should not be able to retrieve this user");

            ///// Delete an existing user /////////////////////////////////////////////////////////////////////
            


            double durationMiliseconds = DateTime.Now.Subtract(startTest).TotalMilliseconds;
            Console.WriteLine(String.Format("Test Duration: {0} miliseconds.", durationMiliseconds));
        }

        [TestMethod]
        public void ManipulateQuestionAndAnswerTest()
        {
            DateTime startTest = DateTime.Now;

            string username = "usernamexyz";
            string uppercaseUsername = "usernamexyz";
            string email = "userxyz@example.com";
            string password = "12345678";
            string question = "What is MongoDB?";
            string newQuestion = "Is this a new Question?";
            string answer = "A cool NoSQL database";
            string newAnswer = "yes";
            bool isApproved = true;

            MembershipUser user;
            MembershipCreateStatus status;
            bool success;

            // Create a user.  This is a valid call.
            user = mongoProvider.CreateUser(username, password, email,
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.AreEqual(status, MembershipCreateStatus.Success, "The creation should have been successful");
            Assert.AreEqual(question, user.PasswordQuestion, "Question property does not coincide");

            /// Validate that the question can be updated. ///////////////////////////////////////////////////////

            success = user.ChangePasswordQuestionAndAnswer(password, newQuestion, newAnswer);
            Assert.IsTrue(success, "This action should have succeeded");

            // Retrieve the user to validate the question.  Use the two variations of the method for good coverage, as well as using lower and upper case for the username.
            user = mongoProvider.GetUser(username, false);
            Assert.AreEqual(newQuestion, user.PasswordQuestion, "Question property does not coincide");

            user = mongoProvider.GetUser(username, true);
            Assert.AreEqual(newQuestion, user.PasswordQuestion, "Question property does not coincide");

            // Retrieve the user to validate the question.  Use the two variations of the method for good coverage.
            user = mongoProvider.GetUser(uppercaseUsername, false);
            Assert.AreEqual(newQuestion, user.PasswordQuestion, "Question property does not coincide");

            user = mongoProvider.GetUser(uppercaseUsername, true);
            Assert.AreEqual(newQuestion, user.PasswordQuestion, "Question property does not coincide");

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            double durationMiliseconds = DateTime.Now.Subtract(startTest).TotalMilliseconds;
            Console.WriteLine(String.Format("Test Duration: {0} miliseconds.", durationMiliseconds));
        }

        [TestMethod]
        public void PasswordRetrievalTest()
        {
            DateTime startTest = DateTime.Now;

            string username = "usernamexyz";
            string uppercaseUsername = "usernamexyz";
            string email = "userxyz@example.com";
            string password = "12345678";
            string question = "What is MongoDB?";
            string answer = "A cool NoSQL database";
            bool isApproved = true;

            MembershipUser user;
            MembershipCreateStatus status;

            // Create a user.  This is a valid call.
            user = mongoProvider.CreateUser(username, password, email,
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.AreEqual(status, MembershipCreateStatus.Success, "The creation should have been successful");
            Assert.AreEqual(question, user.PasswordQuestion, "Question property does not coincide");

            
            /// Validate that the password cannot be retrieved with the answer //////////////////////////////////////

            string retrievedPassword;

            if (mongoProvider.PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                try
                {
                    retrievedPassword = mongoProvider.GetPassword(username, answer);
                    Assert.Fail("Should have raised an exception in the previous line.");
                }
                catch (System.Configuration.Provider.ProviderException exc)
                {
                    Assert.AreEqual("Cannot retrieve Hashed passwords.", exc.Message);
                }
            }
            else
            {
                if (!mongoProvider.EnablePasswordRetrieval)
                {
                    try
                    {
                        retrievedPassword = mongoProvider.GetPassword(username, answer);
                        Assert.Fail("Should have raised an exception in the previous line.");
                    }
                    catch (System.Configuration.Provider.ProviderException exc)
                    {
                        Assert.AreEqual("Password Retrieval Not Enabled.", exc.Message);
                    }
                }
                else
                {
                    retrievedPassword = mongoProvider.GetPassword(username, answer);
                    Assert.AreEqual(password, retrievedPassword);
                }
            }
            
           

            //////////////////////////////////////////////////////////////////////////////////////////////////////








            double durationMiliseconds = DateTime.Now.Subtract(startTest).TotalMilliseconds;
            Console.WriteLine(String.Format("Test Duration: {0} miliseconds.", durationMiliseconds));
        }

        [TestMethod]
        public void PasswordChangeTest()
        {
            DateTime startTest = DateTime.Now;

            MembershipUser user;
            MembershipCreateStatus status;
            bool success;

            string username = "usernamexyz";
            string upperCaseUsername = "UsernameXYZ";
            string email = "userxyz@example.com";
            string upperCaseEmail = "UserXYZ@example.com";
            string password = "12345678";
            string newPassword = "newPasswordXYZ";
            string question = "What is MongoDB?";
            string answer = "A cool NoSQL database";
            bool isApproved = true;

            // Create a user.  This is a valid call.
            user = mongoProvider.CreateUser(username, password, email,
                question, answer, isApproved, Guid.NewGuid(), out status);

            Assert.IsNotNull(user, "A user should have been created");


            /// Validate that the password cannot be changed /////////////////////////////////////////////////
            user = mongoProvider.GetUser(username, false);

            success = user.ChangePassword(password, newPassword);
            Assert.IsTrue(success, "Should have been successful");

            user = mongoProvider.GetUser(username, false);

            success = mongoProvider.ValidateUser(username, password);
            Assert.IsFalse(success, "Should not have been successful");

            success = mongoProvider.ValidateUser(username, newPassword);
            Assert.IsTrue(success, "Should have been successful");

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            /// Validate that the password cannot be changed with different casing ///////////////////////////////
            user = mongoProvider.GetUser(username.ToUpper(), false);

            success = mongoProvider.ValidateUser(username, password);
            Assert.IsFalse(success, "Should not have been successful");

            success = mongoProvider.ValidateUser(username, newPassword);
            Assert.IsTrue(success, "Should have been successful");

            success = mongoProvider.ValidateUser(username.ToUpper(), newPassword);
            Assert.IsTrue(success, "Should have been successful");

            //////////////////////////////////////////////////////////////////////////////////////////////////////

            /// Validate that passwords are case sensitive ///////////////////////////////////////////////////////
            user = mongoProvider.GetUser(username, false);

            success = mongoProvider.ValidateUser(username, newPassword);
            Assert.IsTrue(success, "Should have been successful");

            success = mongoProvider.ValidateUser(username, newPassword.ToUpper());
            Assert.IsFalse(success, "Should not have been successful");

            //////////////////////////////////////////////////////////////////////////////////////////////////////








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
