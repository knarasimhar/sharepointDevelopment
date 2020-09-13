using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using System.Diagnostics;
using SP = Microsoft.SharePoint.Client;
using System.Web;
using Newtonsoft.Json.Linq;

namespace BulkPushConsoleApp
{
    class Program
    {


        // GET api/values/5
        static String strSiteURL = "http://sharepoint2/sites/teamsiteex/PipFlowSite", strUSER = "spuser2", strPWD = "User@123#", strADUserURL = "";
        static string SITE_API_URL = "";
        static string strDomainName = ClsGeneral.getConfigvalue("DomainName");
       

        static string cPipflowListName = "pipflow1";
      
        static string cWfHListName = "workflow_history";
        static string stateids = "",FMRStatus="-1" ,wfhstatus="-2" ,FMRSuccessStatus="9", FMRFailStatus = "-9",
                      wfhSuccessstatus = "4", wfhFailsstatus = "-4", CallbackStatus = "5", CallbackFailStatus = "-5";
       // ClientContext BulkclientContext;

        
        static void Main(string[] args)
        {
            if (ClsGeneral.getConfigvalue("SITE_URL") != "")
                strSiteURL = ClsGeneral.getConfigvalue("SITE_URL");
            if (ClsGeneral.getConfigvalue("SITE_API_URL") != "")
                SITE_API_URL = ClsGeneral.getConfigvalue("SITE_API_URL");
            if (ClsGeneral.getConfigvalue("SITE_URL_USER") != "")
                strUSER = ClsGeneral.getConfigvalue("SITE_URL_USER");
            if (ClsGeneral.getConfigvalue("SITE_URL_PWD")!= "")
                strPWD = ClsGeneral.getConfigvalue("SITE_URL_PWD");
            if (ClsGeneral.getConfigvalue("AD_USER_URL") != "")
                strADUserURL = ClsGeneral.getConfigvalue("AD_USER_URL");
            if (ClsGeneral.getConfigvalue("ISSUPLIMENTARY").ToLower() == "y")
            {
                cPipflowListName = "Spipflow1";
                cWfHListName = "Sworkflow_history";

                FMRStatus = "-3"; wfhstatus = "-4"; FMRSuccessStatus = "59" ; FMRFailStatus = "-59";
                wfhSuccessstatus = "54"; wfhFailsstatus = "-54"; CallbackStatus = "55"; CallbackFailStatus = "-55";
            }

            if (ClsGeneral.getConfigvalue("stateids").ToLower() !="")
            {
                stateids = ClsGeneral.getConfigvalue("stateids");

            }

            

            /* ThreadStart thread = new ThreadStart(spgetListItemByID);
             Thread myThread = new Thread(thread);

             myThread.Start();

             for (int y = 0; y < 4; y++)
             {
                 Console.WriteLine(".");
                 Thread.Sleep(1000);
             }

             Console.ReadKey();*/

            spgetListItemByID().GetAwaiter().GetResult();

        }


        static async Task<string> DoWebGetRequest(string url, string data)
        {
            WebRequest request = WebRequest.Create(url + data);

            // request.ContentType = "Plain/text; charset=UTF-8";

            // If required by the server, set the credentials.
            request.Credentials = CredentialCache.DefaultCredentials;
            //request.ContentType = "application/json; charset=UTF-8";
            request.ContentType = "application/json; odata=nometadata";

            WebResponse response = await request.GetResponseAsync();

            // Display the status.
            //Console.WriteLine(((HttpWebResponse)response).StatusDescription);
            // Get the stream containing content returned by the server.
            Stream dataStream = response.GetResponseStream();
            // Open the stream using a StreamReader for easy access.
            StreamReader reader = new StreamReader(dataStream);
            // Read the content.
            string responseFromServer = reader.ReadToEnd();

            reader.Close();
            response.Close();
            return responseFromServer;

        }
        public class BulkpushAPIS
        {
            #region Properties
            public string Id { get; set; }
            public string Title { get; set; }
            public string url { get; set; }

            public string callbackurl { get; set; }

