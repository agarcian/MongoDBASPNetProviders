using System;
using System.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using AltovientoSolutions.Security.Spaces;
using AltovientoSolutions.Security.Instrumentation;
using System.Web.Management;

namespace AltovientoSolutions.DAL.Security
{
    public class MongoDBSpacesProvider : SpacesProvider
    {
        #region Static fields.
        private static string connectionString = ConfigurationManager.ConnectionStrings[SpacesConfiguration.Settings.ConnectionString].ConnectionString;
        private static string databaseName = SpacesConfiguration.Settings.DatabaseName;
        private static string collectionName = SpacesConfiguration.Settings.CollectionName;
        #endregion

        #region Constructors
        public MongoDBSpacesProvider()
            : base()
        {
            if (!BsonClassMap.IsClassMapRegistered(typeof(Space)))
            {
                BsonClassMap.RegisterClassMap<Space>(cm =>
                {
                    cm.AutoMap();
                    cm.MapIdProperty(c => c.Id);
                    cm.IdMemberMap.SetIdGenerator(CombGuidGenerator.Instance);
                });
            }
        }

        public MongoDBSpacesProvider(string ApplicationName)
            : base(ApplicationName)
        {
        }
        #endregion

        #region Properties

        public override string Description
        {
            get
            {
                return "MongoDB Spaces Provider";
            }
        }
        public override string Name
        {
            get
            {
                return "An implementation of the Spaces Provider";
            }
        }
        #endregion

        #region Spaces management

        public override bool NameExists(string Name)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            var query = Query.And(Query.EQ("SpaceName", Name),
                Query.EQ("ApplicationName", this.applicationName),
                Query.EQ("RecordType", Space.RECORD_TYPE));

            var doc = collection.FindOneAs(typeof(Space), query);

            return (doc != null);
        }

        public override Space CreateSpace(string Name)
        {
            Space space = new Space();

            // Create in the database the new space.

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            if (NameExists(Name))
            {
                throw new SpaceProviderException(String.Format("A Space with name '{0}' for the ApplicationName '{1}' already exists.", Name, this.applicationName));
            }

            space.Name = Name;
            space.ApplicationName = this.applicationName;
            space.ApiSecret = Guid.NewGuid().ToString();

            SafeModeResult result = collection.Save(space, SafeMode.True);

            if (!result.Ok)
            {
                throw new SpaceProviderException();
            }

            return GetSpace(Name);
        }

        public override Space GetSpace(string Name)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            var query = Query.And(Query.EQ("SpaceName", Name),
                Query.EQ("ApplicationName", this.applicationName),
                Query.EQ("RecordType", Space.RECORD_TYPE));


