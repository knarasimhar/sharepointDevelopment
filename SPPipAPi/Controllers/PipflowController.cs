using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using SPPipAPi.Models;
using System;
using System.Collections.Generic;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using SPPipAPi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Web;
using System.Web.Http;
using SP = Microsoft.SharePoint.Client;
using System.Configuration;
using System.Security;
using System.Web;
using System.Net.Http.Headers;

namespace SPPipAPi.Controllers
{
    public class PipflowController : ApiController
    {
        // GET api/values/5
       
        String strSiteURL = "http://sharepoint2/sites/teamsiteex/PipFlowSite", strUSER="spuser2",strPWD="User@123#",strADUserURL="";
       
        public PipflowController()
        {
            if (ConfigurationManager.AppSettings["SITE_URL"] != null)
                strSiteURL = ConfigurationManager.AppSettings["SITE_URL"].ToString();
            if (ConfigurationManager.AppSettings["SITE_URL_USER"] != null)
                strUSER = ConfigurationManager.AppSettings["SITE_URL_USER"].ToString();
            if (ConfigurationManager.AppSettings["SITE_URL_PWD"] != null)
                strPWD = ConfigurationManager.AppSettings["SITE_URL_PWD"].ToString();
            if (ConfigurationManager.AppSettings["AD_USER_URL"] != null)
                strADUserURL = ConfigurationManager.AppSettings["AD_USER_URL"].ToString();
            

            //strPWD = HttpUtility.UrlEncode(strPWD);
        }

        [Route("api/Pipflow/spgetuserinfo")]
        // [ActionName("spcheckuser")]
        [HttpGet]
       
        public HttpResponseMessage spgetuserinfo(string uname, string pwd)
        {

            // prepare site connection
            try
            {
                ClientContext context = new ClientContext(strSiteURL);
                context.Credentials = new NetworkCredential(uname, pwd);
                Web web = context.Web;
                context.Load(web);
                User user = context.Web.CurrentUser;
                context.Load(user);
                context.ExecuteQuery();
                UserInfo objUinfo = new UserInfo
                {
                    Id = user.Id.ToString(),
                    LoginName = user.LoginName,
                    title = user.Title
                };
                return getHttpResponseMessage(JsonConvert.SerializeObject(objUinfo));


            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);

            }



        }
        [Route("api/Pipflow/spcheckuser")]
        // [ActionName("spcheckuser")]
        [HttpGet]
        public HttpResponseMessage spcheckuser(string uname, string pwd)
        {

            // prepare site connection
            try
            {
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(uname, pwd);
                return getSuccessmessage("True");
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }


            return getSuccessmessage("False");
        }

