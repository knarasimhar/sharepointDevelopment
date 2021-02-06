using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.DirectoryServices.AccountManagement;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;
using ADUVerify.Models;
using System.DirectoryServices.ActiveDirectory;
using System.Configuration;

namespace ADUVerify.Controllers
{
    public class HomeController : Controller
    {
        // NetBIOS name of domain. E.g. CONTOSO
        string domainName = "saathispdt.com";
        // Full distinguished name of OU to create user in. E.g. OU=Users,OU=Perth,DC=Contoso,DC=com
        string userOU = "OU=National,OU=PIP,DC=saathispdt,DC=com";
         string userOU_root = "OU=PIP,DC=saathispdt,DC=com";

        List<SelectListItem> ObjList;

        List<SelectListItem> ObjSubList;

        List<SelectListItem> ObjGroups;

        List<SelectListItem> ObjUsers;
        public ActionResult Index()
        {
            if (ConfigurationManager.AppSettings["AD_domainName"] != null)
                domainName = ConfigurationManager.AppSettings["AD_domainName"].ToString();
            if (ConfigurationManager.AppSettings["AD_userOU"] != null)
                userOU = ConfigurationManager.AppSettings["AD_userOU"].ToString();
            if (ConfigurationManager.AppSettings["AD_userOU_root"] != null)
                userOU_root = ConfigurationManager.AppSettings["AD_userOU_root"].ToString();
            loadDefaultValues();

            return View();
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
            getUsers();
            getGroups();
            //Creating generic list
            //Assigning generic list to ViewBag
            ViewBag.Locations = new SelectList(ObjList, "Value", "Text");
            ViewBag.SubLocations = new SelectList(ObjSubList, "Value", "Text");
            ViewBag.users = new SelectList(ObjUsers, "Value", "Text");
            ViewBag.groups = new SelectList(ObjGroups, "Value", "Text");
        }
        public ActionResult Completed()
        {
            return View();
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

        public void RemoveUserFromGroup(string userId, string groupName)
        {
            try
            {
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName))
                {
                    GroupPrincipal group = GroupPrincipal.FindByIdentity(pc, groupName);
                    group.Members.Remove(pc, IdentityType.UserPrincipalName, userId);
                    group.Save();
                }
            }
            catch (System.DirectoryServices.DirectoryServicesCOMException E)
            {
                //doSomething with E.Message.ToString(); 

            }
        }

        [HttpPost]
        public ActionResult Index(CreateUser model)
        {
           

            // Source: http://stackoverflow.com/a/2305871
            using (var pc = new PrincipalContext(ContextType.Domain, domainName, model.OU))
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
                    up.EmailAddress = model.Emailid;
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
                        if(model.groups!=null)
                        foreach (var _group in model.groups)
                        {
                            if(_group!=null)
                            AddUserToGroup(model.UserName, _group);
                        }
                    }
                    catch (Exception e)
                    {
                        // Display exception(s) within validation summary
                        loadDefaultValues();
                        ModelState.AddModelError("", "Exception creating user object. " + e);
                        return View(model);
                    }

