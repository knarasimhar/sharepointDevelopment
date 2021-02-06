using ADUVerify.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using System.Data;


namespace ADUVerify.Controllers
{
    [EnableCors(origins: "http://localhost:44359/", headers: "*", methods: "*")]
    public class AduVerifyController : ApiController
    {


        String strSiteURL = "http://sharepoint2/sites/teamsiteex/PipFlowSite", strUSER = "spuser2", strPWD = "User@123#", strResponse = "";
        string domainName = "saathispdt.com";
        // Full distinguished name of OU to create user in. E.g. OU=Users,OU=Perth,DC=Contoso,DC=com
        string userOU = "OU=National,OU=PIP,DC=saathispdt,DC=com";
        string userOU_stats = "OU=State,OU=PIP,DC=saathispdt,DC=com";
        string userOU_root = "OU=PIP,DC=saathispdt,DC=com";

        List<SelectListItem> ObjList;

        List<SelectListItem> ObjSubList;

        List<SelectListItem> ObjGroups;

        List<SelectListItem> ObjUsers;

        public AduVerifyController()
        {
            loadDefaultValues();
            if (ConfigurationManager.AppSettings["AD_domainName"] != null)
                domainName = ConfigurationManager.AppSettings["AD_domainName"].ToString();
            if (ConfigurationManager.AppSettings["AD_userOU"] != null)
                userOU = ConfigurationManager.AppSettings["AD_userOU"].ToString();
            if (ConfigurationManager.AppSettings["AD_userOU_root"] != null)
                userOU_root = ConfigurationManager.AppSettings["AD_userOU_root"].ToString();

            if (ConfigurationManager.AppSettings["SITE_URL"] != null)
                strSiteURL = ConfigurationManager.AppSettings["SITE_URL"].ToString();
            if (ConfigurationManager.AppSettings["SITE_URL_USER"] != null)
                strUSER = ConfigurationManager.AppSettings["SITE_URL_USER"].ToString();
            if (ConfigurationManager.AppSettings["SITE_URL_PWD"] != null)
                strPWD = ConfigurationManager.AppSettings["SITE_URL_PWD"].ToString();

            //strPWD = HttpUtility.UrlEncode(strPWD);
        }

        [System.Web.Http.Route("api/AduVerify/getADUsers")]

        [System.Web.Http.HttpGet, System.Web.Http.HttpPost]

