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
using System.Threading.Tasks;
using System.IO;
using System.Web.Http.Cors;
using System.Xml;
using System.Data;
using System.Xml.Linq;
using System.Threading;

namespace SPPipAPi.Controllers
{
    [EnableCors(origins: "http://sharepoint2:8081,http://localhost:44349", headers: "*", methods: "*")]
    public class PipflowController : ApiController
    {
        // GET api/values/5
        String strSiteURL = "http://sharepoint2/sites/teamsiteex/PipFlowSite", strUSER = "spuser2", strPWD = "User@123#", strADUserURL = "", SitepathValue = "";
        string SITE_API_URL = "";
        string strDomainName = ConfigurationManager.AppSettings["DomainName"].ToString();
        Boolean isWait = false;
        public PipflowController()
        {
            if (ConfigurationManager.AppSettings["SITE_URL"] != null)
                strSiteURL = ConfigurationManager.AppSettings["SITE_URL"].ToString();
            if (ConfigurationManager.AppSettings["SITE_API_URL"] != null)
                SITE_API_URL = ConfigurationManager.AppSettings["SITE_API_URL"].ToString();
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

        //[System.Web.Http.Route("api/Pipflow/getADUser")]

        //[System.Web.Http.HttpGet, System.Web.Http.HttpPost]

        //public HttpResponseMessage getADUsers(string OUNAMES)
        //{
        //    List<CreateUser> userlist = new List<CreateUser>();
        //    foreach (string OU in OUNAMES.Split(','))
        //    {
        //        try
        //        {
        //            if (ConfigurationManager.AppSettings["AD_USER_URL"] != null)
        //            {

        //                string strADUserURL = ConfigurationManager.AppSettings["AD_USER_URL"].ToString();
        //                string url = strADUserURL + "getADUsers?OUNAMES="+ OUNAMES;

        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return getErrormessage(ex.Message);
        //        }

        //        return getHttpResponseMessage(JsonConvert.SerializeObject(userlist));
        //    }           
        //}    



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
        public HttpResponseMessage spgetListByName(string Listname, string status = "", string ListitemId = "")
        {

            // prepare site connection
            try
            {
                // global parameters<View><Query>

                if (status == null) status = "";

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
                ListItemCollection olists;
                /*  if (ListitemId != null && ListitemId != "")
                  {
                      ListItem Litem = list.GetItemById(ListitemId);
                      //olists = new ListItemCollection;

                      clientContext.Load(Litem,item => item.Include(
                         item => item["Title"],
                         item => item["ry3a"],
                         item => item["currentAssignee"],
                         item => item["status"],
                         item => item["remarks"],
                         item => item["Editor"],
                         item => item["Author"]);
                  }
                  else*/
                olists = list.GetItems(camlQuery);
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
                        item => item["Author"],
                        item => item["currenttaskid"]));
                clientContext.ExecuteQuery();
                List<pipflow> respmsg = new List<pipflow>();

                FieldUserValue[] fuvassigned_to = null;
                FieldUserValue[] fuvCurassigned_to = null;

                FieldUserValue fuvEditor = null;
                FieldUserValue fuvAuthor = null;


                foreach (ListItem oListItem in olists)
                {
                    string strLookupValues = "", strLookupIDS = "", strLookupAssignTOIds = "", strLookupAssignTOvalues = "";
                    if (status != "" && oListItem["status"].ToString().ToLower() != status.ToLower())
                        continue;
                    if (oListItem["currentAssignee"] != null)
                        fuvCurassigned_to = (FieldUserValue[])oListItem["currentAssignee"];
                    if (oListItem["ry3a"] != null)
                        fuvassigned_to = (FieldUserValue[])oListItem["ry3a"];
                    if (oListItem["Editor"] != null)
                        fuvEditor = (FieldUserValue)oListItem["Editor"];
                    if (oListItem["Author"] != null)
                        fuvAuthor = (FieldUserValue)oListItem["Author"];

                    if (fuvCurassigned_to != null)
                    {
                        foreach (FieldUserValue FUV in fuvCurassigned_to)
                        {
                            strLookupIDS += FUV.LookupId.ToString() + ",";
                            strLookupValues += FUV.LookupValue + ",";
                        }
                    }

                    if (fuvCurassigned_to != null)
                    {
                        foreach (FieldUserValue FUV in fuvassigned_to)
                        {
                            strLookupAssignTOIds += FUV.LookupId.ToString() + ",";
                            strLookupAssignTOvalues += FUV.LookupValue + ",";
                        }
                    }

                    respmsg.Add(new pipflow
                    {
                        id = oListItem.Id.ToString(),
                        title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                        assigned_to = (oListItem["ry3a"] != null) ? strLookupAssignTOvalues.TrimEnd(',') : "",
                        assigned_to_id = (oListItem["ry3a"] != null) ? strLookupAssignTOIds.TrimEnd(',') : "",
                        status = (oListItem["status"] != null) ? oListItem["status"].ToString() : "",
                        remarks = (oListItem["remarks"] != null) ? oListItem["remarks"].ToString() : "",
                        currenttaskid = (oListItem["currenttaskid"] != null) ? oListItem["currenttaskid"].ToString() : "",
                        Modified_By = (oListItem["Editor"] != null) ? fuvEditor.LookupValue : "",
                        Modified_By_id = (oListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                        Created_By = (oListItem["Author"] != null) ? fuvAuthor.LookupValue : "",
                        Created_By_id = (oListItem["Author"] != null) ? fuvAuthor.LookupId.ToString() : "",
                        currentassign_to = (oListItem["currentAssignee"] != null) ? strLookupValues.TrimEnd(',') : "",
                        currentassign_to_id = (oListItem["currentAssignee"] != null) ? strLookupIDS.TrimEnd(',') : ""
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
        public HttpResponseMessage spsetFMR(string fmrid, string remarks, string Listname, string AssignedTo = "", string FY = "", string stateid = "")
        {
            // string createdby, string taskid, string assignevent = "", string AssignedTo = ""
            // prepare site connection
            try
            {
                // global parameters

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";

                // prepare site connection
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);



                List oList = clientContext.Web.Lists.GetByTitle("pipflow1");

                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                ListItem oListItem = oList.AddItem(itemCreateInfo);

                oListItem["Title"] = fmrid;
                oListItem["remarks"] = remarks;
                if (AssignedTo != "")
                {
                    User uAssignedTo = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(AssignedTo));
                    oListItem["ry3a"] = uAssignedTo;
                }

                oListItem["FY"] = FY;
                oListItem["stateid"] = stateid;

                //oListItem["Body"] = "Hello World!";

                oListItem.Update();

                clientContext.ExecuteQuery();
                //ListItem targetListItem = oList.(ListitemId);

                /*   isWait = true;
                   getLatestTaskIDByFMRNO(string.Format(SITE_API_URL + "/api/Pipflow/spgetTaskDetails?listname&taskuser={0}&ReleatedItems={1}&status=not started", "SPM", fmrid));
                   */
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }

            return getSuccessmessage("success");
        }

        [Route("api/Pipflow/spupdateFMR")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spupdateFMR(string Listname, string fmrSPid, string remarks, string status)
        {

            // prepare site connection
            try
            {
                // global parameters

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";

                // prepare site connection
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);



                List oList;
                if (Listname != "")
                    oList = clientContext.Web.Lists.GetByTitle(Listname);
                else
                    oList = clientContext.Web.Lists.GetByTitle("pipflow1");

                ListItem targetListItem = oList.GetItemById(fmrSPid);



                if (remarks != "")
                    targetListItem["remarks"] = remarks;
                if (status != "")
                    targetListItem["status"] = status;

                //oListItem["Body"] = "Hello World!";

                targetListItem.Update();

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
        // TASKTYPE 1 FOR NORMAL,2 FOR ADD REVIEW,3 FOR ROP
        // below for workflow task assign and reject and others tagas 
        [Route("api/Pipflow/spsetTaskItemByID")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spsetTaskItemByID(string status, string percentComplete, string Comments, string createdby, string taskid, string assignevent = "", string AssignedTo = "", string areviewuserTo = "", string SPFmrID = "", string TASKTYPE = "")
        {
            // prepare site connection
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential("spm", "pip@123");
            if (AssignedTo == null) AssignedTo = "";

            try
            {

                /* multi users assign
                 

                // Get the Look Up values from lookup coloum.
                    string rawvalue = item["MyLookUpCol"].ToString();

                    // Print information about the lookup values.
                    SPFieldLookupValueCollection values = new SPFieldLookupValueCollection(rawvalue);
                    
                    Console.WriteLine("Toltal Values: {0}", values.Count);

                    // Print each value.
                    foreach (SPFieldLookupValue value in values)
                        Console.WriteLine("\t{0} (Value {1})", value.LookupValue, value.LookupId);
                
    */




                //Get the list items from list
                SP.List oList = clientContext.Web.Lists.GetByTitle("Workflow Tasks");
                SP.ListItem list2 = oList.GetItemById(Int32.Parse(taskid));
                User createuser = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(createdby));
                User assignuser = null;
                clientContext.Load(createuser);
                int i = 0;
                if (AssignedTo != "")
                {
                    FieldUserValue[] userValueCollection = new FieldUserValue[AssignedTo.Split(',').Length];
                    //for multiple assigies should be send , separate paramers

                    foreach (string auser in AssignedTo.Split(','))
                    {
                        assignuser = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(auser));

                        clientContext.Load(assignuser);
                        clientContext.ExecuteQuery();
                        if (assignuser != null)
                        {

                            FieldUserValue fieldUserVal = new FieldUserValue();
                            fieldUserVal.LookupId = assignuser.Id;
                            //fieldUserVal.LookupValue = assignuser.LoginName;

                            userValueCollection.SetValue(fieldUserVal, i);
                            i++;

                        }

                    }
                    list2["approveduser"] = userValueCollection;
                }



                //list2["AssignedTo"] = @"it1";
                //list2["Completed"] = true;



                if (AssignedTo != "")
                {

                    list2["AssignedTo"] = createuser;


                    /* old list2["approveduser"] = assignuser;
                    list2["AssignedTo"] = createuser;*/


                    /* sub tqaswk creations */



                    /*  if (list2["RelatedItems"] != null)
                      {

                          List<RelatedItemFieldValue> obj = JsonConvert.DeserializeObject<List<RelatedItemFieldValue>>(list2["RelatedItems"].ToString());
                          if (obj != null)
                              strRelatedID = obj[0].ItemId.ToString();
                      }*/

                    if (Comments.ToLower() == "!comments!")
                    {
                        list2["PercentComplete"] = 1;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = status;
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = TASKTYPE;

                    }
                    else
                    {

                        list2["PercentComplete"] = 1;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = status;
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = TASKTYPE;
                        //list2["comments"] = Comments;
                    }
                    // list2["Status"] = "Rejected";
                    // list2["TaskOutcome"] = "Rejected";
                    list2.Update();
                    clientContext.ExecuteQuery();
                }


                if (areviewuserTo != null && areviewuserTo != "" && (TASKTYPE == "2" || TASKTYPE == "3"))
                {
                    ListItemCreationInformation oListItemCreationInformation = new ListItemCreationInformation();
                    ListItem oItem = oList.AddItem(oListItemCreationInformation);


                    var lookupValue = new FieldLookupValue();
                    lookupValue.LookupId = int.Parse(taskid); // Get parent item ID and assign it value in lookupValue.LookupId  
                    var lookupValueCollection = new FieldLookupValue[1];
                    lookupValueCollection.SetValue(lookupValue, 0);

                    FieldUserValue[] AreveiweruserValueCollection = new FieldUserValue[areviewuserTo.Split(',').Length];
                    //for multiple assigies should be send , separate paramers
                    i = 0;
                    foreach (string auser in areviewuserTo.Split(','))
                    {
                        assignuser = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(auser));
                        clientContext.Load(assignuser);
                        clientContext.ExecuteQuery();
                        if (assignuser != null)
                        {

                            FieldUserValue fieldUserVal = new FieldUserValue();
                            fieldUserVal.LookupId = assignuser.Id;

                            //fieldUserVal.l = assignuser.LoginName;
                            AreveiweruserValueCollection.SetValue(fieldUserVal, i);
                            i++;

                        }

                    }

                    oItem["AssignedTo"] = AreveiweruserValueCollection;
                    if(TASKTYPE=="2")
                    oItem["Title"] = "Additional Review";
                    else if (TASKTYPE == "3")
                        oItem["Title"] = "ROP";
                    else
                        oItem["Title"] = "sub task";
                    oItem["ParentID"] = lookupValueCollection; // set chidl item ParentID field  
                    oItem["tasktype"] = TASKTYPE;
                    oItem.Update();
                    clientContext.ExecuteQuery();
                }



                // updated latest task id to FMR list for viewing
                //http://52.172.200.35:2020/sppipapidevtesting/api/Pipflow/spgetTaskDetails?listname&taskuser=&ReleatedItems=82&status=not started

                if (SPFmrID != null && SPFmrID != "" && AssignedTo != null && AssignedTo != "")
                {
                    isWait = true;
                    getLatestTaskIDByFMRNO(string.Format(SITE_API_URL + "/api/Pipflow/spgetTaskDetails?listname&taskuser={0}&ReleatedItems={1}&status=not started", AssignedTo, SPFmrID));
                }
                //end of the 
            }
            catch (Exception ex)
            {

                return getErrormessage(ex.Message);
            }
            return getSuccessmessage("Success");

        }

        private void getLatestTaskIDByFMRNO(string _Url)
        {

            //http://52.172.200.35:2020/sppipapidevtesting/api/Pipflow/spgetTaskDetails?listname&taskuser=&ReleatedItems=82&status=not started
            // http://sharepoint2/sites/teamsiteex/pipflowsitetesting/_api/web/lists/getbytitle('Workflow%20Tasks')/items?$top=1&$orderby=Id%20desc
            if (isWait)
                Thread.Sleep(10000);
            string strResp = ClsGeneral.DoWebGetRequest(_Url, "");

            /*  string[] strRespsplit1 = strResp.Split(new string[] { "<d:ID" },StringSplitOptions.None);
               string[] strRespsplit2 = strRespsplit1[1].Split(new string[] { "</d:ID" }, StringSplitOptions.None);
               string[] strRespsplit3 = strRespsplit2[0].Split(new string[] { ">" }, StringSplitOptions.None);
               //< d:RelatedItems >[{ "ItemId":200,"WebId":"b99be816-7288-42b3-8fb3-e82ae1968aaa","ListId":"3f0b3b6a-3038-4154-8ac9-6f95e99454fd"}]</ d:RelatedItems >
               string[] strRespsplit4 = strRespsplit1[0].Split(new string[] { "ItemId" }, StringSplitOptions.None);
               string[] strRespsplit5 = strRespsplit4[1].Split(new string[] { "WebId" }, StringSplitOptions.None);
               string strRelID = strRespsplit5[0].Replace(":","").Replace(",","").Replace("\"","");
               string strTaskID = strRespsplit3[1];
               //string id = xdoc.SelectSingleNode(@"ns:feed/ns:entry/ns:content/ns:m:properties");
               // xdoc.GetElementById("m:type")
               //update latest taskid to fmr current taskid field
               spsetAddorupdteItemByID("", "pipflow1", "","",strRelID,"",strTaskID);*/
            List<pipflow> pipfs = JsonConvert.DeserializeObject<List<pipflow>>(strResp);
            foreach (pipflow pipf in pipfs)
            {
                spsetAddorupdteItemByID("", "pipflow1", "", "", pipf.RelatedItems, "", pipf.id); break;
            }
            return;

        }
        // below for workflow task assign and reject and others tagas 
        [Route("api/Pipflow/spsetAddorupdteItemByID")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spsetAddorupdteItemByID(string status, string Listname, string Comments, string createdby, string itemid, string keyvalue, string Taskid)
        {
            // prepare site connection
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

            try
            {
                //Get the list items from list
                SP.List oList = clientContext.Web.Lists.GetByTitle(Listname);

                SP.ListItem list2 = oList.GetItemById(Int32.Parse(itemid));
                //User user = clientContext.Web.EnsureUser(@"i:0#.w|saathispdt\" + HttpUtility.UrlDecode(createdby));
                //clientContext.Load(user);
                //list2["AssignedTo"] = @"it1";
                //list2["Completed"] = true;
                if (Comments != "")
                    list2["comments"] = Comments;
                if (Taskid != "")
                    list2["currenttaskid"] = Taskid;

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
                ClientContext clientContext = new ClientContext(strSiteURL);
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
                        item => item["arole"],
                         item => item["rrole"],
                          item => item["event"]
                      ));
                clientContext.ExecuteQuery();
                List<pipflowevents> respmsg = new List<pipflowevents>();

                foreach (ListItem oListItem in olists)
                {
                    // create and cast the FieldUserValue from the value
                    /*   FieldUserValue fuvAssignedTo = null;
                     FieldUserValue fuvEditor = null;
                     FieldUserValue fuvAuthor = null;
                     if (oListItem["assignee"] != null)
                           foreach (FieldUserValue userValue in oListItem["assignee"] as FieldUserValue[])
                           {
                               //string test = userValue.LookupId;
                               fuvAssignedTo = userValue;
                           }*/

                    // assigned to for listing the data

                    /*if (oListItem["assignee"] != null)
                        fuvAssignedTo = (FieldUserValue)oListItem["assignee"];
                    if (oListItem["Title"] != null && Eventuser != "")
                        if (fuvAssignedTo.LookupValue.ToLower() != Eventuser.ToLower()) continue;
                 
                    if (oListItem["approver"] != null)
                        fuvEditor = (FieldUserValue)oListItem["approver"];
                    if (oListItem["ekfw"] != null)
                        fuvAuthor = (FieldUserValue)oListItem["ekfw"];
                    */
                    if (oListItem["Title"] != null && Eventuser != "")
                        if (oListItem["Title"].ToString().ToLower() != Eventuser.ToLower()) continue;

                    respmsg.Add(new pipflowevents
                    {
                        id = oListItem.Id.ToString(),
                        title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                        arole = (oListItem["arole"] != null) ? oListItem["arole"].ToString() : "",
                        rrole = (oListItem["rrole"] != null) ? oListItem["rrole"].ToString() : "",
                        flowevent = (oListItem["event"] != null) ? oListItem["event"].ToString() : ""
                        /*,
                        flowevent = (oListItem["event"] != null) ? oListItem["event"].ToString() : "",
                        assigned_to = (oListItem["assignee"] != null) ? fuvAssignedTo.LookupValue : "",
                        assigned_to_id = (oListItem["assignee"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                        approved_to = (oListItem["approver"] != null) ? fuvEditor.LookupValue : "",
                        approved_to_id = (oListItem["approver"] != null) ? fuvEditor.LookupId.ToString() : "",
                        rejected_to = (oListItem["ekfw"] != null) ? fuvAuthor.LookupValue : "",
                        rejected_to_id = (oListItem["ekfw"] != null) ? fuvAuthor.LookupId.ToString() : ""*/

                    });


                }

                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }


        }


        public static Dictionary<string, List<VersionProperty>> GetVersionsPageInfo(string webUrl, ICredentials credentials, Guid listId, int itemId)
        {
            var versionsPageUrl = string.Format("{0}/_layouts/15/versions.aspx?list={1}&ID={2}", webUrl, listId, itemId);
            using (var client = new WebClient())
            {
                client.Credentials = credentials;
                client.Headers.Add("X-FORMS_BASED_AUTH_ACCEPTED", "f");
                var content = client.DownloadString(versionsPageUrl);
                //extract version history info goes here.. 
            }
            return null;
        }
        public class VersionLabel
        {
            public string VersionNo { get; set; }

            public string Modified { get; set; }

            public string ModifiedBy { get; set; }

        }

        public class VersionProperty
        {
            public VersionLabel Label { get; set; }

            public string Value { get; set; }
        }

        public DataSet GetDoucmentHistory(string siteUrl, string listName, int id)
        {
            using (ClientContext ctx = new ClientContext(siteUrl))
            {

                List list = ctx.Site.RootWeb.GetCatalog((int)ListTemplateType.WebPartCatalog);
                CamlQuery query = CamlQuery.CreateAllItemsQuery();
                ListItemCollection items = list.GetItems(query);
                ctx.Load(items);

                ctx.Credentials = new NetworkCredential("spm", "pip@123");
                var file = ctx.Web.Lists.GetByTitle(listName).GetItemById(id).File;
                var versions = file.Versions;
                ctx.Load(file);

                ctx.Load(versions);
                ctx.Load(versions, vs => vs.Include(v => v.CreatedBy));

                ctx.ExecuteQuery();

                var ds = CreatHistoryDataSet();
                foreach (FileVersion fileVersion in versions)
                {
                    var row = ds.Tables[0].NewRow();
                    row["CreatedBy"] = fileVersion.CreatedBy.Title;
                    row["Comments"] = fileVersion.CheckInComment;
                    row["Created"] = fileVersion.Created.ToShortDateString() + " " +
                                     fileVersion.Created.ToShortTimeString();
                    row["Title"] = file.Title;
                    row["VersionLabel"] = fileVersion.VersionLabel;
                    row["IsCurrentVersion"] = fileVersion.IsCurrentVersion;
                    ds.Tables[0].Rows.Add(row);
                }

                return ds;
            }

        }
        private static DataSet CreatHistoryDataSet()
        {
            DataSet ds = new DataSet();
            DataTable table = new DataTable();
            table.Columns.Add("Title");
            table.Columns.Add("Created");
            table.Columns.Add("CreatedBy");
            table.Columns.Add("EncodedAbsUrl");
            table.Columns.Add("VersionLabel");
            table.Columns.Add("Comments");
            table.Columns.Add("IsCurrentVersion");
            ds.Tables.Add(table);
            return ds;
        }
        [Route("api/Pipflow/spgetTaskDetailsByuser")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spgetTaskDetailsByuser(string Listname, string ListitemId, string Vfieldname = "")
        {

            // prepare site connection
            try
            {

                // global parameters
                if (Vfieldname == null) Vfieldname = "";
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<Where><Eq><FieldRef Name='Author' LookupId='True' /><Value Type='User'>123</Value></Eq></Where>";
                //<View><RowLimit>1000</RowLimit></View>
                // prepare site connection
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential("spm", "pip@123");


                Web web = clientContext.Web;
                clientContext.Load(web);


                List list = web.Lists.GetByTitle("Workflow Tasks");


                ListItem targetListItem = list.GetItemById(ListitemId);
                clientContext.Load(list);
                clientContext.Load(targetListItem);
                clientContext.ExecuteQuery();
                // GetDoucmentHistory(pathValue, "Workflow Tasks",int.Parse(ListitemId));
                //GetVersionsPageInfo(pathValue, clientContext.Credentials, list.Id, targetListItem.Id);
                if (Vfieldname == "comments")
                {


                    var file = list.GetItemById(ListitemId).File;
                    var versions = file.Versions;
                    clientContext.Load(file);
                    clientContext.Load(versions);
                    clientContext.Load(versions, vs => vs.Include(v => v.CreatedBy));
                    clientContext.ExecuteQuery();
                    if ((Vfieldname == "comments"))
                    {
                        List<comments> Allcomments = new List<comments>();
                        foreach (FileVersion fileVersion in versions)
                        {
                            Allcomments.Add(new comments
                            {
                                id = fileVersion.ID.ToString(),
                                title = fileVersion.CreatedBy.Title,
                                CheckInComment = fileVersion.CheckInComment,
                                VersionLabel = fileVersion.VersionLabel,
                                IsCurrentVersion = fileVersion.IsCurrentVersion.ToString()

                            });
                        }
                        return getHttpResponseMessage(JsonConvert.SerializeObject(Allcomments));
                    }

                }
                //clientContext.ExecuteQuery();

                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(targetListItem, item => item["Title"],
                         item => item["Title"],
                        item => item["TaskOutcome"],
                        item => item["RelatedItems"],
                        item => item["Status"],
                         item => item["comments"],
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
        public HttpResponseMessage spgetTaskDetails(string Listname, string Taskuser = null, string ReleatedItems = null, string status = "", string TaskType = "")
        {
            // prepare site connection
            try
            {
                if (status == null) status = "";
                // global parameters
                Taskuser = Taskuser == null ? "" : Taskuser;
                ReleatedItems = ReleatedItems == null ? "" : "," + ReleatedItems + ",";

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";
                // camlQuery.ViewXml = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='ParentID'/><Value Type='Counter'>569</Value></Eq></Where></Query></View>", Taskuser);

                //< View Scope = "RecursiveAll" >< Query >< Where >< Eq >< FieldRefName = "ParentID" />< ValueType = "Counter" > 1 </ Value ></ Eq ></ Where ></ Query ></ View >
                // camlQuery.ViewXml = "<View><Where><Eq><FieldRef Name='AssignedTo' /><Value Type='User'>" + Taskuser + "</Value></Eq></Where></View>";
                //camlQuery.ViewXml = "<Where><And><Or><Membership Type='CurrentUserGroups'><FieldRef Name='AssignedTo' /></Membership><Eq><FieldRef Name='AssignedTo'  LookupId='TRUE' /><Value Type='Lookup'>27</Value></Eq></Or><Neq><FieldRef Name='Status' /><Value Type='Text'>Completed</Value></Neq></And></Where>";

                //camlQuery. = "<Where><Or><Eq><FieldRef Name='AssignedTo' LookupId='TRUE'/><Value Type='Integer'><UserID /></Value></Eq><Membership Type='CurrentUserGroups'><FieldRef Name='AssignedTo'/></Membership></Or></Where>";
                // prepare site connection
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

                Web web = clientContext.Web;
                clientContext.Load(web);
                //  var tasks;
                // clientContext.Load(tasks, c => c.Where(t => t.Parent != null && t.Parent.Id == parentId));
                List list = web.Lists.GetByTitle("Workflow Tasks");
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
                        item => item["approveduser"],
                        item => item["areviewuser"],
                        item => item["Editor"],
                        item => item["Author"],
                         item => item["tasktype"],
                          item => item["ParentID"]));
                clientContext.ExecuteQuery();
                List<pipflow> respmsg = new List<pipflow>();

                foreach (ListItem oListItem in olists)
                {





                    // create and cast the FieldUserValue from the value
                    FieldUserValue fuvAssignedTo = null;
                    FieldUserValue fuvapproveduser = null;
                    FieldUserValue fuvareviewuser = null;
                    FieldUserValue fuvEditor = null;
                    FieldUserValue fuvAuthor = null;
                   
                    if (oListItem["AssignedTo"] != null)
                        foreach (FieldUserValue userValue in oListItem["AssignedTo"] as FieldUserValue[])
                        {
                            //string test = userValue.LookupId;
                            fuvAssignedTo = userValue;
                        }
                    if (oListItem["approveduser"] != null)
                        foreach (FieldUserValue userValue in oListItem["approveduser"] as FieldUserValue[])
                        {
                            //string test = userValue.LookupId;
                            fuvapproveduser = userValue;
                        }
                    if (oListItem["areviewuser"] != null)
                        foreach (FieldUserValue userValue in oListItem["areviewuser"] as FieldUserValue[])
                        {
                            //string test = userValue.LookupId;
                            fuvareviewuser = userValue;
                        }
                    // assigned to for listing the data
                    if (oListItem["AssignedTo"] != null && Taskuser != "")
                        if (fuvAssignedTo.LookupValue.ToLower() != Taskuser.ToLower()) continue;

                    if (oListItem["Editor"] != null)
                        fuvEditor = (FieldUserValue)oListItem["Editor"];
                    if (oListItem["Author"] != null)
                        fuvAuthor = (FieldUserValue)oListItem["Author"];
                    string RelItem = "";
                    if (oListItem["RelatedItems"] != null && oListItem["RelatedItems"].ToString() != "")
                    {

                        List<RelatedItemFieldValue> obj = JsonConvert.DeserializeObject<List<RelatedItemFieldValue>>(oListItem["RelatedItems"].ToString());
                        if (obj != null)
                            RelItem = obj[0].ItemId.ToString();
                    }

                    // related item to for listing the data filtering
                    if (ReleatedItems != "" && ReleatedItems.Contains("," + RelItem + ",") != true)
                        continue;
                    if (status != "" && oListItem["Status"].ToString().ToLower() != status.ToLower()) continue;
                    if (TaskType != "" && TaskType != "1")
                    {
                        getSubTasks(ref respmsg, oListItem.Id.ToString(),Taskuser, TaskType);
                        continue;
                    }
                    respmsg.Add(new pipflow
                    {
                        id = oListItem.Id.ToString(),
                        title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                        taskoutcome = (oListItem["TaskOutcome"] != null) ? oListItem["TaskOutcome"].ToString() : "",
                        RelatedItems = (oListItem["RelatedItems"] != null) ? RelItem : "",
                        status = (oListItem["Status"] != null) ? oListItem["Status"].ToString() : "",
                        assigned_to = (oListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupValue : "",
                        assigned_to_id = (oListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                        approveduser_to = (oListItem["approveduser"] != null) ? fuvAssignedTo.LookupValue : "",
                        approveduser_to_id = (oListItem["approveduser"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                        areviewuser_to = (oListItem["areviewuser"] != null) ? fuvAssignedTo.LookupValue : "",
                        areviewuser_to_id = (oListItem["areviewuser"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                        Modified_By = (oListItem["Editor"] != null) ? fuvEditor.LookupValue : "",
                        Modified_By_id = (oListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                        Created_By = (oListItem["Author"] != null) ? fuvAuthor.LookupValue : "",
                        Created_By_id = (oListItem["Author"] != null) ? fuvAuthor.LookupId.ToString() : "",
                        tasktype = (oListItem["tasktype"] != null) ? oListItem["tasktype"].ToString() : ""


                    });



                }

                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }


        }
        private void getSubTasks(ref List<pipflow> respmsg, string _parentID, string Taskuser,string TaskType)
        {
            CamlQuery camlQuery = new CamlQuery();
            //camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";
            camlQuery.ViewXml = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='ParentID'/><Value Type='Counter'>{0}</Value></Eq></Where></Query></View>", _parentID);
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

            Web web = clientContext.Web;
            clientContext.Load(web);
            //  var tasks;
            // clientContext.Load(tasks, c => c.Where(t => t.Parent != null && t.Parent.Id == parentId));
            List list = web.Lists.GetByTitle("Workflow Tasks");
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
                    item => item["approveduser"],
                    item => item["areviewuser"],
                    item => item["Editor"],
                    item => item["Author"],
                     item => item["tasktype"],
                      item => item["ParentID"]));
            clientContext.ExecuteQuery();
            foreach (ListItem oListItem in olists)
            {

                if (oListItem["tasktype"] != null && TaskType != oListItem["tasktype"].ToString()) continue;
                // create and cast the FieldUserValue from the value
                FieldUserValue fuvAssignedTo = null;
                FieldUserValue fuvapproveduser = null;
                FieldUserValue fuvareviewuser = null;
                FieldUserValue fuvEditor = null;
                FieldUserValue fuvAuthor = null;
                FieldLookupValue fuvParentID = null;
                if (oListItem["AssignedTo"] != null)
                    foreach (FieldUserValue userValue in oListItem["AssignedTo"] as FieldUserValue[])
                    {
                        //string test = userValue.LookupId;
                        fuvAssignedTo = userValue;
                    }

                if (oListItem["AssignedTo"] != null && Taskuser != "")
                    if (fuvAssignedTo.LookupValue.ToLower() != Taskuser.ToLower()) continue;

                if (oListItem["approveduser"] != null)
                    foreach (FieldUserValue userValue in oListItem["approveduser"] as FieldUserValue[])
                    {
                        //string test = userValue.LookupId;
                        fuvapproveduser = userValue;
                    }
                if (oListItem["areviewuser"] != null)
                    foreach (FieldUserValue userValue in oListItem["areviewuser"] as FieldUserValue[])
                    {
                        //string test = userValue.LookupId;
                        fuvareviewuser = userValue;
                    }
                // assigned to for listing the data
                if (oListItem["ParentID"] != null)
                    fuvParentID = (FieldLookupValue)oListItem["ParentID"];

                if (oListItem["Editor"] != null)
                    fuvEditor = (FieldUserValue)oListItem["Editor"];
                if (oListItem["Author"] != null)
                    fuvAuthor = (FieldUserValue)oListItem["Author"];
                string RelItem = "";
              
                if (oListItem["RelatedItems"] != null && oListItem["RelatedItems"].ToString() != "")
                {

                    List<RelatedItemFieldValue> obj = JsonConvert.DeserializeObject<List<RelatedItemFieldValue>>(oListItem["RelatedItems"].ToString());
                    if (obj != null)
                        RelItem = obj[0].ItemId.ToString();
                }

                // related item to for listing the data filtering


                respmsg.Add(new pipflow
                {
                    id = oListItem.Id.ToString(),
                    title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                    taskoutcome = (oListItem["TaskOutcome"] != null) ? oListItem["TaskOutcome"].ToString() : "",
                    RelatedItems = (oListItem["RelatedItems"] != null) ? RelItem : "",
                    status = (oListItem["Status"] != null) ? oListItem["Status"].ToString() : "",
                    assigned_to = (oListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupValue : "",
                    assigned_to_id = (oListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                    approveduser_to = (oListItem["approveduser"] != null) ? fuvAssignedTo.LookupValue : "",
                    approveduser_to_id = (oListItem["approveduser"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                    areviewuser_to = (oListItem["areviewuser"] != null) ? fuvAssignedTo.LookupValue : "",
                    areviewuser_to_id = (oListItem["areviewuser"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                    Modified_By = (oListItem["Editor"] != null) ? fuvEditor.LookupValue : "",
                    Modified_By_id = (oListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                    Created_By = (oListItem["Author"] != null) ? fuvAuthor.LookupValue : "",
                    Created_By_id = (oListItem["Author"] != null) ? fuvAuthor.LookupId.ToString() : "",
                    tasktype = (oListItem["tasktype"] != null) ? oListItem["tasktype"].ToString() : "",
                    ParentID = (oListItem["ParentID"] != null) ? fuvParentID.LookupValue.ToString() : ""


                });


            }
        }

        //start comments history version info  below code is for 
        /*
        public static string GetVersions(string siteUrl, string listId, string itemId, string fieldName)
        {
            StringBuilder sb = new StringBuilder();
            Lists listService = new Lists();
            listService.Credentials = System.Net.CredentialCache.DefaultCredentials;
            listService.Url = siteUrl + "/_vti_bin/lists.asmx";

            if (!string.IsNullOrEmpty(fieldName))
            {
                XmlNode nodeVersions = listService.GetVersionCollection(listId, itemId, fieldName);
                foreach (System.Xml.XmlNode xNode in nodeVersions)
                {
                    string dateHistory = xNode.Attributes["Modified"].Value;
                    dateHistory = FormatDateFromSP(dateHistory);
                    string commentHistory = xNode.Attributes[fieldName].Value;
                    string editor = GetEditor(xNode.Attributes["Editor"].Value);
                    sb.Append(editor + " (" + dateHistory + ") " + commentHistory + "\n\n");
                }
            }
            return sb.ToString();
        }
        private static string FormatDateFromSP(string dateHistory)
        {
            string result;

            result = dateHistory.Replace("T", " ");
            result = result.Replace("Z", "");

            return result;
        }
        /// <summary>  
        /// The XmlNode for version on the Editor contains the Editor Name  
        /// </summary>  
        /// <param name="ienumEditor"></param>  
        /// <returns></returns>  
        private static string GetEditor(string nodeValue)
        {
            string[] arr;
            char[] sep =
            {
                '#'
            };
            arr = nodeValue.Split(sep);
            // Grab the second element for the array  
            nodeValue = arr[1];
            // Remove the last comma from the Editor value  
            return nodeValue.Remove(nodeValue.Length - 1);
        }
        */
        //end 


        [System.Web.Http.Route("api/Pipflow/ADAddUser")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage ADAddUser(CreateUser model)
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(strADUserURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonConvert.SerializeObject(model);
                HttpContent content = new StringContent(json, UTF8Encoding.UTF8, "application/json");
                if (ConfigurationManager.AppSettings["AD_USER_URL"] != null)
                    strADUserURL = ConfigurationManager.AppSettings["AD_USER_URL"].ToString();

                string strADUserApiURL = strADUserURL + "/ADAddUser";
                HttpResponseMessage Res = client.PostAsync(strADUserApiURL, content).Result;
                var result = Res.Content.ReadAsStringAsync();
            }
            return getHttpResponseMessage("success");
        }



        // start user active directory calls to servers
        //[System.Web.Http.Route("api/Pipflow/ADAddUser")]
        //[System.Web.Http.HttpPost]
        //public HttpResponseMessage ADAddUser(CreateUser model)
        //{

        //    string response = string.Empty;
        //    response = ClsGeneral.DoWebreqeust(strADUserURL + "/ADAddUser", JsonConvert.SerializeObject(model));
        //    string fullURL = HttpUtility.UrlEncode(strADUserURL + "/ADAddUser", Encoding.UTF8);// strADUserURL + "/ADAddUser";

        //    var httpWebRequest = (HttpWebRequest)WebRequest.Create(fullURL);
        //    httpWebRequest.ContentType = "application/json";
        //    httpWebRequest.Method = "POST";

        //    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
        //    {


        //        streamWriter.Write(JsonConvert.SerializeObject(model));
        //        streamWriter.Flush();
        //        streamWriter.Close();

        //        var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();

        //        using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
        //        {
        //            response = streamReader.ReadToEnd();
        //        }
        //    }

        //    return getHttpResponseMessage(response);

        //    using (var client = new HttpClient())
        //    {
        //        var res = client.PostAsync(strADUserURL + "/ADAddUser",
        //          new StringContent(JsonConvert.SerializeObject(model),
        //            Encoding.UTF8, "application/json")
        //        );

        //        try
        //        {
        //            res.Result.EnsureSuccessStatusCode();
        //        }
        //        catch (Exception e)
        //        {
        //            Console.WriteLine(e.ToString());
        //        }
        //        return getHttpResponseMessage(res.ToString());

        //    }

        //    try

        //    {
        //        using (var client = new HttpClient())
        //        {

        //            client.BaseAddress = new Uri("http://localhost:8081");
        //            // var response1 = client.PostAsJsonAsync("/api/ADUVerify/ADAddUser", model).Result;\
        //            var response1 = client.PostAsJsonAsync("/api/ADUVerify/ADAddUser", new StringContent(JsonConvert.SerializeObject(model).ToString(), Encoding.UTF8, "application/json")).Result;

        //            if (response1.IsSuccessStatusCode)
        //            {
        //                Console.Write("Success");
        //            }
        //            else
        //                Console.Write("Error");
        //        }

        //        using (WebClient client = new WebClient())
        //        {



        //            var dataString = JsonConvert.SerializeObject(model);
        //            var content = new StringContent(dataString, Encoding.UTF8, "application/json");
        //            // var result = (await client.Po(strADUserURL + "/ADAddUser",content)).Result;


        //            // wc.Headers["Content-Type"] = "application/x-www-form-urlencoded";
        //            client.Headers.Add(HttpRequestHeader.ContentType, "application/x-www-form-urlencoded");
        //            response = client.UploadString(new Uri(strADUserURL + "/ADAddUser"), "POST", dataString);
        //            return getHttpResponseMessage(response);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return getErrormessage("faile:" + ex.Message);
        //    }

        //}
        [System.Web.Http.Route("api/Pipflow/getADUsers")]
        [System.Web.Http.HttpGet, System.Web.Http.HttpPost]
        public HttpResponseMessage getADUsers(string OUNAMES)
        {
            //List<CreateUser> userlist;
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(strADUserURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                if (ConfigurationManager.AppSettings["AD_USER_URL"] != null)
                {
                    string strADUserApiURL = ConfigurationManager.AppSettings["AD_USER_URL"].ToString();
                    // New code:
                    strADUserApiURL = strADUserApiURL + "/getADUsers?OUNAMES=" + OUNAMES;
                    HttpResponseMessage response = client.GetAsync(strADUserApiURL).Result;
                    return response;
                    //if (response.IsSuccessStatusCode)
                    //{
                    //    //CreateUser Users = null;
                    //  var userlist = response.Content.ReadAsAsync<List<CreateUser>>();
                    //    return getHttpResponseMessage(JsonConvert.SerializeObject(userlist));
                    //}
                }
            }

            return getErrormessage("success");
        }

        //static async Task<CreateUser> GetProductAsync(string path)
        //{
        //    CreateUser createUser = null;
        //    HttpResponseMessage response = await client.GetAsync(path);
        //    if (response.IsSuccessStatusCode)
        //    {
        //        product = await response.Content.ReadAsAsync<Product>();
        //    }
        //    return product;
        //}






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
        [System.Web.Http.Route("api/Pipflow/ADUpdateUser")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage ADUpdateUser(CreateUser createUser)
        {

            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(strADUserURL);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var json = JsonConvert.SerializeObject(createUser);
                HttpContent content = new StringContent(json, UTF8Encoding.UTF8, "application/json");
                if (ConfigurationManager.AppSettings["AD_USER_URL"] != null)
                    strADUserURL = ConfigurationManager.AppSettings["AD_USER_URL"].ToString();
                string strADUserApiURL = strADUserURL + "/ADUpdateUser";
                //string strADUserApiURL = "http://localhost:60385/api/AduVerify/ADUpdateUser";
                HttpResponseMessage Res = client.PostAsync(strADUserApiURL, content).Result;
                var result = Res.Content.ReadAsStringAsync();
            }
            return getHttpResponseMessage("success");
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

        [System.Web.Http.Route("api/Pipflow/BulkPushAPIS")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage BulkPushAPIS(List<BulkpushAPIS> models)
        {
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

            //Get the list items from list
            SP.List oList = clientContext.Web.Lists.GetByTitle("bulkpushapis");
            ListItemCreationInformation oListItemCreationInformation = new ListItemCreationInformation();


            foreach (BulkpushAPIS BulkAPI in models)
            {
                // prepare site connection               

                try
                {
                    ListItem oItem = oList.AddItem(oListItemCreationInformation);
                    oItem["Title"] = BulkAPI.Title;
                    oItem["pushurl"] = BulkAPI.url;
                    oItem.Update();
                    //clientContext.Load(oItem);
                    clientContext.ExecuteQuery();
                }
                catch (Exception ex)
                {

                    return getErrormessage(ex.Message);
                }

            }
            return getSuccessmessage("Success");
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