                    // Add the department to the newly created AD user
                    // Get the directory entry object for the user
                    DirectoryEntry de = up.GetUnderlyingObject() as DirectoryEntry;
                    // Set the department property to the value entered by the user
                   
                     
                    de.Properties["department"].Value = model.Department;
                    if (model.ReportingManager != null)
                       de.Properties["manager"].Value = "CN=" + model.ReportingManager + ","+ userOU;
                    //int val = (int)de.Properties["userAccountControl"].Value;
                   // de.Properties["userAccountControl"].Value = val & ~0x2;
                    //de.Invoke("SetPassword", new object[] { model.Password });
                    try
                    {
                        // Try to commit changes
                        de.CommitChanges();
                    }
                    catch (Exception e)
                    {
                        // Display exception(s) within validation summary
                        loadDefaultValues();
                        ModelState.AddModelError("", "Exception adding manager. " + e);
                        return View(model);
                    }
                }
            }

            // Redirect to completed page if successful
            return RedirectToAction("Completed");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult CreateADUser(string OU,string DC,string _DC,string Uname, string Pwd)
        {


            if (Uname == null)
            {
                ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/CreateADUser?ou=&dc=&_dc&uname=xxxx&pwd=xxxx";
                return View();
            }

           

            DirectoryEntry ouEntry = new DirectoryEntry(userOU);

            
                try
                {
                    DirectoryEntry childEntry = ouEntry.Children.Add("CN=" + Uname, "user");
                
                    childEntry.CommitChanges();
                    ouEntry.CommitChanges();
                    childEntry.Invoke("SetPassword", new object[] { Pwd });
                    childEntry.Properties["samAccountName"].Value = Uname;
                
                  
                childEntry.CommitChanges();
                int val = (int)childEntry.Properties["userAccountControl"].Value;
                childEntry.Properties["userAccountControl"].Value = val & ~0x2;
                childEntry.CommitChanges();
                ViewBag.Message = "your new Active Directory user added successfully.";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "Exception ." + ex.Message;
                }

           
            return View();
        }
        public ActionResult ValidateADUser()
        {

           
                return View();
           

        }
        [HttpPost]

        public ActionResult ValidateADUser(CreateUser model)
        {

            if (model.UserName == null)
            {
                ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/ValidateADUser?uname=xxxx&pwd=xxxx";
                return View();
            }
            bool isValid = false;
            String Uname = model.UserName;
            String Pwd = model.Password;
            
                using (PrincipalContext pc = new PrincipalContext(ContextType.Domain, domainName))
                {
                    // validate the credentials
                    isValid = pc.ValidateCredentials(Uname, Pwd);
                }
           
            ViewBag.Message = "Your User validation is :" + isValid.ToString();
            return View();
        }

        private void getUsers()
        {

            ObjUsers = new List<SelectListItem>();
            
         using (var context = new PrincipalContext(ContextType.Domain, domainName, userOU))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                   
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
    // ViewBag.Message += "</br>First Name: " + de.Properties["givenName"].Value;
    //   ViewBag.Message += "Last Name : " + de.Properties["sn"].Value;
   
                        ObjUsers.Add(new SelectListItem { Text = de.Properties["samAccountName"].Value.ToString(), Value = de.Properties["samAccountName"].Value.ToString() });
                        //  ViewBag.Message += "User principal name: " + de.Properties["userPrincipalName"].Value;

                    }

                }
            }
        }
        
        private void getGroups()
        
            {
          //  PrincipalContext principalContext =  new PrincipalContext(ContextType.Domain, "sharepoint1:636");

          //  bool userValid = principalContext.ValidateCredentials("pruser1", "650005@Dapl^hyD#");

             
                        ObjGroups = new List<SelectListItem>();

            PrincipalContext yourOU = new PrincipalContext(ContextType.Domain, domainName, userOU_root);
            GroupPrincipal findAllGroups = new GroupPrincipal(yourOU, "*");
            PrincipalSearcher ps = new PrincipalSearcher(findAllGroups);
            foreach (var group in ps.FindAll())
            {
               
                ObjGroups.Add(new SelectListItem { Text = group.Name.ToString(), Value = group.Name.ToString() });
            }
            
        }
        private void ListADUsersViews()
        { 
         //Creating generic list
            ObjList = new List<SelectListItem>()
            {
                new SelectListItem { Text = "National", Value = userOU },
                new SelectListItem { Text = "States", Value = userOU.Replace("National","State")}

            };
            ViewBag.Locations = new SelectList(ObjList, "Value", "Text");
        }
        public ActionResult ListADUsers()
        {
            ListADUsersViews();
            return View();
        }
        [HttpPost]
        public ActionResult ListADUsers(CreateUser model)
        {

            ListADUsersViews();
            using (var context = new PrincipalContext(ContextType.Domain,domainName,model.OU))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    ViewBag.Message = "<table><tr><th>User Name</th></tr>";
                    List<CreateUser> userlist = new List<CreateUser>();
                    //CreateUser
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        // ViewBag.Message += "</br>First Name: " + de.Properties["givenName"].Value;
                        //   ViewBag.Message += "Last Name : " + de.Properties["sn"].Value;

                        ViewBag.Message += "<tr><td>" + de.Properties["samAccountName"].Value + "</tr></td>";
                        //  ViewBag.Message += "User principal name: " + de.Properties["userPrincipalName"].Value;
                        CreateUser obj = new CreateUser();
                        obj.UserName = de.Properties["samAccountName"].Value.ToString();
                        if(de.Properties["mail"].Value != null)
                        obj.Emailid = de.Properties["mail"].Value.ToString();
                        if (de.Properties["telephoneNumber"].Value != null)
                            obj.Mobileno = de.Properties["telephoneNumber"].Value.ToString();
                        if (de.Properties["department"].Value != null)
                            obj.Department = de.Properties["department"].Value.ToString();
                        obj.FirstName = de.Properties["displayName"].Value.ToString();
                        userlist.Add(obj);
                    }
                    ViewBag.Message += "</table>";
                    ViewBag.userlist = userlist;
                }
            }
            return View();
        }

        public ActionResult ListADGroups()
        {
            string Uname = "user.start";
            if (Uname == null)
            {
               // ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/getADGroupsByUser?uname=xxxx";
                return View();
            }

            try
            {
                ViewBag.Message = GetAdGroupsForUser22(Uname, domainName);

                if (ViewBag.Message.Count == 0)
                {
                    ViewBag.Message = "No group mapped";
                }
            }
            catch { ViewBag.Message = "user not found"; }

           
            /*
            using (var context = new PrincipalContext(ContextType.Domain, "mylab.local"))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    ViewBag.Message = "<table><tr><th>User Name</th></tr>";
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        // ViewBag.Message += "</br>First Name: " + de.Properties["givenName"].Value;
                        //   ViewBag.Message += "Last Name : " + de.Properties["sn"].Value;
                        ViewBag.Message += "<tr><td>" + de.Properties["samAccountName"].Value + "</tr></td>";
                        //  ViewBag.Message += "User principal name: " + de.Properties["userPrincipalName"].Value;

                    }
                    ViewBag.Message += "</table>";
                }
            }*/
            return View();
        }

        public ActionResult CreateADGroups(string _Groupname)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domainName, userOU))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    ViewBag.Message = "<table><tr><th>User Name</th></tr>";
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        // ViewBag.Message += "</br>First Name: " + de.Properties["givenName"].Value;
                        //   ViewBag.Message += "Last Name : " + de.Properties["sn"].Value;
                        ViewBag.Message += "<tr><td>" + de.Properties["samAccountName"].Value + "</tr></td>";
                        //  ViewBag.Message += "User principal name: " + de.Properties["userPrincipalName"].Value;

                    }
                    ViewBag.Message += "</table>";
                }
            }
            return View();
        }


        public ActionResult SetADGroupsUser(string OU, string CN, string Gname,string Gtype)
        {

            if (Gname == null)
            {
                ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/SetADGroupsUser?ou=&cn=&type=&gname=xxxx";
                return View();
            }

            ViewBag.message = Gname + " created successfully";


            DirectoryEntry ouEntry = new DirectoryEntry(userOU);


            try
            {
                DirectoryEntry childEntry = ouEntry.Children.Add("CN=" + Gname, "group");
                childEntry.CommitChanges();
                ouEntry.CommitChanges();
                childEntry.Properties["samAccountName"].Value = Gname;

                childEntry.CommitChanges();
                // Set the group type to a secured domain local group.
                /*ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_GLOBAL_GROUP | ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_SECURITY_ENABLED;

                if (Gtype == "global")
                    childEntry.Properties["groupType"].Value = ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_GLOBAL_GROUP;
                else
                    childEntry.Properties["groupType"].Value = "ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_DOMAIN_LOCAL_GROUP|ActiveDs.ADS_GROUP_TYPE_ENUM.ADS_GROUP_TYPE_SECURITY_ENABLED";


                childEntry.CommitChanges();
                int val = (int)childEntry.Properties["userAccountControl"].Value;
                childEntry.Properties["userAccountControl"].Value = val & ~0x2;
                childEntry.CommitChanges();*/
                ViewBag.Message = "your new Active Directory group added successfully.";
            }
            catch (Exception ex)
            {
                ViewBag.Message = "Exception ." + ex.Message;
            }


            return View();


           
          
        }

        public ActionResult getADGroupsByUser(string Uname)
        {

            if (Uname == null)
            {
                ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/getADGroupsByUser?uname=xxxx";
                return View();
            }

            try
            {
                ViewBag.Message = GetAdGroupsForUser2(Uname,domainName);
            }
            catch { ViewBag.Message = "user not found";  }

            if (ViewBag.Message.Count == 0)
            {
                ViewBag.Message = "No group mapped";
            }
            /*
            using (var context = new PrincipalContext(ContextType.Domain, "mylab.local"))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    ViewBag.Message = "<table><tr><th>User Name</th></tr>";
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        // ViewBag.Message += "</br>First Name: " + de.Properties["givenName"].Value;
                        //   ViewBag.Message += "Last Name : " + de.Properties["sn"].Value;
                        ViewBag.Message += "<tr><td>" + de.Properties["samAccountName"].Value + "</tr></td>";
                        //  ViewBag.Message += "User principal name: " + de.Properties["userPrincipalName"].Value;

                    }
                    ViewBag.Message += "</table>";
                }
            }*/
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

       
            
             // Usage: GetAdGroupsForUser2("domain\user") or GetAdGroupsForUser2("user","domain")
        public  List<string> GetAdGroupsForUser22(string userName, string domainName = null)
        {
            var result = new List<string>();

            if (userName.Contains('\\') || userName.Contains('/'))
            {
                domainName = userName.Split(new char[] { '\\', '/' })[0];
                userName = userName.Split(new char[] { '\\', '/' })[1];
            }

            using (PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domainName,userOU))
            using (UserPrincipal user = UserPrincipal.FindByIdentity(domainContext, userName))
            using (var searcher = new DirectorySearcher(new DirectoryEntry(userOU)))
            {
                searcher.Filter = String.Format("(&(objectCategory=group))", user.DistinguishedName);
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PropertiesToLoad.Add("cn");

                foreach (SearchResult entry in searcher.FindAll())
                    if (entry.Properties.Contains("cn"))
                        result.Add(entry.Properties["cn"][0].ToString());
            }

            return result;
        }
        
        // Usage: GetAdGroupsForUser2("domain\user") or GetAdGroupsForUser2("user","domain")
        public  List<string> GetAdGroupsForUser2(string userName, string domainName = null)
        {
            var result = new List<string>();

            if (userName.Contains('\\') || userName.Contains('/'))
            {
                domainName = userName.Split(new char[] { '\\', '/' })[0];
                userName = userName.Split(new char[] { '\\', '/' })[1];
            }

            using (PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domainName,userOU))
            using (UserPrincipal user = UserPrincipal.FindByIdentity(domainContext, userName))
            using (var searcher = new DirectorySearcher(new DirectoryEntry("LDAP://" + domainContext.Name)))
            {
                searcher.Filter = String.Format("(&(objectCategory=group)(member={0}))", user.DistinguishedName);
                searcher.SearchScope = SearchScope.Subtree;
                searcher.PropertiesToLoad.Add("cn");

                foreach (SearchResult entry in searcher.FindAll())
                    if (entry.Properties.Contains("cn"))
                        result.Add(entry.Properties["cn"][0].ToString());
            }

            return result;
        }

        public ActionResult getADPermissionsByUser(string Uname)
        {

            if (Uname == null)
            {
                //ViewBag.Message = @"Plz pass paramerters to this url like : [domainurl]/getADPermissionsByUser?uname=xxxx";
                return View();
            }

            try
            {
                ADUVerify.Models.AdRoleModel AdPobj = new ADUVerify.Models.AdRoleModel();

                ViewBag.Message = AdPobj.GetRolesForUser(Uname);
            }
            catch { ViewBag.Message = "user not found"; }

          
       
            return View();
        }

        public ActionResult GetAllRoles()
        {

            

            try
            {
                ADUVerify.Models.AdRoleModel AdPobj = new ADUVerify.Models.AdRoleModel();

                ViewBag.Message = AdPobj.GetAllRoles();
            }
            catch { ViewBag.Message = "not found"; }



            return View();
        }



              public ActionResult getADAccessRules()
        {
            DirectoryEntry entry = new DirectoryEntry(
            userOU,
            null,
            null,
            AuthenticationTypes.Secure
            );
           
           
                       //var sid = (SecurityIdentifier)user.ProviderUserKey;
            ActiveDirectorySecurity sec = entry.ObjectSecurity;

                //  PrintSD(sec);

                try
                {
                // respective user 
                PrincipalContext domainContext = new PrincipalContext(ContextType.Domain, domainName,userOU);
                GroupPrincipal user = GroupPrincipal.FindByIdentity(domainContext, "developer");
                var sid = user.Sid;
               
                ViewBag.Message += "</br></br> SID: " + user.Sid.ToString();
                NTAccount ntAccount = (NTAccount)sid.Translate(typeof(NTAccount));
                foreach (ActiveDirectoryAccessRule rule in sec.GetAccessRules(true, false, typeof(NTAccount)))
                {
                    if (rule.ObjectType.ToString() == sid.ToString())
                    {
                        
                        ViewBag.Message += "</br> Identity: " + rule.IdentityReference.ToString()
                                       + "</br> AccessControlType: " + rule.AccessControlType.ToString()
                                       + "</br> ActiveDirectoryRights: " + rule.ActiveDirectoryRights.ToString()
                                       + "</br> InheritanceType: " + rule.InheritanceType.ToString()
                                       + "</br> ObjectFlags: " + rule.ObjectFlags.ToString() + "</br>";
                    }
                }
               // ViewBag.Message += sec.GetOwner(typeof(ntAccount));
                //  ViewBag.Message += " Owner: " + sec.GetOwner(typeof(NTAccount("mylab.user", "user1")));
                //  ViewBag.Message += " Group: " + sec.GetOwner(typeof(Account));
            }
                catch 
                {

                    //throw;
                }

                ViewBag.Message += "</br></br>=====Security Descriptor=====";
                ViewBag.Message += " Owner: " + sec.GetOwner(typeof(NTAccount));
                ViewBag.Message += " Group: " + sec.GetOwner(typeof(NTAccount));


                AuthorizationRuleCollection rules = null;
                rules = sec.GetAccessRules(true, true, typeof(NTAccount));

                foreach (ActiveDirectoryAccessRule rule in rules)
                {
                    PrintAce(rule);
                    ViewBag.Message += "</br> Identity: " + rule.IdentityReference.ToString()
                                       + "</br> AccessControlType: " + rule.AccessControlType.ToString()
                                       + "</br> ActiveDirectoryRights: " + rule.ActiveDirectoryRights.ToString()
                                       + "</br> InheritanceType: " + rule.InheritanceType.ToString()
                                       + "</br> ObjectFlags: " + rule.ObjectFlags.ToString() + "</br>"
                                       +  "</br> ObjectType: " + rule.ObjectType.ToString() + "</br>";
            }
            
            return View();
        }

        public static void PrintAce(ActiveDirectoryAccessRule rule)
        {
            Console.WriteLine("=====ACE=====");
            Console.Write(" Identity: ");
            Console.WriteLine(rule.IdentityReference.ToString());
            Console.Write(" AccessControlType: ");
            Console.WriteLine(rule.AccessControlType.ToString());
            Console.Write(" ActiveDirectoryRights: ");
            Console.WriteLine(
            rule.ActiveDirectoryRights.ToString());
            Console.Write(" InheritanceType: ");
            Console.WriteLine(rule.InheritanceType.ToString());
            Console.Write(" ObjectType: ");
            if (rule.ObjectType == Guid.Empty)
                Console.WriteLine("");
            else
                Console.WriteLine(rule.ObjectType.ToString());

            Console.Write(" InheritedObjectType: ");
            if (rule.InheritedObjectType == Guid.Empty)
                Console.WriteLine("");
            else
                Console.WriteLine(
                rule.InheritedObjectType.ToString());
            Console.Write(" ObjectFlags: ");
            Console.WriteLine(rule.ObjectFlags.ToString());
        }

        public static void PrintSD(ActiveDirectorySecurity sd)
        {
            Console.WriteLine("=====Security Descriptor=====");
            Console.Write(" Owner: ");
            Console.WriteLine(sd.GetOwner(typeof(NTAccount)));
            Console.Write(" Group: ");
            Console.WriteLine(sd.GetGroup(typeof(NTAccount)));
        }





    }
}