            public string status { get; set; }
            #endregion
        }
     static private void spsetFMRDBBulk(string fmrid, string remarks, string Listname, ref ClientContext clientContext, string AssignedTo = "", string FY = "", string stateid = "", string fmrtype = "", string roleid = "")
        {
            // string createdby, string taskid, string assignevent = "", string AssignedTo = ""
            // prepare site connection

            // global parameters

            // CamlQuery camlQuery = new CamlQuery();
            // camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";

            // prepare site connection


            List oList = clientContext.Web.Lists.GetByTitle(cPipflowListName);

            ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
            ListItem oListItem = oList.AddItem(itemCreateInfo);

            oListItem["Title"] = fmrid;
            oListItem["remarks"] = remarks;
            User uAssignedTo;
            if (AssignedTo != "")
            {
                 uAssignedTo = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(AssignedTo));
                oListItem["ry3a"] = uAssignedTo;
            }
            oListItem["roleid"] = roleid;
            oListItem["FY"] = FY;
            oListItem["stateid"] = stateid;
            if (fmrtype != null && fmrtype != "")
                oListItem["fmrtype"] = fmrtype;
            //oListItem["Body"] = "Hello World!";

            oListItem.Update();

            //clientContext.Load(oListItem);
            clientContext.ExecuteQuery();
            string relatedItem = oListItem.Id.ToString();
            // below code is for add into workflow_history
            // ClientContext clientContext = new ClientContext(strSiteURL);
            // clientContext.Credentials = new NetworkCredential("spm", "pip@123");
              oList = clientContext.Web.Lists.GetByTitle(cWfHListName);

             itemCreateInfo = new ListItemCreationInformation();
             oListItem = oList.AddItem(itemCreateInfo);
            oListItem["Title"] = fmrid;
            oListItem["comments"] = "FMR to ADD Task";
            //oListItem["approveduser"] = _list2History["approveduser"];
            // oListItem["areviewuser"] = _list2History["areviewuser"];
            if (AssignedTo != "")
            {
                uAssignedTo = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(AssignedTo));
                oListItem["Assigned_x0020_To"] = uAssignedTo;
            }
           // oListItem["Assigned_x0020_To"] = uAssignedTo;
            oListItem["roleid"] = roleid;
            oListItem["stateid"] = stateid;
           // oListItem["event"] = _list2History["event"];
            oListItem["relateditem"] = relatedItem;
            oListItem["tasktype"] = "1";
            oListItem["TaskOutcome"] = "";
           
            oListItem["Status"] = "Not Started";
           // oListItem["taskid"] = taskid;

            oListItem.Update();
            clientContext.Load(oListItem);
            //ListItem targetListItem = oList.(ListitemId);

