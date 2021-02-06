﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Diagnostics;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;

namespace ADUVerify.Models
{
    public sealed class AdRoleModel : RoleProvider
    {
        private const string AD_FILTER = "(&(objectCategory=group)(|(groupType=-2147483646)(groupType=-2147483644)(groupType=-2147483640)))";
        private const string AD_FIELD = "samAccountName";

        private string _activeDirectoryConnectionString;
        private string _domain;

        // Retrieve Group Mode
        // "Additive" indicates that only the groups specified in groupsToUse will be used
        // "Subtractive" indicates that all Active Directory groups will be used except those specified in groupsToIgnore
        // "Additive" is somewhat more secure, but requires more maintenance when groups change
        private bool _isAdditiveGroupMode;

        private List<string> _groupsToUse;
        private List<string> _groupsToIgnore;
        private List<string> _usersToIgnore;

        #region Ignore Lists

        // IMPORTANT - DEFAULT LIST OF ACTIVE DIRECTORY USERS TO "IGNORE"
        //             DO NOT REMOVE ANY OF THESE UNLESS YOU FULLY UNDERSTAND THE SECURITY IMPLICATIONS
        //             VERYIFY THAT ALL CRITICAL USERS ARE IGNORED DURING TESTING
        private String[] _DefaultUsersToIgnore = new String[]
        {
            "Administrator", "TsInternetUser", "Guest", "krbtgt", "Replicate", "SERVICE", "SMSService"
        };

        // IMPORTANT - DEFAULT LIST OF ACTIVE DIRECTORY DOMAIN GROUPS TO "IGNORE"
        //             PREVENTS ENUMERATION OF CRITICAL DOMAIN GROUP MEMBERSHIP
        //             DO NOT REMOVE ANY OF THESE UNLESS YOU FULLY UNDERSTAND THE SECURITY IMPLICATIONS
        //             VERIFY THAT ALL CRITICAL GROUPS ARE IGNORED DURING TESTING BY CALLING GetAllRoles MANUALLY
        private String[] _defaultGroupsToIgnore = new String[]
            {
                "Domain Guests", "Domain Computers", "Group Policy Creator Owners", "Guests", "Users",
                "Domain Users", "Pre-Windows 2000 Compatible Access", "Exchange Domain Servers", "Schema Admins",
                "Enterprise Admins", "Domain Admins", "Cert Publishers", "Backup Operators", "Account Operators",
                "Server Operators", "Print Operators", "Replicator", "Domain Controllers", "WINS Users",
                "DnsAdmins", "DnsUpdateProxy", "DHCP Users", "DHCP Administrators", "Exchange Services",
                "Exchange Enterprise Servers", "Remote Desktop Users", "Network Configuration Operators",
                "Incoming Forest Trust Builders", "Performance Monitor Users", "Performance Log Users",
                "Windows Authorization Access Group", "Terminal Server License Servers", "Distributed COM Users",
                "Administrators", "Everybody", "RAS and IAS Servers", "MTS Trusted Impersonators",
                "MTS Impersonators", "Everyone", "LOCAL", "Authenticated Users"
            };
        #endregion

        /// <summary>
        /// Initializes a new instance of the ADRoleProvider class.
        /// </summary>
        public AdRoleModel()
        {
            _groupsToUse = new List<string>();
            _groupsToIgnore = new List<string>();
            _usersToIgnore = new List<string>();
        }

        public override String ApplicationName { get; set; }

        /// <summary>
        /// Initialize ADRoleProvider with config values
        /// </summary>
        /// <param name="name"></param>
        /// <param name="config"></param>
        public override void Initialize(String name, NameValueCollection config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (String.IsNullOrEmpty(name))
                name = "ADRoleProvider";

            if (String.IsNullOrEmpty(config["description"]))
            {
                config.Remove("description");
                config.Add("description", "Active Directory Role Provider");
            }

            // Initialize the abstract base class.
            base.Initialize(name, config);

            _domain = ReadConfig(config, "domain");
            _isAdditiveGroupMode = (ReadConfig(config, "groupMode") == "Additive");
            _activeDirectoryConnectionString = ReadConfig(config, "connectionString");

            DetermineApplicationName(config);
            PopulateLists(config);
        }

        private string ReadConfig(NameValueCollection config, string key)
        {
            if (config.AllKeys.Any(k => k == key))
                return config[key];

            throw new ProviderException("Configuration value required for key: " + key);
        }