        [Route("api/Pipflow/spgetListByName")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spgetListByName(string Listname)
        {

            // prepare site connection
            try
            {
                // global parameters<View><Query>


                CamlQuery camlQuery = new CamlQuery();

                camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";
                //camlQuery.ViewXml = "<Where><Eq><FieldRef Name='Author' LookupId='True' /><Value Type='User'>123</Value></Eq></Where>";
                // camlQuery.ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name='Author' LookupId='TRUE' /><Value Type='Integer'>{0}</Value></Eq></Where><View><Query>", 4);
                // camlQuery.ViewXml = "<View><Where><Eq><FieldRef Name='ry3a' /><Value Type='User'>SPM</Value></Eq></Where></View>";
                // camlQuery.ViewXml = "<View><Query><Where><Eq><FieldRefName='Author'LookupId='True'/><ValueType='Lookup'>" + 24 + "</Value></Eq></Where></Query></View>";
                // prepare site connection

                ClientContext clientContext = new ClientContext(strSiteURL);

                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);
                Web web = clientContext.Web;
                clientContext.Load(web);
                User user = clientContext.Web.CurrentUser;
                clientContext.Load(user);
                clientContext.ExecuteQuery();

                List list = web.Lists.GetByTitle(Listname);

                Console.WriteLine("Client context::  " + clientContext.ToString());
                ListItemCollection olists = list.GetItems(camlQuery);
                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(olists,
                     items => items.Include(

                        item => item.Id,
                        item => item["Title"],
                        item => item["ry3a"],
                        item => item["currentAssignee"],
                        item => item["status"],
                        item => item["remarks"],
                        item => item["Editor"],
                        item => item["Author"]));
                clientContext.ExecuteQuery();
                List<pipflow> respmsg = new List<pipflow>();

                FieldUserValue fuvassigned_to = null;
                FieldUserValue fuvCurassigned_to = null;
                FieldUserValue fuvEditor = null;
                FieldUserValue fuvAuthor = null;


                foreach (ListItem oListItem in olists)
                {

                    if (oListItem["currentAssignee"] != null)
                        fuvCurassigned_to = (FieldUserValue)oListItem["currentAssignee"];
                    if (oListItem["ry3a"] != null)
                        fuvassigned_to = (FieldUserValue)oListItem["ry3a"];
                    if (oListItem["Editor"] != null)
                        fuvEditor = (FieldUserValue)oListItem["Editor"];
                    if (oListItem["Author"] != null)
                        fuvAuthor = (FieldUserValue)oListItem["Author"];
                    respmsg.Add(new pipflow
                    {
                        id = oListItem.Id.ToString(),
                        title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                        assigned_to = (oListItem["ry3a"] != null) ? fuvassigned_to.LookupValue : "",
                        assigned_to_id = (oListItem["ry3a"] != null) ? fuvassigned_to.LookupId.ToString() : "",
                        status = (oListItem["status"] != null) ? oListItem["status"].ToString() : "",
                        remarks = (oListItem["remarks"] != null) ? oListItem["remarks"].ToString() : "",
                        Modified_By = (oListItem["Editor"] != null) ? fuvEditor.LookupValue : "",
                        Modified_By_id = (oListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                        Created_By = (oListItem["Author"] != null) ? fuvAuthor.LookupValue : "",
                        Created_By_id = (oListItem["Author"] != null) ? fuvAuthor.LookupId.ToString() : "",
                        currentassign_to = (oListItem["currentAssignee"] != null) ? fuvCurassigned_to.LookupValue : "",
                        currentassign_to_id = (oListItem["currentAssignee"] != null) ? fuvCurassigned_to.LookupId.ToString() : ""
                    });


                }

                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }



        }


        [Route("api/Pipflow/spsetFMR")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spsetFMR(string fmrid, string remarks, string Listname)
        {

            // prepare site connection
            try
            {
                // global parameters

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";

                // prepare site connection
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER,strPWD);



                List oList = clientContext.Web.Lists.GetByTitle("pipflow1");

                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                ListItem oListItem = oList.AddItem(itemCreateInfo);

                oListItem["Title"] = fmrid;
                oListItem["remarks"] = remarks;

                //oListItem["Body"] = "Hello World!";

                oListItem.Update();

                clientContext.ExecuteQuery();

            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }

            return getSuccessmessage("success");
        }


        [Route("api/Pipflow/spgetListItemByID")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spgetListItemByID(string Listname, string ListitemId)
        {

            // prepare site connection
            try
            {
                // global parameters

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><RowLimit>10000</RowLimit></View>";
                camlQuery.ViewXml = "<View><Where><Eq><FieldRef Name='ry3a' /><Value Type='User'>SPM</Value></Eq></Where></View>";
                // prepare site connection
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);


                Web web = clientContext.Web;
                clientContext.Load(web);
                List list = web.Lists.GetByTitle(Listname);
                ListItem targetListItem = list.GetItemById(ListitemId);
                //clientContext.ExecuteQuery();

                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(targetListItem, item => item["Title"],
                        item => item["Title"],
                        item => item["ry3a"],
                        item => item["currentAssignee"],
                        item => item["status"],
                        item => item["remarks"],
                        item => item["Editor"],
                        item => item["Author"]);
                clientContext.ExecuteQuery();

                List<pipflow> respmsg = new List<pipflow>();
                FieldUserValue fuvassigned_to = null;
                FieldUserValue fuvCurassigned_to = null;
                FieldUserValue fuvEditor = null;
                FieldUserValue fuvAuthor = null;
                if (targetListItem["currentAssignee"] != null)
                    fuvCurassigned_to = (FieldUserValue)targetListItem["currentAssignee"];

                if (targetListItem["ry3a"] != null)
                    fuvassigned_to = (FieldUserValue)targetListItem["ry3a"];
                if (targetListItem["Editor"] != null)
                    fuvEditor = (FieldUserValue)targetListItem["Editor"];
                if (targetListItem["Author"] != null)
                    fuvAuthor = (FieldUserValue)targetListItem["Author"];
                respmsg.Add(new pipflow
                {

                    //id = (targetListItem.Id != null) ? targetListItem.Id.ToString() : "",
                    title = (targetListItem["Title"] != null) ? targetListItem["Title"].ToString() : "",
                    assigned_to = (targetListItem["ry3a"] != null) ? fuvassigned_to.LookupValue : "",
                    assigned_to_id = (targetListItem["ry3a"] != null) ? fuvassigned_to.LookupId.ToString() : "",
                    status = (targetListItem["status"] != null) ? targetListItem["status"].ToString() : "",
                    remarks = (targetListItem["remarks"] != null) ? targetListItem["remarks"].ToString() : "",
                    Modified_By = (targetListItem["Editor"] != null) ? fuvEditor.LookupValue : "",
                    Modified_By_id = (targetListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                    Created_By = (targetListItem["Author"] != null) ? fuvAuthor.LookupValue : "",
                    Created_By_id = (targetListItem["Author"] != null) ? fuvAuthor.LookupId.ToString() : "",
                    currentassign_to = (targetListItem["currentAssignee"] != null) ? fuvCurassigned_to.LookupValue : "",
                    currentassign_to_id = (targetListItem["currentAssignee"] != null) ? fuvCurassigned_to.LookupId.ToString() : ""

                });
                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));



            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }



        }


