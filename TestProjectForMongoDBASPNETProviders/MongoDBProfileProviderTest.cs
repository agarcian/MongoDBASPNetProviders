using System;
using System.Configuration;
using ASPNETProvidersForMongoDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;


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
        public void MyTest()
        {
            DateTime startTest = DateTime.Now;




            double durationMiliseconds = DateTime.Now.Subtract(startTest).TotalMilliseconds;
            Console.WriteLine(String.Format("Test Duration: {0} miliseconds.", durationMiliseconds));
        }


    }
}