        private void DetermineApplicationName(NameValueCollection config)
        {
            // Retrieve Application Name
            ApplicationName = config["applicationName"];
            if (String.IsNullOrEmpty(ApplicationName))
            {
                try
                {
                    string app =
                        HostingEnvironment.ApplicationVirtualPath ??
                        Process.GetCurrentProcess().MainModule.ModuleName.Split('.').FirstOrDefault();

                    ApplicationName = app != "" ? app : "/";
                }
                catch
                {
                    ApplicationName = "/";
                }
            }

            if (ApplicationName.Length > 256)
                throw new ProviderException("The application name is too long.");
        }

        private void PopulateLists(NameValueCollection config)
        {
            // If Additive group mode, populate GroupsToUse with specified AD groups
            if (_isAdditiveGroupMode && !String.IsNullOrEmpty(config["groupsToUse"]))
                _groupsToUse.AddRange(
                    config["groupsToUse"].Split(',').Select(group => group.Trim())
                );

            // Populate GroupsToIgnore List<string> with AD groups that should be ignored for roles purposes
            _groupsToIgnore.AddRange(
                _defaultGroupsToIgnore.Select(group => group.Trim())
            );

            _groupsToIgnore.AddRange(
                (config["groupsToIgnore"] ?? "").Split(',').Select(group => group.Trim())
            );

            // Populate UsersToIgnore ArrayList with AD users that should be ignored for roles purposes
            string usersToIgnore = config["usersToIgnore"] ?? "";
            _usersToIgnore.AddRange(
                _DefaultUsersToIgnore
                    .Select(value => value.Trim())
                    .Union(
                        usersToIgnore
                            .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(value => value.Trim())
                    )
            );
        }

        private void RecurseGroup(PrincipalContext context, string group, List<string> groups)
        {
            var principal = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, group);

            if (principal == null)
                return;

            List<string> res =
                principal
                    .GetGroups()
                    .ToList()
                    .Select(grp => grp.Name)
                    .ToList();

            groups.AddRange(res.Except(groups));
            foreach (var item in res)
                RecurseGroup(context, item, groups);
        }

