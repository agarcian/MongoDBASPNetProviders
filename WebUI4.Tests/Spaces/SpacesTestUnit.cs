using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using AltovientoSolutions.Security.Spaces;
using AltovientoSolutions.DAL.Security;


namespace WebUI4.Tests.Spaces
{
    [TestClass]
    public class SpacesTestUnit
    {
        [TestInitialize]
        public void TestInitialize()
        {
            string name = "MyTestSpace";
            Space space;

            MongoDBSpacesProvider spacesProvider = new MongoDBSpacesProvider();
            space = spacesProvider.GetSpace(name);

            if (space != null)
                spacesProvider.DeleteSpace(name);


            if (spacesProvider.GroupExists("MyGroup"))
                spacesProvider.DeleteGroup("MyGroup", false);
            if (spacesProvider.GroupExists("SubGroupA"))
                spacesProvider.DeleteGroup("SubGroupA", false);
            if (spacesProvider.GroupExists("SubGroupB"))
                spacesProvider.DeleteGroup("SubGroupB", false);
        }


        [TestMethod]
        public void TestConfigurationSettings()
        {
            Assert.AreEqual("/", SpacesConfiguration.Settings.ApplicationName);
            Assert.AreEqual("MongoDB Provider for Spaces", SpacesConfiguration.Settings.Name);
            Assert.AreEqual("MongoSpacesConnStr", SpacesConfiguration.Settings.ConnectionString);
            Assert.AreEqual("This is the implementation of Spaces for MongoDB", SpacesConfiguration.Settings.Description);
            Assert.AreEqual("Spaces_Unit_Test", SpacesConfiguration.Settings.CollectionName);
            Assert.AreEqual("spacesdb", SpacesConfiguration.Settings.DatabaseName);
        }

        [TestMethod]
        public void TestDoesNameExist()
        {
            string applicationName = "/";
            string name = "MyTestSpace";
            bool exists;
            Space space;

            MongoDBSpacesProvider spacesProvider = new MongoDBSpacesProvider();

            exists = spacesProvider.NameExists(name);
            Assert.IsFalse(exists, "This space name should not exist");

            ////////////////////////////////////////////////////////////////

            space = spacesProvider.CreateSpace(name);
            Assert.AreEqual(applicationName, space.ApplicationName);
            Assert.IsNotNull(space.ApiSecret);
            Assert.AreEqual(name, space.SpaceName);
            ////////////////////////////////////////////////////////////////

            exists = spacesProvider.NameExists(name);
            Assert.IsTrue(exists, "This space name should exist");

            space = spacesProvider.GetSpace(name);
            Assert.AreEqual(applicationName, space.ApplicationName);
            Assert.IsNotNull(space.ApiSecret);
            Assert.AreEqual(name, space.SpaceName);
            ////////////////////////////////////////////////////////////////


            bool success = spacesProvider.DeleteSpace(space.SpaceName);
            Assert.IsTrue(success);

        }

        [TestMethod]
        public void TestApiSecretReset()
        {
            string name = "MyTestSpace";
            Space space;

            MongoDBSpacesProvider spacesProvider = new MongoDBSpacesProvider();

            space = spacesProvider.CreateSpace(name);
            string apiSecret = space.ApiSecret;
            ////////////////////////////////////////////////////////////////

            spacesProvider.ResetApiSecret(name);
            space = spacesProvider.GetSpace(name);

            Assert.AreNotEqual(apiSecret, space.ApiSecret);
            ////////////////////////////////////////////////////////////////

            bool success = spacesProvider.DeleteSpace(name);
            Assert.IsTrue(success);

        }