            Space doc = (Space)collection.FindOneAs(typeof(Space), query);
            return doc;

        }

        public override bool SaveSpace(Space Space)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            BsonDocument doc = BsonDocument.Create(Space);

            // Removes the ApiSecret so it cannot be changed on a Save.  See reset ApiSecret.
            doc.Remove("ApiSecret");

            SafeModeResult result = collection.Save(doc, SafeMode.True);

            return result.Ok;
        }

        public override bool DeleteSpace(string Name)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            var query = Query.EQ("SpaceName", Name);

            SafeModeResult result = collection.Remove(query, RemoveFlags.None, SafeMode.True);

            return result.Ok;
        }

        public override void ResetApiSecret(string Name)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            var query = Query.And(Query.EQ("SpaceName", Name),
                Query.EQ("ApplicationName", this.applicationName),
                Query.EQ("RecordType", Space.RECORD_TYPE));

            collection.FindAndModify(query, SortBy.Null, Update.Set("ApiSecret", Guid.NewGuid().ToString()), true, false);

        }
        #endregion

        #region Group management

        public override bool GroupExists(string groupName)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            try
            {
                var query = Query.And(Query.EQ("ApplicationName", this.applicationName),
                     Query.EQ("GroupName", groupName),
                     Query.EQ("RecordType", Group.RECORD_TYPE));

                int count = collection.Count(query);

                return count > 0;

            }
            catch (ApplicationException e)
            {
                (new SpacesProviderErrorEvent("GroupExists", this, WebEventCodes.WebExtendedBase + 1, e)).Raise();
                throw;
            }
        }

        public override void CreateGroup(string groupname)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            try
            {

                if (String.IsNullOrWhiteSpace(groupname))
                    throw new SpaceProviderException("Group name cannot be empty or null.");
                if (groupname.Contains(","))
                    throw new ArgumentException("Group names cannot contain commas.");
                if (GroupExists(groupname))
                    throw new SpaceProviderException("Group name already exists.");
                if (groupname.Length > 255)
                    throw new SpaceProviderException("Group name cannot exceed 255 characters.");


                Group group = new Group() { GroupName = groupname, ApplicationName = this.applicationName };

                bool bSuccess = collection.Save(group, SafeMode.True).Ok;

                if (!bSuccess)
                    throw new SpaceProviderException(String.Format("Failed to create new group '{0}'.", groupname));

            }
            catch (ApplicationException e)
            {
                (new SpacesProviderErrorEvent("CreateGroup", this, WebEventCodes.WebExtendedBase + 1, e)).Raise();
                throw;
            }
        }

        public override string[] GetUsersInGroup(string groupName)
        {
            string[] usernames;
            string[] subgroups;

            GetMembersInGroup(groupName, out usernames, out subgroups);
            return usernames;
        }

        public override string[] GetSubgroupsInGroup(string groupName)
        {
            string[] usernames;
            string[] subgroups;

            GetMembersInGroup(groupName, out usernames, out subgroups);
            return subgroups;
        }

        public override void GetMembersInGroup(string groupName, out string[] usernames, out string[] subgroups)
        {
            List<String> lstUsernames = new List<string>();
            List<String> lstSubgroups = new List<string>();

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            try
            {
                if (String.IsNullOrWhiteSpace(groupName))
                    throw new SpaceProviderException("Group name cannot be empty or null.");

                if (!GroupExists(groupName))
                    throw new SpaceProviderException("Group does not exist.");

                var query = Query.And(Query.EQ("ApplicationName", this.applicationName),
                     Query.EQ("ParentGroup", groupName),
                     Query.EQ("RecordType", GroupHierarchy.RECORD_TYPE));

                var grpHierarchy = collection.FindOneAs<GroupHierarchy>(query);
                if (grpHierarchy != null)
                {
                    lstSubgroups.AddRange(grpHierarchy.Subgroups.AsEnumerable<String>());
                    lstUsernames.AddRange(grpHierarchy.Usernames.AsEnumerable<String>());
                }
            }
            catch (ApplicationException e)
            {
                (new SpacesProviderErrorEvent("GetMembersInGroup", this, WebEventCodes.WebExtendedBase + 1, e)).Raise();
            }
            finally
            {
            }

            usernames = lstUsernames.ToArray();
            subgroups = lstSubgroups.ToArray();
            return;
        }

        public override bool IsUserInGroup(string groupName, string username)
        {
            return IsUserInGroup(groupName, username, false);
        }
        public override bool IsUserInGroup(string groupName, string username, bool multipleLevels)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            try
            {
                var query = Query.And(Query.EQ("ApplicationName", this.applicationName),
                Query.EQ("Usernames", username),
                Query.EQ("ParentGroup", groupName),
                Query.EQ("RecordType", GroupHierarchy.RECORD_TYPE));

                var cursor = collection.FindAs<GroupHierarchy>(query);

                return cursor.Count() > 0;

            }
            catch (ApplicationException e)
            {
                (new SpacesProviderErrorEvent("IsUserInGroup", this, WebEventCodes.WebExtendedBase + 1, e)).Raise();
                throw;
            }
        }
        public override string[] GetParentGroups(string groupName)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);


            List<String> parentGroups = new List<String>();
            try
            {
                var query = Query.And(Query.EQ("ApplicationName", this.applicationName),
                    Query.EQ("Subgroups", groupName),
                    Query.EQ("RecordType", GroupHierarchy.RECORD_TYPE));

                var cursor = collection.FindAs<GroupHierarchy>(query);

                foreach (GroupHierarchy hierarchy in cursor)
                {
                    parentGroups.Add(hierarchy.ParentGroup);
                    parentGroups.AddRange(GetParentGroups(hierarchy.ParentGroup)); // iterate thru multiple levels
                }
            }
            catch (ApplicationException e)
            {
                (new SpacesProviderErrorEvent("GetParentGroups", this, WebEventCodes.WebExtendedBase + 1, e)).Raise();
                throw;
            }

            return parentGroups.ToArray();
        }

        public override void AddUsernamesToGroup(string groupName, string[] usernames)
        {
            AddMembersToGroup(groupName, usernames, new string[] { });
        }

        public override void AddSubgroupsToGroup(string groupName, string[] subgroups)
        {
            AddMembersToGroup(groupName, new string[] { }, subgroups);
        }

        public override void AddMembersToGroup(string groupName, string[] usernames, string[] subgroups)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            try
            {

                if (!GroupExists(groupName))
                    throw new SpaceProviderException("Group " + groupName + " name not found.");

                foreach (string groupname in subgroups)
                {
                    if (String.IsNullOrWhiteSpace(groupname))
                        throw new SpaceProviderException("Group name cannot be empty or null.");

                    if (!GroupExists(groupname))
                        throw new SpaceProviderException("Group " + groupname + " name not found.");
                }

                foreach (string username in usernames)
                {
                    if (String.IsNullOrWhiteSpace(username))
                        throw new SpaceProviderException("User name cannot be empty or null.");

                    if (username.Contains(","))
                        throw new ArgumentException("User names cannot contain commas.");

                }


                var query = Query.And(Query.EQ("ApplicationName", this.applicationName),
                    Query.EQ("RecordType", GroupHierarchy.RECORD_TYPE),
                    Query.EQ("ParentGroup", groupName));

                GroupHierarchy hierarchy = collection.FindOneAs<GroupHierarchy>(query);

                if (hierarchy == null)
                {
                    hierarchy = new GroupHierarchy() { ParentGroup = groupName, ApplicationName = this.applicationName };
                }

                //Add to the list the usernames that are not already in the list.
                hierarchy.Usernames.AddRange(
                    (from u in usernames
                     where !hierarchy.Usernames.Contains(u)
                     select u).ToList<String>());


                // add the subgroups that are not already in the list and that can become subgroups (don't add circular reference).
                hierarchy.Subgroups.AddRange((from s in subgroups
                                              where !hierarchy.Subgroups.Contains(s) && CanBecomeSubgroup(groupName, s)
                                              select s).ToList<String>());


                collection.Save(hierarchy);


            }
            catch (ApplicationException e)
            {
                (new SpacesProviderErrorEvent("AddMembersToGroup", this, WebEventCodes.WebExtendedBase + 1, e)).Raise();
                throw;
            }
        }

        public override bool CanBecomeSubgroup(string ParentGroup, string SubGroup)
        {
            //return false;  // if want to disable group hierarchies, return false.

            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            try
            {
                // Check that SubGroup does not have subgroups.
                if (GetSubgroupsInGroup(SubGroup).Length > 0)
                {
                    throw new SpaceProviderException("Only one-level hierarchy is allowed.  Attempting to add a subgroup which already has subgroups on its own.");
                }

                // Check that ParentGroup don't have parents.
                if (GetParentGroups(ParentGroup).Length > 0)
                {
                    throw new SpaceProviderException("Only one-level hierarchy is allowed.  Attempting to add a subgroup which already has subgroups on its own.");
                }

                // Check that SubGroup does not belong to another parent group.  Don't allow multiple hierarchy.
                if (GetParentGroups(SubGroup).Length > 0)
                {
                    throw new SpaceProviderException("Groups can only belong to one subgroup.  Multiple 'parents' are not allowed in this implementation");
                }

            }
            catch (ApplicationException e)
            {
                (new SpacesProviderErrorEvent("CanBecomeSubgroup", this, WebEventCodes.WebExtendedBase + 1, e)).Raise();
                throw;
            }


            return true;
        }

        public override bool DeleteGroup(string groupName, bool throwOnPopulatedRole)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);


            bool bSuccess = false;

            try
            {
                if (!GroupExists(groupName))
                {
                    throw new SpaceProviderException("Group does not exist.");
                }

                if (throwOnPopulatedRole && GetUsersInGroup(groupName).Length > 0)
                {
                    throw new SpaceProviderException("Cannot delete a populated group.");
                }

                // Remove the Group.
                var query = Query.And(Query.EQ("ApplicationName", this.applicationName),
                    Query.EQ("GroupName", groupName),
                    Query.EQ("RecordType", Group.RECORD_TYPE));

                bSuccess = collection.FindAndRemove(query, SortBy.Null).Ok;

                // Remove the hierarchy for which the group is a parent.
                var query2 = Query.And(Query.EQ("ApplicationName", this.applicationName),
                    Query.EQ("ParentGroup", groupName),
                    Query.EQ("RecordType", GroupHierarchy.RECORD_TYPE));

                bSuccess = collection.FindAndRemove(query2, SortBy.Null).Ok;

                // Remove the membership from any group.
                var query3 = Query.And(Query.EQ("ApplicationName", this.applicationName),
                    Query.EQ("Subgroups", groupName),
                    Query.EQ("RecordType", GroupHierarchy.RECORD_TYPE));

                bSuccess = collection.FindAndModify(query3, SortBy.Null, Update.Pull("Subgroups", groupName), true).Ok;

            }
            catch (ApplicationException e)
            {
                (new SpacesProviderErrorEvent("DeleteGroup", this, WebEventCodes.WebExtendedBase + 1, e)).Raise();
                throw;
            }

            return bSuccess;
        }

        private List<Group> GetGroups(List<String> groupnames)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            var query = Query.And(Query.EQ("ApplicationName", this.applicationName),
                Query.EQ("RecordType", Group.RECORD_TYPE),
                Query.In("GroupName", BsonArray.Create(groupnames)));

            List<Group> groups = collection.FindAs<Group>(query).ToList<Group>();

            return groups;
        }

        public override void GetSecurityTokensFromGroupsForUser(string userName)
        {
            MongoServer server = MongoServer.Create(connectionString); // connect to the mongoDB url.
            MongoDatabase db = server.GetDatabase(databaseName);
            MongoCollection collection = db.GetCollection(collectionName);

            try
            {
                var query = Query.And(Query.EQ("ApplicationName", this.applicationName),
                    Query.EQ("Usernames", userName),
                    Query.EQ("RecordType", GroupHierarchy.RECORD_TYPE));

                var cursor = collection.FindAs<GroupHierarchy>(query);

                List<String> ParentGroups = new List<string>();
                List<String> SecurityTokens = new List<string>();

                foreach (GroupHierarchy hierarchy in cursor)
                {
                    // Keep track of the parent groups
                    ParentGroups.Add(hierarchy.ParentGroup);
                }

                List<Group> groups = GetGroups(ParentGroups);
                foreach (Group g in groups)
                {
                    // Gets all the security tokens from all the parent groups.
                    SecurityTokens.AddRange(g.SecurityTokens);
                }

            }
            catch (ApplicationException e)
            {
                (new SpacesProviderErrorEvent("DeleteGroup", this, WebEventCodes.WebExtendedBase + 1, e)).Raise();
            }
        }

        #endregion

    }
}
