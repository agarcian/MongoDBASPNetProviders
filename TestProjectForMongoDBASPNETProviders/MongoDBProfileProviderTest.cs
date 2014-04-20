using System;
using System.Configuration;
using System.Web.Profile;
using System.Web.Security;
using ASPNETProvidersForMongoDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace TestProjectForMongoDBASPNETProviders
{
    [TestClass]
    public class MongoDBProfileProviderTest
    {

        static string usernameKey = "username";
        static int numberOfUsers = 100;



        static string mongoDBConnString = ConfigurationManager.ConnectionStrings["MongoProvidersDBConnStr"].ConnectionString;
        static MongoDBMembershipProvider mongoProvider = new MongoDBMembershipProvider();

        /// <summary>
        /// Runs once before any test in this class runs and prepopulates the database with some values to be used in the tests.
        /// </summary>
        /// <remarks>
        /// Initializes a number of users in the form of usernamei:passwordi where i is a number.   These are going to be users in the database.
        /// </remarks>
        [ClassInitialize]
        public static void InitializeTestClass(TestContext context)
        {
            // Removes from the database the 10 sample users.
            for (int i = 1; i <= numberOfUsers; i++)
            {
                ProfileManager.DeleteProfile(usernameKey + i);
            }
        }

        /// <summary>
        /// Runs once after all test in this class runs and destroys any prepopulated values from the database.
        /// </summary>
        [ClassCleanup]
        public static void CleanupTestClass()
        {
            // Removes from the database the 10 sample users.
            for (int i = 1; i <= numberOfUsers; i++)
            {
                ProfileManager.DeleteProfile(usernameKey + i);
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
            
        }

        /// <summary>
        /// Destroys users that should no longer exist.
        /// </summary>
        [TestCleanup]
        public void CleanupTest()
        {
            // Clear the values of the profiles.  There is no Delete in the ProfileCommon class.
            for (int i = 1; i <= numberOfUsers; i++)
            {
                ProfileManager.DeleteProfile(usernameKey + i);
            }
        }

        [TestMethod]
        public void CreateProfileTest()
        {
            string username;
            ProfileCommon profile;

            for (int i = 0; i < numberOfUsers; i++)
            {
                username = usernameKey + i;
                profile = ProfileCommon.Create(username) as ProfileCommon;
                profile.FirstName = "FirstName" + i;
                profile.LastName = "LastName" + i;

                profile.Save();
            }

            for (int i = 0; i < numberOfUsers; i++)
            {
                username = usernameKey + i;
                profile = ProfileCommon.GetProfile(username) as ProfileCommon;
                Assert.AreEqual("FirstName" + i, profile.FirstName);
                Assert.AreEqual("LastName" + i, profile.LastName);

                // To Do: Test all the other properties.
            }


        }


        [TestMethod]
        public void DeleteProfileTest()
        {
            string username;
            ProfileCommon profile;

            for (int i = 0; i < numberOfUsers; i++)
            {
                username = usernameKey + i;
                profile = ProfileCommon.Create(username) as ProfileCommon;
                profile.FirstName = "FirstName" + i;
                profile.LastName = "LastName" + i;

                profile.Save();
            }

            for (int i = 0; i < numberOfUsers; i++)
            {
                username = usernameKey + i;

                ProfileManager.DeleteProfile(username);
      
                profile = ProfileCommon.GetProfile(username) as ProfileCommon;
                Assert.AreEqual(String.Empty, profile.FirstName);
                Assert.AreEqual(String.Empty, profile.LastName);

                // To Do: Test all the other properties.
            }


        }
    }
}
