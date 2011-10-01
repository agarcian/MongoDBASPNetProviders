using System;
using System.Configuration;
using System.Configuration.Provider;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AltovientoSolutions.Security;


namespace AltovientoSolutions.Security.Spaces
{
    public abstract class SpacesProvider : ProviderBase
    {
        protected string applicationName;

        public SpacesProvider()
        {
            applicationName = SpacesConfiguration.Settings.ApplicationName;
        }

        public SpacesProvider(string ApplicationName)
        {
            applicationName = ApplicationName;
        }

        public override string Description
        {
            get
            {
                if (String.IsNullOrEmpty(SpacesConfiguration.Settings.Description))
                {
                    return "Spaces Provider";
                }
                else
                {
                    return SpacesConfiguration.Settings.Description;
                }
            }
        }

        public override string Name
        {
            get
            {
                if (String.IsNullOrEmpty(SpacesConfiguration.Settings.Name))
                {
                    return "Spaces Provider";
                }
                else
                {
                    return SpacesConfiguration.Settings.Name;
                }
            }
        }

        #region Space management

        /// <summary>
        /// When implemented in a derived class, creates a space.
        /// </summary>
        /// <param name="Name">The name of the space to be created.  Will be unique within the application name.</param>
        /// <param name="ApplicationName">Name of the application.</param>
        /// <returns>
        /// A unique id of the space just created
        /// </returns>
        public abstract Space CreateSpace(string Name);

        /// <summary>
        /// When implemented in a derived class, checks whether the name of the Space exists within the applicationName.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <param name="ApplicationName">Name of the application.</param>
        /// <returns></returns>
        public abstract bool NameExists(string Name);

        public abstract Space GetSpace(string Name);

        public abstract bool SaveSpace(Space Space);

        public abstract bool DeleteSpace(string Name);

        public abstract void ResetApiSecret(string Name);
        #endregion

        #region Group management

        public abstract bool GroupExists(string groupName);

        public abstract void CreateGroup(string groupName);

        public abstract string[] GetUsersInGroup(string groupName);

        public abstract string[] GetSubgroupsInGroup(string groupName);

        public abstract void GetMembersInGroup(string groupName, out string[] usernames, out string[] subgroups);
        public bool IsUserInGroup(string groupName, string username)
        {
            return IsUserInGroup(groupName, username, false);
        }
        public abstract bool IsUserInGroup(string groupName, string username, bool multipleLevels);

        public abstract bool DeleteGroup(string groupName, bool throwOnPopulatedRole);

        protected abstract bool CanBecomeSubgroup(string ParentGroup, string SubGroup);

        public abstract string[] GetParentGroups(string groupName);

        public abstract void AddMembersToGroup(string groupName, string[] usernames, string[] subgroups);
        
        public void AddUsernamesToGroup(string groupName, string[] usernames)
        {
            AddMembersToGroup(groupName, usernames, new string[] { });
        }
        public void AddSubgroupsToGroup(string groupName, string[] subgroups)
        {
            AddMembersToGroup(groupName, new string[] { }, subgroups);
        }

        public abstract void GetSecurityTokensFromGroupsForUser(string userName);

        #endregion
    }

    public class Space
    {
        public static string RECORD_TYPE = "Space";
        public string ApplicationName { get; set; }
        public Guid Id { get; set; }
        public string SpaceName { get; set; }
        public string ApiSecret { get; set; }
        public List<string> AssignableTokens { get; set; }
        public string RecordType
        {
            get
            {
                return RECORD_TYPE;
            }
            set
            {
            }
        }
    }

    public class Group
    {
        public static string RECORD_TYPE = "Group";
        public string ApplicationName { get; set; }
        public Guid Id { get; set; }
        public string GroupName { get; set; }
        public string RecordType
        {
            get
            {
                return RECORD_TYPE;
            }
            set
            {
            }
        }
        [ContextualSecurity]
        public List<string> SecurityTokens { get; set; }
        public List<string> AssignableTokens { get; set; }
    }

    public class GroupHierarchy
    {
        public static string RECORD_TYPE = "GroupHierarchy";
        public string ApplicationName { get; set; }
        public Guid Id { get; set; }
        public string ParentGroup { get; set; }
     
        private List<string> subgroups = new List<string> ();
        public List<string> Subgroups
        {
            get { return subgroups; }
            set { subgroups = value; }
        }

        private List<string> usernames = new List<string>();
        public List<string> Usernames
        {
            get { return usernames; }
            set { usernames = value; }
        }
        
        public string RecordType
        {
            get
            {
                return RECORD_TYPE;
            }
            set
            {
            }
        }
    }


    public class SpaceProviderException : ApplicationException
    {
        public SpaceProviderException()
            : base()
        {
        }
        public SpaceProviderException(string message)
            : base(message)
        {
        }

        public SpaceProviderException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
        public SpaceProviderException(string message, System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }

    public class SpacesConfiguration : ConfigurationSection
    {
        private static SpacesConfiguration settings = ConfigurationManager.GetSection("spaces") as SpacesConfiguration;

        public static SpacesConfiguration Settings
        {
            get
            {
                return settings;
            }
        }

        [ConfigurationProperty("connectionString", IsRequired = true)]
        public string ConnectionString
        {
            get { return (string)this["connectionString"]; }
            set { this["connectionString"] = value; }
        }
        [ConfigurationProperty("databaseName", IsRequired = true)]
        public string DatabaseName
        {
            get { return (string)this["databaseName"]; }
            set { this["databaseName"] = value; }
        }
        [ConfigurationProperty("collectionName", IsRequired = true)]
        public string CollectionName
        {
            get { return (string)this["collectionName"]; }
            set { this["collectionName"] = value; }
        }

        [ConfigurationProperty("applicationName", IsRequired = true)]
        //[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;’\"|\\", MinLength = 1, MaxLength = 256)]
        public string ApplicationName
        {
            get { return (string)this["applicationName"]; }
            set { this["applicationName"] = value; }
        }

        [ConfigurationProperty("name", IsRequired = false)]
        //[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;’\"|\\", MinLength = 1, MaxLength = 256)]
        public string Name
        {
            get { return (string)this["name"]; }
            set { this["name"] = value; }
        }

        [ConfigurationProperty("description", IsRequired = false)]
        //[StringValidator(InvalidCharacters = "~!@#$%^&*()[]{}/;’\"|\\", MinLength = 1, MaxLength = 256)]
        public string Description
        {
            get { return (string)this["description"]; }
            set { this["description"] = value; }
        }
    }
}