        public HttpResponseMessage getADUsers(string OUNAMES)
        {
            List<CreateUser> userlist = new List<CreateUser>();
            foreach (string OU in OUNAMES.Split(','))
            {
                try
                {
                    // OU =  userOU.Replace("National", OU);
                    using (var context = new PrincipalContext(ContextType.Domain, domainName, userOU.Replace("National", OU)))
                    {
                        using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                        {

                            //CreateUser
                            foreach (var result in searcher.FindAll())
                            {
                                DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                                // ViewBag.Message += "</br>First Name: " + de.Properties["givenName"].Value;
                                //   ViewBag.Message += "Last Name : " + de.Properties["sn"].Value;

                                //  ViewBag.Message += "User principal name: " + de.Properties["userPrincipalName"].Value;
                                CreateUser obj = new CreateUser();

                                if (de.Properties["sn"].Value != null)
                                    obj.LastName = de.Properties["sn"].Value.ToString();
                                //obj.fu    de.Properties["userPrincipalName"].Value
                                obj.OU = OU.ToUpper();
                                obj.UserName = de.Properties["samAccountName"].Value.ToString();
                                if (de.Properties["mail"].Value != null)
                                    obj.Emailid = de.Properties["mail"].Value.ToString();
                                if (de.Properties["telephoneNumber"].Value != null)
                                    obj.Mobileno = de.Properties["telephoneNumber"].Value.ToString();
                                if (de.Properties["department"].Value != null)
                                    obj.Department = de.Properties["department"].Value.ToString();
                                obj.FirstName = de.Properties["displayName"].Value.ToString();
                                if (de.Properties["st"].Value != null)
                                    obj.State = de.Properties["st"].Value.ToString();
                                userlist.Add(obj);
                            }

                        }
                    }
                }
                catch (Exception ex)
                {
                    return getErrormessage(ex.Message);
                }
            }
            return getHttpResponseMessage(JsonConvert.SerializeObject(userlist));
        }
        // tsfsdf sdkfjldsf 
        [System.Web.Http.Route("api/AduVerify/ADAddUser")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage ADAddUser(CreateUser model)
        {

            // verfiy user name before creation
            DirectoryEntry myLdapConnection = createDirectoryEntry();
            DirectorySearcher search = new DirectorySearcher(myLdapConnection);
            search.Filter = "(sAMAccountName=" + model.UserName + ")";

            // search.PropertiesToLoad.Add("mail");
            search.PropertiesToLoad.Add("telephoneNumber");

            SearchResult result = search.FindOne();
            if (result != null)

            {

                return getHttpResponseMessage(JsonConvert.SerializeObject("already exists"));
           }
            // Source: http://stackoverflow.com/a/2305871
            using (var pc = new PrincipalContext(ContextType.Domain, domainName, userOU.Replace("National", model.OU)))
            {
                using (var up = new UserPrincipal(pc))
                {
                    // Create username and display name from firstname and lastname
                    var userName = model.FirstName + "." + model.LastName;
                    var displayName = model.FirstName + " " + model.LastName;
                    // In a real scenario a randomised password would be preferred
                    var password = model.Password;

                    // Set the values for new user account

                    up.Name = displayName;
                    up.DisplayName = displayName;
                    up.GivenName = model.FirstName;
                    up.Surname = model.LastName;
                    up.SamAccountName = model.UserName;
                   // up.EmailAddress = model.Emailid;
                    up.UserPrincipalName = model.UserName;
                    up.VoiceTelephoneNumber = model.Mobileno;

                    up.SetPassword(password);
                    up.Enabled = true;
                    up.PasswordNeverExpires = true;

                    try
                    {
                        // Attempt to save the account to AD
                        up.Save();



                        // apped groups
                        if (model.groups != null)
                            foreach (var _group in model.groups)
                            {
                                if (_group != null)
                                    AddUserToGroup(model.UserName, _group);
                            }
                    }
                    catch (Exception ex)
                    {
                        return getErrormessage(ex.Message);

                    }

                    // Add the department to the newly created AD user
                    // Get the directory entry object for the user
                    DirectoryEntry de = up.GetUnderlyingObject() as DirectoryEntry;
                    // Set the department property to the value entered by the user


                    de.Properties["department"].Value = model.Department;
                    if (model.ReportingManager != null)
                        de.Properties["manager"].Value = "CN=" + model.ReportingManager + "," + userOU;
                    //int val = (int)de.Properties["userAccountControl"].Value;
                    // de.Properties["userAccountControl"].Value = val & ~0x2;
                    //de.Invoke("SetPassword", new object[] { model.Password });
                    try
                    {
                        // Try to commit changes
                        de.CommitChanges();
                    }
                    catch (Exception ex)
                    {

                        return getErrormessage(ex.Message);

                    }
                }
            }

            // Redirect to completed page if successful
            return getHttpResponseMessage(JsonConvert.SerializeObject("success"));

        }

        private DirectoryEntry createDirectoryEntry()
        {
            // create and return new LDAP connection with desired settings

            DirectoryEntry ldapConnection = new DirectoryEntry(domainName);

            ldapConnection.Path = "LDAP://" + userOU_root;

            ldapConnection.AuthenticationType = AuthenticationTypes.Secure;

            return ldapConnection;

        }
        [System.Web.Http.Route("api/AduVerify/ADUpdateUser")]
        [System.Web.Http.HttpGet, System.Web.Http.HttpPost]
        public HttpResponseMessage ADUpdateUser(CreateUser model)
        {
            try
            {

                DirectoryEntry myLdapConnection = createDirectoryEntry();
                DirectorySearcher search = new DirectorySearcher(myLdapConnection);
                search.Filter = "(sAMAccountName="  + model.UserName + ")";

               // search.PropertiesToLoad.Add("mail");
                search.PropertiesToLoad.Add("telephoneNumber");
                
                SearchResult result = search.FindOne();
                if (result != null)

                {

                    // create new object from search result
                    DirectoryEntry entryToUpdate = result.GetDirectoryEntry();
                    /* show existing title
                    if (model.Emailid != null)
                        entryToUpdate.Properties["mail"].Value = model.Emailid;*/
                    if (model.Mobileno != null)
                        entryToUpdate.Properties["telephoneNumber"].Value = model.Mobileno;


                    entryToUpdate.CommitChanges();



                }
                else
                    return getSuccessmessage("user not found");



            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }

            return getSuccessmessage("success");

        }
      
        public void AddUserToGroup(string userId, string groupName)
        {
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName))
                {
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(pc, groupName);
                    group.Members.Add(pc, IdentityType.UserPrincipalName, userId);
                    group.Save();
                }
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //doSomething with E.Message.ToString(); 

            }
        }

        public void loadDefaultValues()
        {
            //Creating generic list
            ObjList = new List<SelectListItem>()
            {
               
                 new SelectListItem { Text = "National", Value = userOU },
                new SelectListItem { Text = "States", Value = userOU.Replace("National","State")}

            };

            //Creating generic list
            ObjSubList = new List<SelectListItem>()
            {
                new SelectListItem { Text = "HR", Value = "HR" },
                new SelectListItem { Text = "Finance", Value = "2" },
                new SelectListItem { Text = "IT", Value = "3" },
                new SelectListItem { Text = "Operations", Value = "4" },

            };

            //Creating generic list
            //  getUsers();
            //   getGroups();
            //Creating generic list
            //Assigning generic list to ViewBag

        }
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }
        private HttpResponseMessage getErrormessage(string strEmsg)
        {
            return Request.CreateErrorResponse(HttpStatusCode.BadRequest,
                                      strEmsg);
        }

        private HttpResponseMessage getSuccessmessage(string strEmsg)
        {
            return Request.CreateErrorResponse(HttpStatusCode.OK,
                                      strEmsg);
        }
        private HttpResponseMessage getHttpResponseMessage(string Resp)
        {

            return new HttpResponseMessage { Content = new StringContent(Resp, System.Text.Encoding.UTF8, "application/json") };

        }
        [System.Web.Http.HttpGet]
        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}