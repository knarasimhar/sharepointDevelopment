using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using System.Web;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net;
using System.IO;
using System.Collections;

namespace CopyListItemsSsom
{
    class Program
    {

        static String strSiteURL = "http://sharepoint2/sites/teamsiteex/PipFlowSite", strDestSiteURL = "", strUSER = "spuser2", strPWD = "User@123#", strPUSHType = "addfmr";
        static string strDomainName = ClsGeneral.getConfigvalue("DomainName");
        static SPSite destsite;
        static SPWeb destweb;
        static SPList sourceList, destList, pipflow1;

        static Guid workflowhistoryGuid, pipflow1Guid, bulkpushapisGuid;
        static string stateids = "", FMRStatus = "-1", wfhstatus = "-2", FMRSuccessStatus = "9", FMRFailStatus = "7",
                    wfhSuccessstatus = "4", wfhFailsstatus = "-4", CallbackStatus = "-5", DirectCallUrlStatus = "-6", CallbackFailStatus = "5";
        static string cPipflowListName = "pipflow1";
        static string stateid = "0", roleid = "0";
        static string cWfHListName = "workflow_history", bulkpushlistname = "bulkpushapis", url = "", ItemID = "", ItemStatus = "";
        static StringBuilder SBquery = new StringBuilder();
        static Hashtable _userstable = new Hashtable();
        static void Main(string[] args)
        {

         //  Thread.Sleep(120000);
            if (getConfigvalue("SITE_URL") != "")
                strSiteURL = getConfigvalue("SITE_URL");
            if (getConfigvalue("DEST_SITE_URL") != "")
                strDestSiteURL = getConfigvalue("DEST_SITE_URL");
            if (getConfigvalue("SITE_URL_USER") != "")
                strUSER = getConfigvalue("SITE_URL_USER");
            if (getConfigvalue("SITE_URL_PWD") != "")
                strPWD = getConfigvalue("SITE_URL_PWD");
            if (getConfigvalue("sourceListname") != "")
                bulkpushlistname = getConfigvalue("sourceListname");
            if (getConfigvalue("destinationListname") != "")
                cWfHListName = getConfigvalue("destinationListname");
            if (getConfigvalue("ISSUPLIMENTARY").ToLower() == "y")
            {
                cPipflowListName = "Spipflow1";
                cWfHListName = "S" + getConfigvalue("destinationListname");

                FMRStatus = "-3"; wfhstatus = "-4"; FMRSuccessStatus = "59"; FMRFailStatus = "57";
                wfhSuccessstatus = "54"; wfhFailsstatus = "-54"; CallbackStatus = "-55"; CallbackFailStatus = "55";
            }
            string filePath = getConfigvalue("statefind");
            // below threding used for every 5 seconds process going on not wait for 1 minute
            /* new Thread(new ThreadStart(() => {
                 for (int x = 0; x < 8; x++)
                 {*/

            using (SPSite site = new SPSite(strSiteURL))
            {


                using (SPWeb web = site.OpenWeb())
                {

                    if (args.Length == 0)
                        for (int i = 1; i <= 39; i++)
                             CopyItemsFromOneListToAnotherList(web, i.ToString());
                       
                    else if (args[0] != null && args.Length == 1)
                    // parllel process state wise 
                    {
                               string fileName = filePath +  @"\STATE_" + args[0];
                                System.IO.File.Create(fileName + ".started").Dispose();
                                CopyItemsFromOneListToAnotherList(web, args[0]);
                                System.IO.File.Delete(fileName + ".started");
   
                    }
                    else if (args[0].ToLower() == "addfmr")
                    {
                                //    for (int i = 1; i <= 39; i++)
                                //       AddFMRCopyItems(web, args[1], "", i.ToString());
                                AddFMRCopyItems(web, args[1], "", "7");
                            }
                    else if (args[0].ToLower() == "nextlevel")
                    {
                        strPUSHType = "nextlevel";
                                //  for (int i = 1; i <= 39; i++)
                                //   CopyItems(web, args[1], args[2], i.ToString());
                                CopyItems(web, args[1], args[2], "7");
                            }


                    // CopyItems(web, "Contacts", "Contacts");
                }
            }
                 /*   Console.WriteLine("X => {0}", x);
                    Thread.Sleep(5000);
                }
            })).Start();*/
        }
        public static void CopyItemsFromOneListToAnotherList(SPWeb web, string stateid)
        {
            try
            {

                Console.WriteLine(" Processing state is   " + stateid + " From List " + getConfigvalue("sourceListname") + " To List  " + getConfigvalue("destinationListname"));
                destsite = new SPSite(strDestSiteURL);
                destweb = destsite.OpenWeb();
                sourceList = web.Lists[bulkpushlistname];
                
               /* DateTime Listdt = sourceList.LastItemModifiedDate;
                TimeSpan t =  DateTime.Now.Subtract(Listdt);
               if (t.TotalSeconds > 30) { Console.WriteLine("NO bulkpush records... "); return; }*/
                destList = destweb.Lists[cWfHListName];
                pipflow1 = destweb.Lists[cPipflowListName];
                string[] strFMRStatuses = { "-6","-5", "-55", "-2", "-1", "-4", "-3"};
                //oQuery.ViewXml = ("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + sateid + "</Value></Eq></Where></Query></View>");
                foreach (string strFmrstatus in strFMRStatuses)
                {
                    SPQuery SPquery = new SPQuery();
                     if (strFmrstatus == "-1")
                        SPquery.Query = string.Format("<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>{0}</Value></Eq><Eq><FieldRef Name='status'/><Value Type='Number'>-1</Value></Eq></And></Where>", stateid, strFmrstatus);
                    else if (strFmrstatus == "-2")
                        SPquery.Query = "<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + stateid + "</Value></Eq><Eq><FieldRef Name='status'/><Value Type='Number'>" + strFmrstatus + "</Value></Eq></And></Where>";
                    //  SPquery.Query = "<Where><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + stateid + "</Value></Eq></Where>";// <Eq><FieldRef Name='status'/><Value Type='Number'>-2</Value></Eq></And></Where>";

                    else if (strFmrstatus == "-3")
                        SPquery.Query = string.Format("<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>{0}</Value></Eq><Eq><FieldRef Name='status'/><Value Type='Number'>-3</Value></Eq></And></Where>", stateid, strFmrstatus);

                    else if (strFmrstatus == "-4")
                        SPquery.Query = string.Format("<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>{0}</Value></Eq><Eq><FieldRef Name='status'/><Value Type='Number'>-4</Value></Eq></And></Where>", stateid, strFmrstatus);

                    else if (strFmrstatus == "-5")
                        SPquery.Query = string.Format("<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>{0}</Value></Eq><Eq><FieldRef Name='status'/><Value Type='Number'>-5</Value></Eq></And></Where>", stateid, strFmrstatus);

                    else if (strFmrstatus == "-6")
                        SPquery.Query = string.Format("<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>{0}</Value></Eq><Eq><FieldRef Name='status'/><Value Type='Number'>-6</Value></Eq></And></Where>", stateid, strFmrstatus);


                    else if (strFmrstatus == "-55")
                        SPquery.Query = string.Format("<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>{0}</Value></Eq><Eq><FieldRef Name='status'/><Value Type='Number'>-55</Value></Eq></And></Where>", stateid, strFmrstatus);
                    
                   // SPquery.Query = "<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + stateid + "</Value></Eq><Eq><FieldRef Name='status'/><Value Type='Number'>" + strFmrstatus1 + "</Value></Eq></And></Where>";
                   // SPquery.RowLimit = 2000;
                    int i = 1, j = 1, k = 1, ii = 1;
                    destweb.AllowUnsafeUpdates = true;
                    workflowhistoryGuid = destList.ID; pipflow1Guid = pipflow1.ID; bulkpushapisGuid = sourceList.ID;


                    string strColumnFiels = "", strQuerry = "";
                     SBquery.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?><Batch>");

                    strColumnFiels = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Batch onError=\"Return\">!CONTENT!</Batch>";

                  //  SPquery.Query = "<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + stateid + "</Value></Eq><Eq><FieldRef Name='status'/><Value Type='Number'>" + strFmrstatus1 + "</Value></Eq></And></Where>";

                    SPListItemCollection SPLists = sourceList.GetItems(SPquery);
                    Console.WriteLine(" Total list count for " + SPLists.Count.ToString());
                    string _destListname = getConfigvalue("destinationListname");

                    SBquery.Clear();
                    strQuerry = "";
                    // string test = "SomeValue";


                    dynamic QueryParams, QueryParam;
                    foreach (SPListItem item in SPLists)
                    {
                        ItemID = item["ID"].ToString();
                        ItemStatus = item["status"].ToString();
                        if (item["pushurl"] != null)
                            url = item["pushurl"].ToString();
                        if (ItemStatus == FMRStatus)
                        {

                            try

                            {
                                Console.WriteLine("SNO is " + i + " STATUS " + strFmrstatus + " ID:" + ItemID + " Bulk push ADDFMR veriffication: " + i.ToString() + " stateid:" + stateid);

                                var uri = new Uri(url);
                                var query = HttpUtility.ParseQueryString(uri.Query);
                                if (url.ToLower().Contains("/spsetfmr?"))
                                {
                                    QueryParams = JArray.Parse(ClsGeneral.GetJsonStringFromQueryString(query.ToString().ToLower()));

                                    QueryParam = QueryParams[0];

                                    if (QueryParam.stateid != null) stateid = QueryParam.stateid.Value;

                                    if (stateids != "" && !stateids.Contains(stateid)) continue;
                                    // if (stateid == "39") cWfHListName = cWfHListName + "_39"; ;
                                    if (QueryParam.roleid != null) roleid = QueryParam.roleid.Value;



                                    try
                                    {
                                        Stopwatch stopwatch = new Stopwatch();
                                        stopwatch.Start();
                                        spsetFMRDBBulk(QueryParam.fmrid.Value, QueryParam.remarks.Value, QueryParam.assignedto.Value, QueryParam.fy.Value, stateid, QueryParam.fmrtype.Value, roleid);
                                        stopwatch.Stop();
                                        Console.WriteLine(" spsetFMRDBBulk Time elapsed: {0}", stopwatch.Elapsed);
                                    }
                                    catch (Exception ex) { }
                                    // oItem["pushurl"] = BulkAPI.callbackurl; 

                                    i++;
                                }

                            }
                            catch
                            { }
                        }
                        else if (ItemStatus == wfhstatus)
                        {
                            try

                            {
                                Console.WriteLine("SNO is " + j + " STATUS " + strFmrstatus + " ID:" + ItemID + " Bulk push NEXT Tasks veriffication: " + j.ToString() + " stateid:" + stateid);

                                var uri = new Uri(url);
                                var query = HttpUtility.ParseQueryString(uri.Query);
                                if (url.ToLower().Contains("/spsettaskitembyid?"))
                                {
                                    QueryParams = JArray.Parse(ClsGeneral.GetJsonStringFromQueryString(query.ToString().ToLower()));

                                    QueryParam = QueryParams[0];

                                    if (QueryParam.stateid != null) stateid = QueryParam.stateid.Value;

                                    if (QueryParam.roleid != null) roleid = QueryParam.roleid.Value;
                                    if (QueryParam.callbackurl != null && QueryParam.callbackurl != "")
                                        wfhSuccessstatus = CallbackStatus;
                                    //targetListItem["Title"] = resp.url;

                                    try
                                    {
                                        // Create new stopwatch.
                                        Stopwatch stopwatch = new Stopwatch();
                                        stopwatch.Start();
                                        spsetTaskItemByID_New(QueryParam.status.Value, QueryParam.percentcomplete.Value, QueryParam.comments.Value, QueryParam.createdby.Value, QueryParam.taskid.Value, QueryParam.assignevent.Value, QueryParam.assignedto.Value, QueryParam.areviewuserto.Value, QueryParam.spfmrid.Value, QueryParam.tasktype.Value, stateid, roleid);
                                        stopwatch.Stop();
                                        Console.WriteLine(" spsetTaskItemByID_New Time elapsed: {0}", stopwatch.Elapsed);
                                    }
                                    catch (Exception ex) { }
                                    //oItem["pushurl"] = resp.callbackurl; 


                                    j++;
                                }




                            }
                            catch { }
                        }
                        else if (ItemStatus == CallbackStatus) 
                        {


                            try

                            {
                                Console.WriteLine( "STATUS " + strFmrstatus + " SNO " + k + " ID:" + ItemID + " Call back URL request -- " + " stateid:" + stateid);

                                //  targetListItem["pushurl"] = resp.url.ToString().Replace("sppipapitestlocal", "sppipapitesting");

                                var uri = new Uri(url);
                                var query = HttpUtility.ParseQueryString(uri.Query);
                                if (url.ToLower().Contains("/spsettaskitembyid?"))
                                {
                                    QueryParams = JArray.Parse(ClsGeneral.GetJsonStringFromQueryString(query.ToString().ToLower()));

                                    QueryParam = QueryParams[0];
                                    if (QueryParam.stateid != null) stateid = QueryParam.stateid.Value;
                                    if (stateids != "" && !stateids.Contains(stateid)) continue;
                                    // below method for update bulkpush apid task status
                                    SBquery.AppendFormat("<Method ID=\"{0}\">" +
                                          "<SetList>{1}</SetList>" +
                                          "<SetVar Name=\"ID\">{2}</SetVar>" +
                                           //for update //
                                           //    "<SetVar Name=\"Cmd\">Save</SetVar>", _destListname, listGuid, item.ID)
                                           "<SetVar Name=\"Cmd\">Save</SetVar>", bulkpushlistname, bulkpushapisGuid, ItemID);




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
                                            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "status", wfhSuccessstatus);
                                            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "log", "SSOM Call back success");
                                        }
                                    }
                                    catch (Exception ex)
                                    {

                                        SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "status", CallbackFailStatus);
                                        SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "log", "SSOM Call back fail " + ex.Message);
                                    }
                                    SBquery.Append("</Method>");
                                    //spsetFMRDBBulk(QueryParam.fmrid.Value, QueryParam.remarks.Value, "", ref BulkclientContext, QueryParam.assignedto.Value, QueryParam.fy.Value, QueryParam.stateid.Value, QueryParam.fmrtype.Value, "");
                                    // oItem["pushurl"] = BulkAPI.callbackurl; 

                                }

                            }
                            catch { }