        // below for workflow task assign and reject and others tagas 
        [Route("api/Pipflow/spsetTaskItemByID")]

        [HttpGet, HttpPost]
       
        public HttpResponseMessage spsetTaskItemByID(string status, string percentComplete, string Comments, string createdby, string taskid, string assignevent = "")
        {
            // prepare site connection
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

            try
            {
                //Get the list items from list
                SP.List oList = clientContext.Web.Lists.GetByTitle("Workflow Tasks");

                SP.ListItem list2 = oList.GetItemById(Int32.Parse(taskid));

               
                User user = clientContext.Web.EnsureUser(@"i:0#.w|saathispdt\" + HttpUtility.UrlDecode(createdby));
                clientContext.Load(user);
                //list2["AssignedTo"] = @"it1";
                //list2["Completed"] = true;
                if (status.ToLower() != "save")
                {
                    list2["PercentComplete"] = 1;
                    list2["Status"] = status;
                    list2["TaskOutcome"] = status;
                    list2["comments"] = Comments;
                    list2["event"] = assignevent;
                }
                else
                {
/*                    Approved, //0
    Denied,   //1
    Pending,  //2
    Draft,    //3
    Scheduled //4*/

                    list2["_ModerationStatus"] = 3;
                    list2["PercentComplete"] = percentComplete;
                    list2["comments"] = Comments;
                }
                // list2["Status"] = "Rejected";
                // list2["TaskOutcome"] = "Rejected";
                list2.Update();
                clientContext.ExecuteQuery();
            }
            catch (Exception ex)
            {

                return getErrormessage(ex.Message);
            }
            return getSuccessmessage("Success");

        }

        // below for workflow task assign and reject and others tagas 
        [Route("api/Pipflow/spsetAddorupdteItemByID")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spsetAddorupdteItemByID(string status, string Listname,string Comments, string createdby, string itemid,string keyvalue)
        {
            // prepare site connection
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

            try
            {
                //Get the list items from list
                SP.List oList = clientContext.Web.Lists.GetByTitle(Listname);

                SP.ListItem list2 = oList.GetItemById(Int32.Parse(itemid));
                User user = clientContext.Web.EnsureUser(@"i:0#.w|saathispdt\" + HttpUtility.UrlDecode(createdby));
                clientContext.Load(user);
                //list2["AssignedTo"] = @"it1";
                //list2["Completed"] = true;
                
                list2["comments"] = Comments;
               
                // list2["Status"] = "Rejected";
                // list2["TaskOutcome"] = "Rejected";
                list2.Update();
                clientContext.ExecuteQuery();
            }
            catch (Exception ex)
            {

                return getErrormessage(ex.Message);
            }
            return getSuccessmessage("Success");

        }

        [Route("api/Pipflow/spgetTaskDetailsByuser")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spgetTaskDetailsByuser(string Listname, string ListitemId)
        {

            // prepare site connection
            try
            {
                // global parameters

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<Where><Eq><FieldRef Name='Author' LookupId='True' /><Value Type='User'>123</Value></Eq></Where>";
                //<View><RowLimit>1000</RowLimit></View>
                // prepare site connection
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);


                Web web = clientContext.Web;
                clientContext.Load(web);


                List list = web.Lists.GetByTitle("Workflow Tasks");
                ListItem targetListItem = list.GetItemById(ListitemId);
                //clientContext.ExecuteQuery();

                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(targetListItem, item => item["Title"],
                         item => item["Title"],
                        item => item["TaskOutcome"],
                        item => item["RelatedItems"],
                        item => item["Status"],
                        item => item["AssignedTo"],
                        item => item["Editor"],
                        item => item["Author"]);
                clientContext.ExecuteQuery();

                List<pipflow> respmsg = new List<pipflow>();
                // create and cast the FieldUserValue from the value
                FieldUserValue fuvAssignedTo = null;
                FieldUserValue fuvEditor = null;
                FieldUserValue fuvAuthor = null;
                if (targetListItem["AssignedTo"] != null)
                    foreach (FieldUserValue userValue in targetListItem["AssignedTo"] as FieldUserValue[])
                    {
                        //string test = userValue.LookupId;
                        fuvAssignedTo = userValue;
                    }

                if (targetListItem["Editor"] != null)
                    fuvEditor = (FieldUserValue)targetListItem["Editor"];
                if (targetListItem["Author"] != null)
                    fuvAuthor = (FieldUserValue)targetListItem["Author"];

                string RelItem = "";

                if (targetListItem["RelatedItems"].ToString() != "")
                {

                    List<RelatedItemFieldValue> obj = JsonConvert.DeserializeObject<List<RelatedItemFieldValue>>(targetListItem["RelatedItems"].ToString());
                    if (obj != null)
                        RelItem = obj[0].ItemId.ToString();
                }

                respmsg.Add(new pipflow
                {

                    //id = (targetListItem.Id != null) ? targetListItem.Id.ToString() : "",
                    title = (targetListItem["Title"] != null) ? targetListItem["Title"].ToString() : "",
                    taskoutcome = (targetListItem["TaskOutcome"] != null) ? targetListItem["TaskOutcome"].ToString() : "",
                    RelatedItems = (targetListItem["RelatedItems"] != null) ? RelItem : "",
                    status = (targetListItem["Status"] != null) ? targetListItem["Status"].ToString() : "",
                    assigned_to = (targetListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupValue : "",
                    assigned_to_id = (targetListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                    Modified_By = (targetListItem["Editor"] != null) ? fuvEditor.LookupValue : "",
                    Modified_By_id = (targetListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                    Created_By = (targetListItem["Author"] != null) ? fuvAuthor.LookupValue : "",
                    Created_By_id = (targetListItem["Author"] != null) ? fuvAuthor.LookupId.ToString() : ""

                });
                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));




            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }

        }

        [Route("api/Pipflow/spgetTaskDetails")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spgetTaskDetails(string Listname, string Taskuser = null, string ReleatedItems = null)
        {
            // prepare site connection
            try
            {
                // global parameters
                Taskuser = Taskuser == null ? "" : Taskuser;
                ReleatedItems = ReleatedItems == null ? "" : "," + ReleatedItems + ",";

                CamlQuery camlQuery = new CamlQuery();
                //camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";
                camlQuery.ViewXml = string.Format("<View><Where><Eq><FieldRef Name='AssignedTo' LookupId='TRUE'  /><Value Type='Text'>{0}</Value></Eq></Where></View>", Taskuser);
                // camlQuery.ViewXml = "<View><Where><Eq><FieldRef Name='AssignedTo' /><Value Type='User'>" + Taskuser + "</Value></Eq></Where></View>";
                //camlQuery.ViewXml = "<Where><And><Or><Membership Type='CurrentUserGroups'><FieldRef Name='AssignedTo' /></Membership><Eq><FieldRef Name='AssignedTo'  LookupId='TRUE' /><Value Type='Lookup'>27</Value></Eq></Or><Neq><FieldRef Name='Status' /><Value Type='Text'>Completed</Value></Neq></And></Where>";

                //camlQuery. = "<Where><Or><Eq><FieldRef Name='AssignedTo' LookupId='TRUE'/><Value Type='Integer'><UserID /></Value></Eq><Membership Type='CurrentUserGroups'><FieldRef Name='AssignedTo'/></Membership></Or></Where>";
                // prepare site connection
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);


                Web web = clientContext.Web;
                clientContext.Load(web);
                List list = web.Lists.GetByTitle("Workflow Tasks");
                var field = list.Fields.GetByInternalNameOrTitle("AssignedTo");
                var lookupField = clientContext.CastTo<FieldLookup>(field);
                clientContext.Load(lookupField);
                clientContext.ExecuteQuery();
                var lookupListId = new Guid(lookupField.LookupList); //returns associated list id
                                                                     //Retrieve associated List
                var lookupList = clientContext.Web.Lists.GetById(lookupListId);
                clientContext.Load(lookupList);
                clientContext.ExecuteQuery();
                Console.WriteLine("Client context::  " + clientContext.ToString());
                ListItemCollection olists = list.GetItems(camlQuery);
                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(olists,
                     items => items.Include(

                        item => item.Id,
                        item => item["Title"],
                        item => item["TaskOutcome"],
                         item => item["RelatedItems"],
                        item => item["Status"],
                        item => item["AssignedTo"],
                        item => item["Editor"],
                        item => item["Author"]));
                clientContext.ExecuteQuery();
                List<pipflow> respmsg = new List<pipflow>();

                foreach (ListItem oListItem in olists)
                {
                    // create and cast the FieldUserValue from the value
                    FieldUserValue fuvAssignedTo = null;
                    FieldUserValue fuvEditor = null;
                    FieldUserValue fuvAuthor = null;
                    if (oListItem["AssignedTo"] != null)
                        foreach (FieldUserValue userValue in oListItem["AssignedTo"] as FieldUserValue[])
                        {
                            //string test = userValue.LookupId;
                            fuvAssignedTo = userValue;
                        }

                    // assigned to for listing the data
                    if (oListItem["AssignedTo"] != null && Taskuser != "")
                        if (fuvAssignedTo.LookupValue.ToLower() != Taskuser.ToLower()) continue;

                    if (oListItem["Editor"] != null)
                        fuvEditor = (FieldUserValue)oListItem["Editor"];
                    if (oListItem["Author"] != null)
                        fuvAuthor = (FieldUserValue)oListItem["Author"];
                    string RelItem = "";
                    if (oListItem["RelatedItems"].ToString() != "")
                    {

                        List<RelatedItemFieldValue> obj = JsonConvert.DeserializeObject<List<RelatedItemFieldValue>>(oListItem["RelatedItems"].ToString());
                        if (obj != null)
                            RelItem = obj[0].ItemId.ToString();
                    }

                    // related item to for listing the data filtering
                    if (ReleatedItems != "" && ReleatedItems.Contains("," + RelItem + ",") != true)
                        continue;
                    respmsg.Add(new pipflow
                    {
                        id = oListItem.Id.ToString(),
                        title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                        taskoutcome = (oListItem["TaskOutcome"] != null) ? oListItem["TaskOutcome"].ToString() : "",
                        RelatedItems = (oListItem["RelatedItems"] != null) ? RelItem : "",
                        status = (oListItem["Status"] != null) ? oListItem["Status"].ToString() : "",
                        assigned_to = (oListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupValue : "",
                        assigned_to_id = (oListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                        Modified_By = (oListItem["Editor"] != null) ? fuvEditor.LookupValue : "",
                        Modified_By_id = (oListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                        Created_By = (oListItem["Author"] != null) ? fuvAuthor.LookupValue : "",
                        Created_By_id = (oListItem["Author"] != null) ? fuvAuthor.LookupId.ToString() : ""

                    });


                }

                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }


        }

        [Route("api/Pipflow/spgetWFEventDetailsByUser")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spgetWFEventDetailsByUser(string Listname, string Eventuser = null)
        {
            // prepare site connection
            try
            {
                // global parameters
                Eventuser = Eventuser == null ? "" : Eventuser;

                CamlQuery camlQuery = new CamlQuery();
                //camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";
                camlQuery.ViewXml = string.Format("<View><Where><Eq><FieldRef Name='AssignedTo' LookupId='TRUE'  /><Value Type='Text'>{0}</Value></Eq></Where></View>", Eventuser);
                // camlQuery.ViewXml = "<View><Where><Eq><FieldRef Name='AssignedTo' /><Value Type='User'>" + Taskuser + "</Value></Eq></Where></View>";
                //camlQuery.ViewXml = "<Where><And><Or><Membership Type='CurrentUserGroups'><FieldRef Name='AssignedTo' /></Membership><Eq><FieldRef Name='AssignedTo'  LookupId='TRUE' /><Value Type='Lookup'>27</Value></Eq></Or><Neq><FieldRef Name='Status' /><Value Type='Text'>Completed</Value></Neq></And></Where>";

                //camlQuery. = "<Where><Or><Eq><FieldRef Name='AssignedTo' LookupId='TRUE'/><Value Type='Integer'><UserID /></Value></Eq><Membership Type='CurrentUserGroups'><FieldRef Name='AssignedTo'/></Membership></Or></Where>";
                // prepare site connection
                ClientContext clientContext = new ClientContext("http://spnarasimha/sites/narasimha/pip");
                clientContext.Credentials = new NetworkCredential("spm", "pip@123");


                Web web = clientContext.Web;
                clientContext.Load(web);
                List list = web.Lists.GetByTitle("pipdept");

                clientContext.ExecuteQuery();


                ListItemCollection olists = list.GetItems(camlQuery);
                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(olists,
                     items => items.Include(

                        item => item.Id,
                        item => item["Title"],
                        item => item["assignee"],
                         item => item["approver"],
                        item => item["ekfw"],
                        item => item["event"]));
                clientContext.ExecuteQuery();
                List<pipflowevents> respmsg = new List<pipflowevents>();

                foreach (ListItem oListItem in olists)
                {
                    // create and cast the FieldUserValue from the value
                    FieldUserValue fuvAssignedTo = null;
                    FieldUserValue fuvEditor = null;
                    FieldUserValue fuvAuthor = null;
                    /*  if (oListItem["assignee"] != null)
                          foreach (FieldUserValue userValue in oListItem["assignee"] as FieldUserValue[])
                          {
                              //string test = userValue.LookupId;
                              fuvAssignedTo = userValue;
                          }*/

                    // assigned to for listing the data

                    if (oListItem["assignee"] != null)
                        fuvAssignedTo = (FieldUserValue)oListItem["assignee"];
                    if (oListItem["assignee"] != null && Eventuser != "")
                        if (fuvAssignedTo.LookupValue.ToLower() != Eventuser.ToLower()) continue;

                    if (oListItem["approver"] != null)
                        fuvEditor = (FieldUserValue)oListItem["approver"];
                    if (oListItem["ekfw"] != null)
                        fuvAuthor = (FieldUserValue)oListItem["ekfw"];



                    respmsg.Add(new pipflowevents
                    {
                        id = oListItem.Id.ToString(),
                        title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                        flowevent = (oListItem["event"] != null) ? oListItem["event"].ToString() : "",
                        assigned_to = (oListItem["assignee"] != null) ? fuvAssignedTo.LookupValue : "",
                        assigned_to_id = (oListItem["assignee"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                        approved_to = (oListItem["approver"] != null) ? fuvEditor.LookupValue : "",
                        approved_to_id = (oListItem["approver"] != null) ? fuvEditor.LookupId.ToString() : "",
                        rejected_to = (oListItem["ekfw"] != null) ? fuvAuthor.LookupValue : "",
                        rejected_to_id = (oListItem["ekfw"] != null) ? fuvAuthor.LookupId.ToString() : ""

                    });


                }

                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }


        }
        // start user active directory calls to servers
        [System.Web.Http.Route("api/AduVerify/ADAddUser")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage ADAddUser(CreateUser model)
        {
            return getErrormessage("success");
        }
        [System.Web.Http.Route("api/AduVerify/getADUsers")]
        [System.Web.Http.HttpGet, System.Web.Http.HttpPost]
        public HttpResponseMessage getADUsers(string OUNAMES)
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(strADUserURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // New code:
                HttpResponseMessage response =  client.GetAsync("api/ADUVerify/getADUsers?oustates=" + OUNAMES).Result ;
                if (response.IsSuccessStatusCode)
                {
                    CreateUser Users = null;// response.Content.ReadAsAsync<List<CreateUser>>();
                   
                }
            }
            return getErrormessage("success");
        }
        /*
        private  HttpResponseMessage  DoWebRequest(string endpoint, string reqtype,object obj)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                PreAuthenticate = true,
                UseDefaultCredentials = true
            };


            string reasonPhrase = "";
            using (var client = new HttpClient(handler))
            {
                client.BaseAddress = new Uri(endpoint);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
               
                if (reqtype.ToUpper()=="GET")
                    var  response = client.GetAsync(endpoint).Result;
                else 
                 var  response = client.PostAsJsonAsync(endpoint, obj).Result;

                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsStringAsync().Result;
                    return result;
                }
                else
                {
                    reasonPhrase = response.ReasonPhrase;
                    if (reasonPhrase.ToUpper() == "UNAUTHORIZED")
                    {
                        throw new KeyNotFoundException("Not authorized");
                    }
                    
                }
            }
        }*/
        [System.Web.Http.Route("api/AduVerify/ADUpdateUser")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage ADUpdateUser(CreateUser model)
        {
            return getErrormessage("success");
        }
        // ENd user active directory calls to servers
        private SecureString getSecuredString(string strPWD)
        {
            SecureString strSCPWD = new SecureString();
            foreach (var b in Encoding.Default.GetBytes(strPWD))
                strSCPWD.AppendChar((char)b);
           
            return strSCPWD;
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

        public string Get()
        {
            return "Welcome To Web API";
        }
        public List<string> Get(int Id)
        {
            return new List<string> {
                "Data1",
                "Data2"
            };
        }


    }
}