        /// <summary>
        /// Retrieve listing of all roles to which a specified user belongs.
        /// </summary>
        /// <param name="username"></param>
        /// <returns>String array of roles</returns>
        public override string[] GetRolesForUser(string username)
        {
            string sessionKey = "groupsForUser:" + username;

            if (HttpContext.Current != null &&
                 HttpContext.Current.Session != null &&
                 HttpContext.Current.Session[sessionKey] != null
            )
                return ((List<string>)(HttpContext.Current.Session[sessionKey])).ToArray();

            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _domain))
            {
                try
                {
                    // add the users groups to the result
                    var groupList =
                        UserPrincipal
                            .FindByIdentity(context, IdentityType.SamAccountName, username)
                            .GetGroups()
                            .Select(group => group.Name)
                            .ToList();

                    // add each groups sub groups into the groupList
                    foreach (var group in new List<string>(groupList))
                        RecurseGroup(context, group, groupList);

                    groupList = groupList.Except(_groupsToIgnore).ToList();

                    if (_isAdditiveGroupMode)
                        groupList = groupList.Join(_groupsToUse, r => r, g => g, (r, g) => r).ToList();

                    if (HttpContext.Current != null)
                        HttpContext.Current.Session[sessionKey] = groupList;

                    return groupList.ToArray();
                }
                catch (Exception ex)
                {
                    // TODO: LogError( "Unable to query Active Directory.", ex );
                    return new[] { "" };
                }
            }
        }

        /// <summary>
        /// Retrieve listing of all users in a specified role.
        /// </summary>
        /// <param name="rolename">String array of users</param>
        /// <returns></returns>
        public override string[] GetUsersInRole(String rolename)
        {
            if (!RoleExists(rolename))
                throw new ProviderException(String.Format("The role '{0}' was not found.", rolename));

            using (PrincipalContext context = new PrincipalContext(ContextType.Domain, _domain))
            {
                try
                {
                    GroupPrincipal p = GroupPrincipal.FindByIdentity(context, IdentityType.SamAccountName, rolename);

                    return (

                        from user in p.GetMembers(true)
                        where !_usersToIgnore.Contains(user.SamAccountName)
                        select user.SamAccountName

                    ).ToArray();
                }
                catch (Exception ex)
                {
                    // TODO: LogError( "Unable to query Active Directory.", ex );
                    return new[] { "" };
                }
            }
        }

        /// <summary>
        /// Determine if a specified user is in a specified role.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="rolename"></param>
        /// <returns>Boolean indicating membership</returns>
        public override bool IsUserInRole(string username, string rolename)
        {
            return GetUsersInRole(rolename).Any(user => user == username);
        }

        /// <summary>
        /// Retrieve listing of all roles.
        /// </summary>
        /// <returns>String array of roles</returns>
        public override string[] GetAllRoles()
        {
            string[] roles = ADSearch(_activeDirectoryConnectionString, AD_FILTER, AD_FIELD);

            return (

                from role in roles.Except(_groupsToIgnore)
                where !_isAdditiveGroupMode || _groupsToUse.Contains(role)
                select role

            ).ToArray();
        }

        /// <summary>
        /// Determine if given role exists
        /// </summary>
        /// <param name="rolename">Role to check</param>
        /// <returns>Boolean indicating existence of role</returns>
        public override bool RoleExists(string rolename)
        {
            return GetAllRoles().Any(role => role == rolename);
        }

        /// <summary>
        /// Return sorted list of usernames like usernameToMatch in rolename
        /// </summary>
        /// <param name="rolename">Role to check</param>
        /// <param name="usernameToMatch">Partial username to check</param>
        /// <returns></returns>
        public override string[] FindUsersInRole(string rolename, string usernameToMatch)
        {
            if (!RoleExists(rolename))
                throw new ProviderException(String.Format("The role '{0}' was not found.", rolename));

            return (
                from user in GetUsersInRole(rolename)
                where user.ToLower().Contains(usernameToMatch.ToLower())
                select user

            ).ToArray();
        }

        #region Non Supported Base Class Functions

        /// <summary>
        /// AddUsersToRoles not supported.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory. 
        /// </summary>
        public override void AddUsersToRoles(string[] usernames, string[] rolenames)
        {
            throw new NotSupportedException("Unable to add users to roles.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory.");
        }

        /// <summary>
        /// CreateRole not supported.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory. 
        /// </summary>
        public override void CreateRole(string rolename)
        {
            throw new NotSupportedException("Unable to create new role.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory.");
        }

        /// <summary>
        /// DeleteRole not supported.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory. 
        /// </summary>
        public override bool DeleteRole(string rolename, bool throwOnPopulatedRole)
        {
            throw new NotSupportedException("Unable to delete role.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory.");
        }

        /// <summary>
        /// RemoveUsersFromRoles not supported.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory. 
        /// </summary>
        public override void RemoveUsersFromRoles(string[] usernames, string[] rolenames)
        {
            throw new NotSupportedException("Unable to remove users from roles.  For security and management purposes, ADRoleProvider only supports read operations against Active Direcory.");
        }
        #endregion

        /// <summary>
        /// Performs an extremely constrained query against Active Directory.  Requests only a single value from
        /// AD based upon the filtering parameter to minimize performance hit from large queries.
        /// </summary>
        /// <param name="ConnectionString">Active Directory Connection String</param>
        /// <param name="filter">LDAP format search filter</param>
        /// <param name="field">AD field to return</param>
        /// <returns>String array containing values specified by 'field' parameter</returns>
        private String[] ADSearch(String ConnectionString, String filter, String field)
        {
            DirectorySearcher searcher = new DirectorySearcher
            {
                SearchRoot = new DirectoryEntry(ConnectionString),
                Filter = filter,
                PageSize = 500
            };
            searcher.PropertiesToLoad.Clear();
            searcher.PropertiesToLoad.Add(field);

            try
            {
                using (SearchResultCollection results = searcher.FindAll())
                {
                    List<string> r = new List<string>();
                    foreach (SearchResult searchResult in results)
                    {
                        var prop = searchResult.Properties[field];
                        for (int index = 0; index < prop.Count; index++)
                            r.Add(prop[index].ToString());
                    }

                    return r.Count > 0 ? r.ToArray() : new string[0];
                }
            }
            catch (Exception ex)
            {
                throw new ProviderException("Unable to query Active Directory.", ex);
            }
        }
    }
}