                            k++;

                        }
                        else if (ItemStatus == DirectCallUrlStatus)
                        {


                            try

                            {
                                Console.WriteLine("STATUS " + strFmrstatus + " SNO " + k + " ID:" + ItemID + " Direct Web URL request -- " + " stateid:" + stateid);

                                //  targetListItem["pushurl"] = resp.url.ToString().Replace("sppipapitestlocal", "sppipapitesting");

                              
                                    SBquery.AppendFormat("<Method ID=\"{0}\">" +
                                          "<SetList>{1}</SetList>" +
                                          "<SetVar Name=\"ID\">{2}</SetVar>" +
                                           //for update //
                                           //    "<SetVar Name=\"Cmd\">Save</SetVar>", _destListname, listGuid, item.ID)
                                           "<SetVar Name=\"Cmd\">Save</SetVar>", bulkpushlistname, bulkpushapisGuid, ItemID);




                                    try
                                    {


                                          Stopwatch stopwatch = new Stopwatch();
                                            stopwatch.Start();
                                            Console.WriteLine(" Direct web request  URL :" + url);
                                            DoWebGetRequest(url, "");
                                            stopwatch.Stop();
                                            Console.WriteLine(" DoWebGetRequest Time elapsed: {0}", stopwatch.Elapsed);
                                            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "status", wfhSuccessstatus);
                                            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "log", "SSOM Direct web request success");
                                       
                                    }
                                    catch (Exception ex)
                                    {

                                        SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "status", CallbackFailStatus);
                                        SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "log", "SSOM Direct web request fail " + ex.Message);
                                    }
                                    SBquery.Append("</Method>");
                                    //spsetFMRDBBulk(QueryParam.fmrid.Value, QueryParam.remarks.Value, "", ref BulkclientContext, QueryParam.assignedto.Value, QueryParam.fy.Value, QueryParam.stateid.Value, QueryParam.fmrtype.Value, "");
                                    // oItem["pushurl"] = BulkAPI.callbackurl; 

                                

                            }
                            catch { }

                            k++;

                        }

                        ii++;
                        //Console.WriteLine(" Current ITem id " + item.ID + " Bulk push SSOM call " + ii);
                        int queuelenght = 100;
                        if (getConfigvalue("queuelength") != "") queuelenght = int.Parse(getConfigvalue("queuelength"));
                        if (ii % queuelenght == 0)
                        {

                            try
                            {
                                strQuerry += strColumnFiels.ToString().Replace("!CONTENT!", SBquery.ToString());
                                var watch1 = System.Diagnostics.Stopwatch.StartNew();

                                // the code that you want to measure comes here
                                // newItem.Update();
                                //  SBquery.Append("</Batch>");
                                // for (int count = 1; count <= 20; count++)
                                // {
                                watch1.Start();
                                destweb.ProcessBatchData(strQuerry);
                                // Thread.Sleep(1000 * 30);
                                watch1.Stop();
                                var elapsedMs1 = watch1.Elapsed;
                                //Console.WriteLine("ITem id " + item.ID + " Bulk push SSOM call " + i + "_" + count + " state id is :" + stateid + " Time Take " + elapsedMs1);
                                Console.WriteLine("ITem id " + item.ID + " Bulk push SSOM call " + ii + "_" + "state id is :" + stateid + " Time Take " + elapsedMs1);
                                // }
                                SBquery.Clear();
                                strQuerry = "";
                                Thread.Sleep(5000);
                                // break;
                                //Console.ReadLine();break;
                            }
                            catch
                            {
                                Console.WriteLine(" Exception  " + item.ID + "Bulk push SSOM call " + ii + " Input  Querry " + strQuerry);
                            }
                        }


                    }

                    if (SBquery.ToString() != "")
                    {
                        strQuerry += strColumnFiels.ToString().Replace("!CONTENT!", SBquery.ToString());
                        var watch = System.Diagnostics.Stopwatch.StartNew();

                        // the code that you want to measure comes here
                        // newItem.Update();
                        //  SBquery.Append("</Batch>");
                        // for (int count = 1; count <= 20; count++)
                        // {
                        watch.Start();
                        destweb.ProcessBatchData(strQuerry);
                        // Thread.Sleep(1000 * 30);
                        watch.Stop();
                        var elapsedMs = watch.Elapsed;
                        //Console.WriteLine("ITem id " + item.ID + " Bulk push SSOM call " + i + "_" + count + " state id is :" + stateid + " Time Take " + elapsedMs1);
                        Console.WriteLine("Last bulk push " + " Bulk push SSOM call " + ii + "_" + "state id is :" + stateid + " Time Take " + elapsedMs);
                        Thread.Sleep(5000);
                    }
                    destweb.AllowUnsafeUpdates = false;
                  
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error message" + ex.Message);
               // Console.ReadLine();
            }
            //Console.ReadLine();
        }
        static private void spsetFMRDBBulk(string fmrid, string remarks, string AssignedTo = "", string FY = "", string stateid = "", string fmrtype = "", string roleid = "")
        {
            /*  SPListItem newItem = destList.Items.Add();
                         //for (int i = 0; i < item.Fields.Count; i++)
                         //   newItem[newItem.Fields[i].InternalName] = item[newItem.Fields[i].InternalName];*/
            // pipflow1.AddItem()

            SPListItem oListItem = pipflow1.Items.Add();

            oListItem["Title"] = fmrid;
            oListItem["remarks"] = remarks;
            oListItem["ry3a"] = AssignedTo;

            if (AssignedTo != "")
            {
                if (_userstable[AssignedTo] == null)
                {
                    SPUser uAssignedTo = null;
                    uAssignedTo = destweb.EnsureUser(strDomainName + HttpUtility.UrlDecode(AssignedTo));
                    _userstable.Add(AssignedTo, uAssignedTo.ID);

                }

                // SPUser uAssignedTo = destweb.AllUsers[strDomainName + AssignedTo];
                oListItem["ry3a"] = _userstable[AssignedTo];
                oListItem["currentAssignee"] = _userstable[AssignedTo];
                // 14;#Ashish Kanoongo

            }
            oListItem["roleid"] = roleid;
            oListItem["FY"] = FY;
            oListItem["stateid"] = stateid;
            if (fmrtype != null && fmrtype != "")
                oListItem["fmrtype"] = fmrtype;
            //oListItem["Body"] = "Hello World!";

            oListItem.Update();

            //clientContext.Load(oListItem);

            string relatedItem = oListItem.ID.ToString();

            // below method for add task to Created fmr
            SBquery.AppendFormat("<Method ID=\"{0}\">" +
               "<SetList>{1}</SetList>" +
               "<SetVar Name=\"ID\">{2}</SetVar>" +
              //for update //
              "<SetVar Name=\"Cmd\">Save</SetVar>", cWfHListName, workflowhistoryGuid, "New");
            //for update  "<SetVar Name=\"Cmd\">Save</SetVar>", _destListname, listGuid,"New");

            if (fmrid != null)
                SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "Title", fmrid);

            if (AssignedTo != "")
                SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "Assigned_x0020_To", _userstable[AssignedTo]);
            // SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "ows_Assigned_x0020_To", uAssignedTo.ID.ToString() + ";#" + AssignedTo);

            if (relatedItem != "")
                SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "relateditem", relatedItem);

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "tasktype", "1");

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "comments", "FMR to ADD Task");

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "TaskOutcome", "");

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "stateid", stateid);

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "roleid", roleid);

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "Status", "Not Started");

            SBquery.Append("</Method>");
            // below method for update bulkpush apid task status
            SBquery.AppendFormat("<Method ID=\"{0}\">" +
                   "<SetList>{1}</SetList>" +
                   "<SetVar Name=\"ID\">{2}</SetVar>" +
                    //for update //
                    //    "<SetVar Name=\"Cmd\">Save</SetVar>", _destListname, listGuid, item.ID)
                    "<SetVar Name=\"Cmd\">Save</SetVar>", bulkpushlistname, bulkpushapisGuid, ItemID);

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "status", FMRSuccessStatus);
            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "log", "SSOM ADD fmr");


            SBquery.Append("</Method>");

            //return SBquery.ToString();
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
        static private void spsetTaskItemByID_New(string status, string percentComplete, string Comments, string createdby, string taskid, string assignevent = "", string AssignedTo = "", string areviewuserTo = "", string SPFmrID = "", string TASKTYPE = "", string stateid = "", string roleid = "")
        {
            /*  SPListItem newItem = destList.Items.Add();
                        //for (int i = 0; i < item.Fields.Count; i++)
                        //   newItem[newItem.Fields[i].InternalName] = item[newItem.Fields[i].InternalName];*/
            if (AssignedTo == null) AssignedTo = "";
            if (percentComplete == null) percentComplete = "1";
            if (TASKTYPE == null) TASKTYPE = "1";
            if (TASKTYPE == "1") percentComplete = "0";

            if (_userstable[createdby] == null)
            {
                SPUser uAssignedTo = null;
                uAssignedTo = destweb.EnsureUser(strDomainName + HttpUtility.UrlDecode(createdby));
                _userstable.Add(createdby, uAssignedTo.ID);

            }
            if (AssignedTo != "" && _userstable[AssignedTo] == null)
            {
                SPUser uAssignedTo = null;
                uAssignedTo = destweb.EnsureUser(strDomainName + HttpUtility.UrlDecode(AssignedTo));
                _userstable.Add(AssignedTo, uAssignedTo.ID);

            }
            if (areviewuserTo != "" && _userstable[areviewuserTo] == null)
            {
                SPUser uAssignedTo = null;
                uAssignedTo = destweb.EnsureUser(strDomainName + HttpUtility.UrlDecode(areviewuserTo));
                _userstable.Add(areviewuserTo, uAssignedTo.ID);

            }


            SBquery.AppendFormat("<Method ID=\"{0}\">" +
               "<SetList>{1}</SetList>" +
               "<SetVar Name=\"ID\">{2}</SetVar>" +
              //for update //
              "<SetVar Name=\"Cmd\">Save</SetVar>", cWfHListName, workflowhistoryGuid, taskid);
            //for update  "<SetVar Name=\"Cmd\">Save</SetVar>", _destListname, listGuid,"New");

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "TaskOutcome", "1");
            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "Status", status);

            SBquery.Append("</Method>");

            SBquery.AppendFormat("<Method ID=\"{0}\">" +
                  "<SetList>{1}</SetList>" +
                  "<SetVar Name=\"ID\">{2}</SetVar>" +
                 
            //for update //
            //    "<SetVar Name=\"Cmd\">Save</SetVar>", _destListname, listGuid, item.ID)
            "<SetVar Name=\"Cmd\">Save</SetVar>", cWfHListName, workflowhistoryGuid, "New");
            string strTitle = "MAIN task";
            if (TASKTYPE == "2")
            {
             
                strTitle = "Additional Review";
            }
            else if (TASKTYPE == "3")
            {
              strTitle = "ROP";
            }
            else if (areviewuserTo != "")
            {  strTitle = "sub task"; }


            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "Title", strTitle);
            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "relateditem", SPFmrID);
            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "event", assignevent);
            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "comments", Comments);

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "TaskOutcome", "");

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "stateid", stateid);

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "roleid", roleid);

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "Status", "Not Started");

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "tasktype", TASKTYPE);
            if (AssignedTo != "")
                SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "Assigned_x0020_To", _userstable[AssignedTo]);
            if (createdby != "")
                SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "approveduser", _userstable[createdby]);
            if (areviewuserTo != "")
                SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "areviewuser", _userstable[areviewuserTo]);


            SBquery.Append("</Method>");

            SBquery.AppendFormat("<Method ID=\"{0}\">" +
                "<SetList>{1}</SetList>" +
                "<SetVar Name=\"ID\">{2}</SetVar>" +
                 //for update //
                 //    "<SetVar Name=\"Cmd\">Save</SetVar>", _destListname, listGuid, item.ID)
                 "<SetVar Name=\"Cmd\">Save</SetVar>", cPipflowListName, pipflow1Guid, SPFmrID);

            if (createdby != "")
                SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "ry3a", _userstable[createdby]);
            if (AssignedTo != "")
                SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "currentAssignee", _userstable[AssignedTo]);

            // SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "remarks", Comments);
            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "stateid", stateid);
            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "roleid", roleid);
            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "currenttaskid", taskid);

            SBquery.Append("</Method>");

            // below method for update bulkpush apid task status
            SBquery.AppendFormat("<Method ID=\"{0}\">" +
                  "<SetList>{1}</SetList>" +
                  "<SetVar Name=\"ID\">{2}</SetVar>" +
                   //for update //
                   //    "<SetVar Name=\"Cmd\">Save</SetVar>", _destListname, listGuid, item.ID)
                   "<SetVar Name=\"Cmd\">Save</SetVar>", bulkpushlistname, bulkpushapisGuid, ItemID);

            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "status", wfhSuccessstatus);
            SBquery.AppendFormat("<SetVar Name=\"urn:schemas-microsoft-com:office:office#{0}\">{1}</SetVar>", "log", "SSOM update NEXT fmr");


            SBquery.Append("</Method>");


        }

        static private void AddFMRCopyItems(SPWeb web, string strCreatedby, string strAssignedTo, string stateid)
        {

            string strSOurcelist = "workflow_history", strDestlist = "bulkpushapis";
            //SPList SList = web.Lists[strSOurcelist];
            SPList DList = web.Lists[strDestlist];
            SPQuery SPquery = new SPQuery();
            //oQuery.ViewXml = ("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + sateid + "</Value></Eq></Where></Query></View>");
            SPquery.Query = "<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + stateid + "</Value></Eq><Eq><FieldRef Name='Status'/><Value Type='Text'>Not Started</Value></Eq></And></Where>";
            SPquery.RowLimit = 100;
           // SPListItemCollection SPLists = SList.GetItems(SPquery);
           // Console.WriteLine(" Total list count for " + SPLists.Count.ToString());
          
            for(int i=1;i<=2000;i++)
            {
                SPListItem newItem = DList.Items.Add();
               // ItemID = i.ToString();
                //ItemStatus = item["Status"].ToString();



                //for (int i = 0; i < item.Fields.Count; i++)
                //   newItem[newItem.Fields[i].InternalName] = item[newItem.Fields[i].InternalName];
                newItem["Title"] = "ADD FMR_" + stateid  + "_" + i.ToString();
                newItem["stateid"] = stateid;
                if (strPUSHType == "addfmr")
                {
                    newItem["pushurl"] = string.Format("http://52.172.200.35:8111/pipflowsitetesting/api/Pipflow/spsetfmr?fmrid={0}&remarks=ADD_FMR_{1}_{0}" +
                        "&listname=pipflow1&AssignedTo={3}&FY=2021-22" +
                        "&stateid={1}&fmrtype=1&roleid={2}", i.ToString(), stateid, "1", strCreatedby);
                    newItem["status"] = "-1";
                }
               
                var watch1 = System.Diagnostics.Stopwatch.StartNew();
                watch1.Start();
                // the code that you want to measure comes here
                newItem.Update();
                watch1.Stop();
                var elapsedMs1 = watch1.Elapsed;
                Console.WriteLine(" SNO id " + i + " State ID " + stateid + " Bulk push SSOM call " + i.ToString() + " Time Take " + elapsedMs1);
                
            }

        }
        static private void CopyItems(SPWeb web, string strCreatedby, string strAssignedTo, string stateid)
        {

            string strSOurcelist = "workflow_history", strDestlist = "bulkpushapis";
            SPList SList = web.Lists[strSOurcelist];
            SPList DList = web.Lists[strDestlist];
            SPQuery SPquery = new SPQuery();
            //oQuery.ViewXml = ("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + sateid + "</Value></Eq></Where></Query></View>");
            SPquery.Query = "<Where><And><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + stateid + "</Value></Eq><Eq><FieldRef Name='Status'/><Value Type='Text'>Not Started</Value></Eq></And></Where>";
            SPquery.RowLimit = 3000;
            SPListItemCollection SPLists = SList.GetItems(SPquery);
            Console.WriteLine(" Total list count for " + SPLists.Count.ToString());
            int SNO = 1;
            foreach (SPListItem item in SPLists)
            {
                SPListItem newItem = DList.Items.Add();
                ItemID = item["ID"].ToString();
                ItemStatus = item["Status"].ToString();



                //for (int i = 0; i < item.Fields.Count; i++)
                //   newItem[newItem.Fields[i].InternalName] = item[newItem.Fields[i].InternalName];
                newItem["Title"] = item["Title"];
                newItem["stateid"] = item["stateid"];
                if (strPUSHType == "addfmr")
                {
                    newItem["pushurl"] = string.Format("http://52.172.200.35:8111/pipflowsitetesting/api/Pipflow/spsetfmr?fmrid=bulkpush_{0}&remarks=U.8.1.5" +
                        "&listname=pipflow1&AssignedTo=spm&FY=2021-22" +
                        "&stateid={1}&fmrtype=1&roleid={2}", item["relateditem"], item["stateid"], item["roleid"]);
                    newItem["status"] = "-1";
                }
                else
                {
                    newItem["pushurl"] = string.Format("http://52.172.200.35:8111/sppipapitestlocal/api/Pipflow/spsettaskitembyid?status=approved&percentcomplete=1&comments=how%20r%20ou" +
                        "&taskid={0}&createdby={1}&assignevent=1&assignedto={2}&areviewuserTo=&spfmrid={3}&TASKTYPE=1&stateid={4}&roleid={5}" +
                        "&callbackurl=", ItemID, strCreatedby, strAssignedTo, item["relateditem"], item["stateid"], item["roleid"]);
                    newItem["status"] = "-2";
                }
                var watch1 = System.Diagnostics.Stopwatch.StartNew();
                watch1.Start();
                // the code that you want to measure comes here
                newItem.Update();
                watch1.Stop();
                var elapsedMs1 = watch1.Elapsed;
                Console.WriteLine("ITem id " + item.ID + " State ID " + stateid + " Bulk push SSOM call " + SNO + " Time Take " + elapsedMs1);
                SNO++;
                if (SNO % 2000 == 0) break;
            }

        }
        public static String getConfigvalue(String key)
        {
            if (ConfigurationSettings.AppSettings[key] != null)
                return ConfigurationSettings.AppSettings[key];
            else
                return "";
        }
    }
}