            /*   isWait = true;
               getLatestTaskIDByFMRNO(string.Format(SITE_API_URL + "/api/Pipflow/spgetTaskDetails?listname&taskuser={0}&ReleatedItems={1}&status=not started", "SPM", fmrid));
               */

        }

       static private string spsetTaskItemByID_New(string status, string percentComplete, string Comments, string createdby, string taskid, ref ClientContext clientContext, ref ClientContext PreHistclientContext, ref ClientContext UpdateFMRclientContext, string assignevent = "", string AssignedTo = "", string areviewuserTo = "", string SPFmrID = "", string TASKTYPE = "", string stateid = "",string roleid="")
        {
            // prepare site connection
          //  string strcallbackurl = callbackurl;
           // ClientContext clientContext = new ClientContext(strSiteURL);
           // clientContext.Credentials = new NetworkCredential("spm", "pip@123");
            if (AssignedTo == null) AssignedTo = "";
            if (percentComplete == null) percentComplete = "1";
            if (TASKTYPE == null) TASKTYPE = "1";
            if (TASKTYPE == "1") percentComplete = "0";

            try
            {

                User createuser = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(createdby));
                User assignuser = null;
                clientContext.Load(createuser);

                //Get the list items from list
                SP.List oList = clientContext.Web.Lists.GetByTitle(cWfHListName);
                SP.ListItem list2 = oList.GetItemById(Int32.Parse(taskid));
               
                list2["Status"] = status;
                list2.Update();
                clientContext.Load(list2);
                clientContext.ExecuteQuery();

                /*SP.ListItem list2History = list2;
                //  clientContext.ExecuteQuery();
                if (TASKTYPE == "1")
                    setPreviousTaskHistory(ref list2, SPFmrID, taskid, status,ref PreHistclientContext);
                */
                string _Titel = list2["Title"].ToString();

                // for create the new list in workflow histroy
                ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
                list2 = oList.AddItem(itemCreateInfo);

                status = "Not Started";
                int i = 0;
                FieldUserValue[] userValueCollection;
                // if assignedTo is null its related to sub task only 
                if (AssignedTo != "")
                {
                    userValueCollection = new FieldUserValue[AssignedTo.Split(',').Length];
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
                    // list2["approveduser"] = userValueCollection;
                    list2["Assigned_x0020_To"] = userValueCollection;
                }



                //list2["AssignedTo"] = @"it1";
                //list2["Completed"] = true;
                list2["Title"] = _Titel;
                list2["relateditem"] = SPFmrID;


                if (AssignedTo != "")
                {


                    // list2["AssignedTo"] = createuser;
                    list2["approveduser"] = createuser;
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
                        //list2["PercentComplete"] = percentComplete;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = "";
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = "1";

                    }
                    else
                    {

                        //list2["PercentComplete"] = percentComplete;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = "";
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = "1";
                        //list2["comments"] = Comments;
                    }
                    // list2["Status"] = "Rejected";
                    // list2["TaskOutcome"] = "Rejected"; 

                    //setPreviousTaskHistory(ref list2History);
                    list2["roleid"] = roleid;
                    list2["stateid"] = stateid;
                    list2.Update();
                    clientContext.Load(list2);
                   // clientContext.ExecuteQuery();
                   // disable fmr  update
                    spsetAddorupdteItemByID("", cPipflowListName, "", "", SPFmrID, "", taskid,ref UpdateFMRclientContext, createdby, AssignedTo);

                    // update the previous history to other list workflow_history

                }

                if (areviewuserTo != null && areviewuserTo != "" && (TASKTYPE == "2" || TASKTYPE == "3"))
                {
                    ListItemCreationInformation oListItemCreationInformation = new ListItemCreationInformation();



                /*    var lookupValue = new FieldLookupValue();
                    lookupValue.LookupId = int.Parse(taskid); // Get parent item ID and assign it value in lookupValue.LookupId  
                    var lookupValueCollection = new FieldLookupValue[1];
                    lookupValueCollection.SetValue(lookupValue, 0);*/

                    //FieldUserValue[] AreveiweruserValueCollection = new FieldUserValue[areviewuserTo.Split(',').Length];
                    FieldUserValue[] AreveiweruserValueCollection = new FieldUserValue[1];
                    //for multiple assigies should be send , separate paramers
                    i = 0;
                    foreach (string auser in areviewuserTo.Split(','))
                    {

                        ListItem oItem = oList.AddItem(oListItemCreationInformation);
                        assignuser = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(auser));
                        clientContext.Load(assignuser);
                        clientContext.ExecuteQuery();
                        if (assignuser != null)
                        {

                            FieldUserValue fieldUserVal = new FieldUserValue();
                            fieldUserVal.LookupId = assignuser.Id;
                            AreveiweruserValueCollection.SetValue(fieldUserVal, 0);
                            i++;

                        }

                        oItem["Assigned_x0020_To"] = createuser;
                        // below column is userd to set the aditional review user store
                        oItem["areviewuser"] = AreveiweruserValueCollection;
                        if (TASKTYPE == "2")
                            oItem["Title"] = "Additional Review";
                        else if (TASKTYPE == "3")
                            oItem["Title"] = "ROP";
                        else
                            oItem["Title"] = "sub task";
                       // oItem["ParentID"] = lookupValueCollection; // set chidl item ParentID field  
                        oItem["tasktype"] = TASKTYPE;
                        oItem["relateditem"] = SPFmrID;
                        oItem["Status"] = status;
                        oItem["TaskOutcome"] = "";
                        list2["roleid"] = roleid;
                        list2["stateid"] = stateid;
                        //  oItem["PercentComplete"] = 0;
                        oItem.Update();
                        clientContext.Load(oItem);
                        // clientContext.ExecuteQuery();

                    }


                    //for close current task and assign to next user

                  //  list2["PercentComplete"] = percentComplete;
                    list2["Status"] = status;
                    list2["TaskOutcome"] = "";
                    list2["comments"] = Comments;
                    list2["event"] = assignevent;
                    list2["tasktype"] = TASKTYPE;
                    list2["roleid"] = roleid;
                    list2["stateid"] = stateid;
                    list2.Update();
                    clientContext.Load(list2);
                    // clientContext.ExecuteQuery();


                }
                else if ((TASKTYPE == "2" || TASKTYPE == "3"))
                {

                    /// for closing or update current task id 
                    //list2["PercentComplete"] = percentComplete;
                    list2["Status"] = status;
                    list2["TaskOutcome"] = "";
                    list2["comments"] = Comments;
                    list2["event"] = assignevent;
                    list2["tasktype"] = TASKTYPE;
                    list2["roleid"] = roleid;
                    list2["stateid"] = stateid;
                    list2.Update();
                    clientContext.Load(list2);
                    // clientContext.ExecuteQuery();

                }





                // updated latest task id to FMR list for viewing
                //http://52.172.200.35:2020/sppipapidevtesting/api/Pipflow/spgetTaskDetails?listname&taskuser=&ReleatedItems=82&status=not started
                /*
                    if (SPFmrID != null && SPFmrID != "" && AssignedTo != null && AssignedTo != "")
                    {
                        isWait = true;
                        getLatestTaskIDByFMRNO(string.Format(SITE_API_URL + "/api/Pipflow/spgetTaskDetails?listname&taskuser={0}&ReleatedItems={1}&status=not started", AssignedTo, SPFmrID));
                    }
                    */
              /*  if (strcallbackurl != null && strcallbackurl != "")
                {
                   // string strResp = ClsGeneral.DoWebGetRequest(strcallbackurl.Replace("~", "&"), "");
                }*/
                //end of the 
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
            return "Success";

        }
       static private void setPreviousTaskHistory(ref SP.ListItem _list2History, string SPFmrID, string taskid, string Status, ref ClientContext clientContext)
        {
           // ClientContext clientContext = new ClientContext(strSiteURL);
           // clientContext.Credentials = new NetworkCredential("spm", "pip@123");
            List oList = clientContext.Web.Lists.GetByTitle("workflow_history");

            ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
            ListItem oListItem = oList.AddItem(itemCreateInfo);
            oListItem["Title"] = _list2History["Title"];
            oListItem["comments"] = _list2History["comments"];
            oListItem["approveduser"] = _list2History["approveduser"];
            oListItem["areviewuser"] = _list2History["areviewuser"];
            oListItem["Assigned_x0020_To"] = _list2History["Assigned_x0020_To"];
            oListItem["roleid"] = _list2History["roleid"];
            oListItem["stateid"] = _list2History["stateid"];
            oListItem["event"] = _list2History["event"];
            oListItem["relateditem"] = SPFmrID;
            oListItem["tasktype"] = _list2History["tasktype"];
            if (_list2History["TaskOutcome"] == null)
            {
                oListItem["TaskOutcome"] = 1;
            }
            else
            {
                oListItem["TaskOutcome"] = _list2History["TaskOutcome"];
            }
            oListItem["Status"] = Status;
            oListItem["taskid"] = taskid;

            oListItem.Update();
            clientContext.Load(oListItem);

        }

        static private string spsetAddorupdteItemByID(string status, string Listname, string Comments, string createdby, string itemid, string keyvalue, string Taskid, ref ClientContext clientContext, string AssignTo = "", string CurAssignTo = "")
        {
            // prepare site connection
           // ClientContext clientContext = new ClientContext(strSiteURL);
           // clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

            try
            {
                //Get the list items from list
                SP.List oList = clientContext.Web.Lists.GetByTitle(Listname);

                SP.ListItem list2 = oList.GetItemById(Int32.Parse(itemid));
                //User user = clientContext.Web.EnsureUser(@"i:0#.w|saathispdt\" + HttpUtility.UrlDecode(createdby));
                //clientContext.Load(user);
                //list2["AssignedTo"] = @"it1";
                //list2["Completed"] = true;
                if (AssignTo == null) AssignTo = "";

                if (CurAssignTo == null) CurAssignTo = "";

                if (CurAssignTo != "")
                {

                    int i = 0;
                    FieldUserValue[] userValueCollection;
                    // if assignedTo is null its related to sub task only 
                    if (CurAssignTo != "")
                    {
                        userValueCollection = new FieldUserValue[CurAssignTo.Split(',').Length];
                        //for multiple assigies should be send , separate paramers
                        User assignuser;
                        foreach (string auser in CurAssignTo.Split(','))
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
                        // list2["approveduser"] = userValueCollection;

                        list2["currentAssignee"] = userValueCollection;

                        list2.Update();
                        clientContext.Load(list2);
                        //clientContext.ExecuteQuery();
                    }

                }

                if (AssignTo != "")
                {


                    User uAssignedTo = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(AssignTo));
                    clientContext.Load(uAssignedTo);
                    clientContext.ExecuteQuery();
                    if (uAssignedTo != null)
                    {
                        FieldUserValue fieldUserVal = new FieldUserValue();
                        fieldUserVal.LookupId = uAssignedTo.Id;
                        //fieldUserVal.LookupValue = assignuser.LoginName;

                        //userValueCollection.SetValue(fieldUserVal, i);
                        list2["ry3a"] = fieldUserVal;

                    }

                }
                /* if (CurAssignTo != "")
                 {


                     User uCurrAssignedTo = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(CurAssignTo));
                     clientContext.Load(uCurrAssignedTo);
                     clientContext.ExecuteQuery();
                     if (uCurrAssignedTo != null)
                     {
                         FieldUserValue fieldUserVal2 = new FieldUserValue();
                         fieldUserVal2.LookupId = uCurrAssignedTo.Id;
                         //fieldUserVal.LookupValue = assignuser.LoginName;

                         //userValueCollection.SetValue(fieldUserVal, i);
                         list2["currentAssignee"] = fieldUserVal2;

                     }



                 }*/
                if (Comments != "")
                    list2["comments"] = Comments;
                if (Taskid != "")
                    list2["currenttaskid"] = Taskid;

                // list2["Status"] = "Rejected";
                // list2["TaskOutcome"] = "Rejected";
                list2.Update();
                clientContext.Load(list2);
                //clientContext.ExecuteQuery();

                // below list is used for store the previours history

            }
            catch (Exception ex)
            {

                return ex.Message;
            }
            return "Success";

        }
        static async Task spgetListItemByID()
        {
            List<BulkpushAPIS> respmsg = new List<BulkpushAPIS>();
            string roleid = "0";
            string stateid = "0";
            // prepare site connection
            try
            {
                // global parameters
               

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><RowLimit>50000</RowLimit></View>";
                //camlQuery.ViewXml = "<View><Where><Eq><FieldRef Name='ry3a' /><Value Type='User'>SPM</Value></Eq></Where></View>";
                // prepare site connection
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);
                clientContext.RequestTimeout = int.MaxValue;

                ClientContext BulkclientContext = new ClientContext(strSiteURL);
                BulkclientContext.Credentials = new NetworkCredential(strUSER, strPWD);
                BulkclientContext.RequestTimeout = int.MaxValue;

                // for update fmr , workflow and workflow history ref PreHistBulkclientContext, ref UPdateFMRBulkclientContext
                ClientContext PreHistBulkclientContext = new ClientContext(strSiteURL);
                PreHistBulkclientContext.Credentials = new NetworkCredential(strUSER, strPWD);
                PreHistBulkclientContext.RequestTimeout = int.MaxValue;

                ClientContext UPdateFMRBulkclientContext = new ClientContext(strSiteURL);
                UPdateFMRBulkclientContext.Credentials = new NetworkCredential(strUSER, strPWD);
                UPdateFMRBulkclientContext.RequestTimeout = int.MaxValue;

                int QueueLength = 10;
                if (ClsGeneral.getConfigvalue("REQUEST_QUEUESIZE") != "")
                {
                    QueueLength = int.Parse(ClsGeneral.getConfigvalue("REQUEST_QUEUESIZE"));
                }
                Web web = clientContext.Web;
                clientContext.Load(web);
                List list = web.Lists.GetByTitle("bulkpushapis");
                ListItemCollection olists = list.GetItems(camlQuery);
                // ListItem targetListItem = list.GetItemById(ListitemId);
                //clientContext.ExecuteQuery();

                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(olists,
                     items => items.Include(
                        item => item.Id,
                        item => item["Title"],
                         item => item["status"],
                        item => item["callbackurl"],
                         item => item["pushurl"]));
                clientContext.ExecuteQuery();


                foreach (ListItem oListItem in olists)
                {
                    respmsg.Add(new BulkpushAPIS
                    {
                        Id = oListItem.Id.ToString(),
                        Title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                        callbackurl = (oListItem["callbackurl"] != null) ? oListItem["callbackurl"].ToString() : "",
                        url = (oListItem["pushurl"] != null) ? oListItem["pushurl"].ToString() : "",
                        status = (oListItem["status"] != null) ? oListItem["status"].ToString() : "0"

                    });
                }


                string respOut = "";

                // /*  for insert */ for instert from other instance
                /*  clientContext = new ClientContext("http://sharepoint2/sites/teamsiteex/pipflowsitetesting");
                   web = clientContext.Web;
                  clientContext.Load(web);

                  List oList = clientContext.Web.Lists.GetByTitle("bulkpushapis");
                  ListItemCreationInformation oListItemCreationInformation = new ListItemCreationInformation();
                  clientContext.ExecuteQuery(); */
                int i = 1;
                int j = 1;
                Stopwatch stopwatch1 = new Stopwatch();
                stopwatch1.Start();
                Stopwatch stopwatch2 = new Stopwatch();
                stopwatch2.Start();
                dynamic QueryParams, QueryParam;
             
                foreach (BulkpushAPIS resp in respmsg)
                {

                    //-1 For new FMR creating 
                    if (resp.status == FMRStatus)
                    {
                        try

                        {
                            Console.WriteLine("ID:" + resp.Id + " Bulk push veriffication:" + i.ToString());
                            ListItem targetListItem = list.GetItemById(resp.Id);
                            //  targetListItem["pushurl"] = resp.url.ToString().Replace("sppipapitestlocal", "sppipapitesting");
                             targetListItem["status"] = FMRSuccessStatus;
                            //targetListItem["Title"] = resp.url;
                            targetListItem["log"] = "Bulk push veriffication :" + i.ToString();
                            var uri = new Uri(resp.url);
                            var query = HttpUtility.ParseQueryString(uri.Query);
                            if (resp.url.ToLower().Contains("/spsetfmr?"))
                            {
                                 QueryParams = JArray.Parse(ClsGeneral.GetJsonStringFromQueryString(query.ToString().ToLower()));

                                 QueryParam = QueryParams[0];

                                 if (QueryParam.stateid != null) stateid = QueryParam.stateid.Value;

                                if (stateids!="" && !stateid.Contains(stateids)) continue;
                                if (QueryParam.roleid != null) roleid = QueryParam.roleid.Value;

                                try
                                {
                                    spsetFMRDBBulk(QueryParam.fmrid.Value, QueryParam.remarks.Value, "", ref BulkclientContext, QueryParam.assignedto.Value, QueryParam.fy.Value, stateid, QueryParam.fmrtype.Value, roleid);
                                }
                                catch(Exception ex) { targetListItem["status"] = FMRFailStatus; targetListItem["log"] = ex.Message; }
                                    // oItem["pushurl"] = BulkAPI.callbackurl; 
                                targetListItem.Update();
                            clientContext.Load(targetListItem);
                                i++;
                            }
                          
                        }
                        catch
                        { }
                    
                    if (i % QueueLength == 0)
                    {


                            // Create new stopwatch.
                            // Stopwatch stopwatch = new Stopwatch();
                            // Create new stopwatch.
                            Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();
                        BulkclientContext.ExecuteQuery();
                        // Stop timing.
                       // stopwatch.Stop();

                        // Write result.
                      //  Console.WriteLine(" BulkclientContext.ExecuteQuery() Time elapsed: {0}", stopwatch.Elapsed);
                      //  stopwatch.Start();
                        BulkclientContext.Dispose();
                      //  stopwatch.Stop();

                        // Write result.
                      //  Console.WriteLine(" BulkclientContext.Dispose() Time elapsed: {0}", stopwatch.Elapsed);

                           
                           // stopwatch.Start();
                            clientContext.ExecuteQuery();
                            // Stop timing.
                           // stopwatch.Stop();

                            // Write result.
                          //  Console.WriteLine(" clientContext.ExecuteQuery() Time elapsed: {0}", stopwatch.Elapsed);
                          //  stopwatch.Start();
                            clientContext.Dispose();
                            stopwatch.Stop();
                            Console.WriteLine("All requests Time elapsed: {0}", stopwatch.Elapsed);
                            Console.WriteLine(" BUlk push to List item from " + (i-QueueLength+1).ToString() + "  To " + i.ToString());
                    }
                    }
                   
                    // 0 for Single task creation next level
                    else if (resp.status == wfhstatus)
                    {
                        try

                        {
                            Console.WriteLine("ID:" + resp.Id + " Bulk push veriffication:" + j.ToString());
                            ListItem targetListItem = list.GetItemById(resp.Id);
                            //  targetListItem["pushurl"] = resp.url.ToString().Replace("sppipapitestlocal", "sppipapitesting");
                            targetListItem["status"] = CallbackStatus;
                            //targetListItem["Title"] = resp.url;
                            targetListItem["log"] = "Bulk push veriffication :" + j.ToString();
                            var uri = new Uri(resp.url);
                            var query = HttpUtility.ParseQueryString(uri.Query);
                           if (resp.url.ToLower().Contains("/spsettaskitembyid?"))
                            {
                                QueryParams = JArray.Parse(ClsGeneral.GetJsonStringFromQueryString(query.ToString().ToLower()));

                                QueryParam = QueryParams[0];

                                if (QueryParam.stateid != null) stateid = QueryParam.stateid.Value;
                                if (QueryParam.roleid != null) roleid = QueryParam.roleid.Value;

                                try
                                {
                                    spsetTaskItemByID_New(QueryParam.status.Value, QueryParam.percentcomplete.Value, QueryParam.comments.Value, QueryParam.createdby.Value, QueryParam.taskid.Value, ref BulkclientContext, ref PreHistBulkclientContext, ref UPdateFMRBulkclientContext, QueryParam.assignevent.Value, QueryParam.assignedto.Value, QueryParam.areviewuserto.Value, QueryParam.spfmrid.Value, QueryParam.tasktype.Value, stateid, roleid);
                                }
                                catch (Exception ex) { targetListItem["status"] = wfhFailsstatus; targetListItem["log"] = ex.Message; }
                                //oItem["pushurl"] = resp.callbackurl; 
                                targetListItem.Update();
                            clientContext.Load(targetListItem);
                                j++;
                            }

                          


                        }
                        catch { }

                        if (j % QueueLength == 0)
                        {
                           

                            // Create new stopwatch.
                             Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();
                            BulkclientContext.ExecuteQuery();
                            // Stop timing.
                            stopwatch.Stop();

                            // Write result.
                            Console.WriteLine(" BulkclientContext.ExecuteQuery() Time elapsed: {0}", stopwatch.Elapsed);
                            stopwatch.Start();
                            BulkclientContext.Dispose();
                            stopwatch.Stop();

                            // Write result.
                            Console.WriteLine(" BulkclientContext.Dispose() Time elapsed: {0}", stopwatch.Elapsed);

                            // Create new stopwatch.
                            // Stopwatch stopwatch = new Stopwatch(); PreHistBulkclientContext, ref UPdateFMRBulkclientContext
                            stopwatch.Start();
                            PreHistBulkclientContext.ExecuteQuery();
                            // Stop timing.
                            stopwatch.Stop();

                            // Write result.
                            Console.WriteLine(" PreHistBulkclientContext.ExecuteQuery() Time elapsed: {0}", stopwatch.Elapsed);
                            stopwatch.Start();
                            PreHistBulkclientContext.Dispose();
                            stopwatch.Stop();

                            // Write result.
                            Console.WriteLine(" PreHistBulkclientContext.Dispose() Time elapsed: {0}", stopwatch.Elapsed);


                            // Create new stopwatch.
                            // Stopwatch stopwatch = new Stopwatch();
                            stopwatch.Start();
                            UPdateFMRBulkclientContext.ExecuteQuery();
                            // Stop timing.
                            stopwatch.Stop();

                            // Write result.
                            Console.WriteLine(" UPdateFMRBulkclientContext.ExecuteQuery() Time elapsed: {0}", stopwatch.Elapsed);
                            stopwatch.Start();
                            UPdateFMRBulkclientContext.Dispose();
                            stopwatch.Stop();

                            // Write result.
                            Console.WriteLine(" UPdateFMRBulkclientContext.Dispose() Time elapsed: {0}", stopwatch.Elapsed);


                            stopwatch.Start();
                            clientContext.ExecuteQuery();
                            // Stop timing.
                            stopwatch.Stop();

                            // Write result.
                            Console.WriteLine(" clientContext.ExecuteQuery() Time elapsed: {0}", stopwatch.Elapsed);
                            stopwatch.Start();
                            clientContext.Dispose();
                            stopwatch.Stop();

                            

                            Console.WriteLine("BUlk push to List item from " + (j - QueueLength + 1).ToString() + "  To " + j.ToString());
                        }

                        // last update if less than 20 fmrs

                        // Create new stopwatch.
                       
                        BulkclientContext.ExecuteQuery();
                      
                        BulkclientContext.Dispose();
                      
                        PreHistBulkclientContext.ExecuteQuery();
                       
                        PreHistBulkclientContext.Dispose();
                        
                        UPdateFMRBulkclientContext.ExecuteQuery();
                       
                        UPdateFMRBulkclientContext.Dispose();
                        
                        clientContext.ExecuteQuery();
                        // Stop timing.
                       
                        clientContext.Dispose();
                       
                        stopwatch2.Stop();
                            // Write result.
                        Console.WriteLine("Final total clientContext.ExecuteQuery(); Time elapsed: {0}", stopwatch2.Elapsed);
                       // Console.ReadLine();
                    }
                    // for callback url push from 6 to data
                    else if (resp.status == CallbackStatus)
                    {
                        ListItem targetListItem = list.GetItemById(resp.Id);
                        try

                        {
                            Console.WriteLine("ID:" + resp.Id + " Call back request ");

                            //  targetListItem["pushurl"] = resp.url.ToString().Replace("sppipapitestlocal", "sppipapitesting");
                            targetListItem["status"] = wfhSuccessstatus;
                            //targetListItem["Title"] = resp.url;
                            targetListItem["log"] = "Bulk push veriffication from status 6 :";
                            var uri = new Uri(resp.url);
                            var query = HttpUtility.ParseQueryString(uri.Query);
                            if (resp.url.ToLower().Contains("/spsettaskitembyid?"))
                            {
                                QueryParams = JArray.Parse(ClsGeneral.GetJsonStringFromQueryString(query.ToString().ToLower()));

                                QueryParam = QueryParams[0];

                                try
                                {
                                    if (QueryParam.callbackurl.Value != null && QueryParam.callbackurl.Value != "")
                                    {
                                        Stopwatch stopwatch = new Stopwatch();
                                        stopwatch.Start();
                                        Console.WriteLine(" Call back request URL :" + QueryParam.callbackurl.Value);
                                        DoWebGetRequest(QueryParam.callbackurl.Value, "");
                                        stopwatch.Stop();
                                        Console.WriteLine(" DoWebGetRequest Time elapsed: {0}", stopwatch.Elapsed);
                                    }
                                }
                                catch (Exception ex)
                                {
                                    targetListItem["log"] = ex.Message;
                                    targetListItem["status"] = CallbackFailStatus;
                                }
                                //spsetFMRDBBulk(QueryParam.fmrid.Value, QueryParam.remarks.Value, "", ref BulkclientContext, QueryParam.assignedto.Value, QueryParam.fy.Value, QueryParam.stateid.Value, QueryParam.fmrtype.Value, "");
                                // oItem["pushurl"] = BulkAPI.callbackurl; 
                                targetListItem.Update();
                                //clientContext.Load(targetListItem);
                                clientContext.ExecuteQuery();
                                continue;
                            }

                        }
                        catch { }


                    }
                    continue;

                    // break;
                    if (resp.status == "0")
                    {
                        try
                        {
                            respOut = await DoWebGetRequest(resp.url, "");
                            if (respOut.ToLower().Contains("success"))
                            {
                                Console.WriteLine("ID:" + resp.Id + "success:" + respOut);
                                ListItem targetListItem = list.GetItemById(resp.Id);
                                targetListItem["status"] = "1";
                                targetListItem["log"] = "console push ok";
                                targetListItem.Update();
                                clientContext.ExecuteQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("ID:" + resp.Id + "Fail:" + respOut);
                            ListItem targetListItem = list.GetItemById(resp.Id);
                            targetListItem["status"] = "2";
                            targetListItem["log"] = "console push Error:" + ex.Message;
                            targetListItem.Update();
                            clientContext.ExecuteQuery();
                        }
                        // break;
                    }
                    //for reporcess
                    /*

                   if (resp.status == "1")
                         {


                             // for Insert hide
                             /*
                             ListItem oItem = oList.AddItem(oListItemCreationInformation);

                             oItem["Title"] = resp.Title;
                             oItem["pushurl"] = resp.url;
                             oItem["callbackurl"] = resp.callbackurl;
                             oItem["log"] = "console Insert ok";
                             oItem.Update();
                             //clientContext.Load(oItem);
                             clientContext.ExecuteQuery();
                             */
                    // for update hide
                    /*
                    respOut = "success"; //await DoWebGetRequest(resp.url, "");
                    if (respOut.ToLower().Contains("success"))
                    {

                        //sppipapidev/api/Pipflow/spsetfmr?
                        Console.WriteLine("ID:" + resp.Id + "retray:" + respOut);
                        ListItem targetListItem = list.GetItemById(resp.Id);
                        targetListItem["status"] = "0";
                        targetListItem["pushurl"] = resp.url.Replace("sppipapidev", "sppipapidevtesting");
                        targetListItem["log"] = "console push retray";
                        targetListItem.Update();
                        clientContext.ExecuteQuery();
                    }

                    // break;
                }
                //break;
            */
                }
                // Stopwatch stopwatch1 = new Stopwatch();
              

                BulkclientContext.ExecuteQuery();
                BulkclientContext.Dispose();

                PreHistBulkclientContext.ExecuteQuery();
                PreHistBulkclientContext.Dispose();

                clientContext.ExecuteQuery();
                clientContext.Dispose();

                stopwatch1.Stop();

               
               

                // Write result.
                Console.WriteLine("Last  clientContext.ExecuteQuery(); Time elapsed: {0}", stopwatch1.Elapsed);
               // Console.ReadLine();
            }
            catch (Exception ex)
            {


                Console.WriteLine("Error:" + ex.Message);
              //  Console.ReadLine();
            }
        }
    }
}