        [TestMethod]
        public void GroupsTest()
        {

            MongoDBSpacesProvider provider = new MongoDBSpacesProvider();

            provider.CreateGroup("MyGroup");
            Assert.IsTrue(provider.GroupExists("MyGroup"));

            provider.DeleteGroup("MyGroup", false);
            Assert.IsFalse(provider.GroupExists("MyGroup"));
            
            provider.CreateGroup("MyGroup");
            provider.CreateGroup("SubGroupA");
            provider.CreateGroup("SubGroupB");

            Assert.IsTrue(provider.GroupExists("MyGroup"));

            provider.AddMembersToGroup("MyGroup", new string[] { "User1", "User2" }, new string[] { "SubGroupA" });


            string[] userInGroup = provider.GetUsersInGroup("MyGroup");
            Assert.AreEqual(2, userInGroup.Length);
            Assert.IsTrue(userInGroup.Contains("User1"));
            Assert.IsTrue(userInGroup.Contains("User2"));


            string[] subgroupsInGroup =provider.GetSubgroupsInGroup("MyGroup");
            Assert.AreEqual(1, subgroupsInGroup.Length);
            Assert.IsTrue(subgroupsInGroup.Contains("SubGroupA"));
            Assert.IsFalse(subgroupsInGroup.Contains("SubGroupB"));

            string[] parentGroups = provider.GetParentGroups("SubGroupA");
            Assert.AreEqual(1, parentGroups.Length);
            Assert.IsTrue(parentGroups.Contains("MyGroup"));




            provider.DeleteGroup("SubGroupA", false);
            subgroupsInGroup = provider.GetSubgroupsInGroup("MyGroup");
            Assert.AreEqual(0, subgroupsInGroup.Length);

            provider.DeleteGroup("MyGroup", false);
            Assert.IsFalse(provider.GroupExists("MyGroup"));

        }

        [TestMethod]
        public void GroupHierarchyTest()
        {
            MongoDBSpacesProvider provider = new MongoDBSpacesProvider();

            provider.CreateGroup("MyGroup");
            provider.CreateGroup("SubGroupA");
            provider.CreateGroup("SubGroupB");

            Assert.IsTrue(provider.GroupExists("MyGroup"));
            Assert.IsTrue(provider.GroupExists("SubGroupA"));
            Assert.IsTrue(provider.GroupExists("SubGroupB"));

            provider.AddMembersToGroup("MyGroup", new string[] { "User1", "User2" }, new string[] { "SubGroupA" });

            provider.AddSubgroupsToGroup("MyGroup", new string[] { "SubGroupB" });

            string[] subgroupsInGroup = provider.GetSubgroupsInGroup("MyGroup");
            Assert.AreEqual(2, subgroupsInGroup.Length);
            Assert.IsTrue(subgroupsInGroup.Contains("SubGroupA"));
            Assert.IsTrue(subgroupsInGroup.Contains("SubGroupB"));

            string[] parentGroups = provider.GetParentGroups("SubGroupA");
            Assert.AreEqual(1, parentGroups.Length);
            Assert.IsTrue(parentGroups.Contains("MyGroup"));

            parentGroups = provider.GetParentGroups("SubGroupB");
            Assert.AreEqual(1, parentGroups.Length);
            Assert.IsTrue(parentGroups.Contains("MyGroup"));

            try
            {
                provider.AddSubgroupsToGroup("UnknownGroup", new string[] { "SubGroupB" });
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc is SpaceProviderException);
            }


            if (provider.GroupExists("SubGroupA"))
                provider.DeleteGroup("SubGroupA", false);
            if (provider.GroupExists("SubGroupB"))
                provider.DeleteGroup("SubGroupB", false);
            if (provider.GroupExists("MyGroup"))
                provider.DeleteGroup("MyGroup", false);

        }




        [TestMethod]
        public void GroupMembershipTest()
        {

            MongoDBSpacesProvider provider = new MongoDBSpacesProvider();

            provider.CreateGroup("MyGroup");
            provider.CreateGroup("SubGroupA");
            provider.CreateGroup("SubGroupB");

            Assert.IsTrue(provider.GroupExists("MyGroup"));
            Assert.IsTrue(provider.GroupExists("SubGroupA"));
            Assert.IsTrue(provider.GroupExists("SubGroupB"));

            provider.AddMembersToGroup("MyGroup", new string[] { "User1", "User2" }, new string[] { "SubGroupA" });
            provider.AddSubgroupsToGroup("MyGroup", new string[] { "SubGroupB" });

            provider.AddUsernamesToGroup("SubGroupA", new string[] {"User1"});
            Assert.IsTrue(provider.IsUserInGroup("SubGroupA", "User1"));
            

            Assert.IsTrue(provider.IsUserInGroup("MyGroup", "User1"));
            Assert.IsTrue(provider.IsUserInGroup("MyGroup", "User2"));
            Assert.IsFalse(provider.IsUserInGroup("SubGroupB", "User1"));


            try
            {
                provider.AddSubgroupsToGroup("UnknownGroup", new string[] { "SubGroupB" });
            }
            catch (Exception exc)
            {
                Assert.IsTrue(exc is SpaceProviderException);
            }






            provider.DeleteGroup("SubGroupA", false);
            provider.DeleteGroup("SubGroupB", false);
            provider.DeleteGroup("MyGroup", false);

        }

    }
}
