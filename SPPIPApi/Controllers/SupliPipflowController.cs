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
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.IO;
using System.Web.Http.Cors;
using System.Xml;
using System.Data;
using System.Xml.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace SPPipAPi.Controllers
{
    [EnableCors(origins: "http://sharepoint2:8081,http://localhost:44349", headers: "*", methods: "*")]
    public class SupliPipflowController : ApiController
    {

        // GET api/values/5
        String strSiteURL = "http://sharepoint2/sites/teamsiteex/PipFlowSite", strUSER = "spuser2", strPWD = "User@123#", strADUserURL = "", SitepathValue = "";
        string SITE_API_URL = "";
        string strDomainName = ConfigurationManager.AppSettings["DomainName"].ToString();
        Boolean isWait = false;
        string cPipflowListName = "Spipflow1";
        string cPipdeptListName = "Spipdept";
        string cWfListName = "Sworkflow_history";
        string cWfSHListName = "Sworkflow_history";
        string cBUlkPushListName = "SBulkPushAPIS";
        public SupliPipflowController()
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

        [Route("api/SupliPipflow/spgetuserinfo")]
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

        //[System.Web.Http.Route("api/SupliPipflow/getADUser")]

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



        [Route("api/SupliPipflow/spcheckuser")]
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

        [Route("api/SupliPipflow/spgetListByName")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spgetListByName(string Listname, string FY, string fmrtype, string stateid,string sid, string roleid = "", string status = "", string ListitemId = "")
        {

            // prepare site connection
            try
            {
                // global parameters<View><Query>
                if (ClsGeneral.getConfigvalue("FROM_SSOM_URL") != "")
                    return getHttpResponseMessage(ClsGeneral.DoWebGetRequest(ClsGeneral.getConfigvalue("FROM_SSOM_URL") + "/api/Pipflow/spgetListByName" + ControllerContext.Request.RequestUri.Query.ToString(), ""));


                string strCamlQuery_temp = "<View><Query><Where><And>!WHERE!</And></Where></Query></View>";
                string strWhereText_temp = "<Eq><FieldRef Name='!NAME!'/><Value Type='!TYPE!'>!VALUE!</Value></Eq>";
                // string strWhereText_temp_withoutAND = "<Eq><FieldRef Name='!NAME!'/><Value Type='!TYPE!'>!VALUE!</Value></Eq>";
                string strCamlQuery = "";
                int serarchCount = 0;
                if (status == null) status = "";
                // if (1 == 1) strCamlQuery += strWhereText_temp.Replace("!NAME!", "1").Replace("!TYPE!", "Text").Replace("!VALUE!", "1");
                if (FY != null && FY != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "FY").Replace("!TYPE!", "Text").Replace("!VALUE!", FY); } else FY = "";
                if (stateid != null && stateid != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "stateid").Replace("!TYPE!", "Number").Replace("!VALUE!", stateid); } else stateid = "";
                if (roleid != null && roleid != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "roleid").Replace("!TYPE!", "Number").Replace("!VALUE!", roleid); } else roleid = "";
                if (fmrtype != null && fmrtype != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "fmrtype").Replace("!TYPE!", "Number").Replace("!VALUE!", fmrtype); } else fmrtype = "";



                CamlQuery camlQuery = new CamlQuery();
                /* old camlQuery.ViewXml = "<View><RowLimit>10000</RowLimit></View>";
                camlQuery.ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Choice'>Not Started</Value></Eq>"
                                              + "<Eq><FieldRef Name='AssignedTo'/><Value Type='UserMulti'>{0}</Value></Eq><Contains><FieldRef Name='RelatedItems'/><Value Type='Text'>:{1},</Value></Contains>");
                   + "</Where></Query></View>", Userid.TrimEnd(','), FMRID);*/
                strCamlQuery = strCamlQuery_temp.Replace("!WHERE!", strCamlQuery);
                if (serarchCount == 1) strCamlQuery = strCamlQuery.Replace("<And>", "").Replace("</And>", "");
                // if (serarchCount > 2) strCamlQuery = strCamlQuery.Replace("<And>", "").Replace("</And>", "");
                //  if (serarchCount == 0)
                strCamlQuery = "<View><RowLimit>1000000</RowLimit></View>";

                camlQuery.ViewXml = "<View><Query><Where><And>" +
                              "<And><Eq><FieldRef Name='fmrtype' /><Value Type='Number'>" + fmrtype + "</Value></Eq>" +
                              "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq></And>";
                camlQuery.ViewXml += "<And><Eq><FieldRef Name='FY' /><Value Type='Text'>" + FY + "</Value></Eq>";
                camlQuery.ViewXml += "<Eq><FieldRef Name='sid' /><Value Type='Text'>" + sid + "</Value></Eq></And>";
                if (roleid != "")
                    camlQuery.ViewXml += "<And><Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";
                if (roleid != "")
                    camlQuery.ViewXml += "</And>";
                camlQuery.ViewXml += "</And></Where></Query></View>";
                //camlQuery.ViewXml = strCamlQuery;



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

                List list = web.Lists.GetByTitle(cPipflowListName);

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
                         item => item["Created"],
                        item => item["Modified"],
                        item => item["FY"],
                           item => item["fmrtype"],
                          item => item["stateid"],
                           item => item["roleid"],
                            item => item["sid"],
                        item => item["currenttaskid"]));
                clientContext.ExecuteQuery();
                List<fmrlist> respmsg = new List<fmrlist>();

                FieldUserValue[] fuvassigned_to = null;
                FieldUserValue[] fuvCurassigned_to = null;

                FieldUserValue fuvEditor = null;
                FieldUserValue fuvAuthor = null;


                foreach (ListItem oListItem in olists)
                {
                    string strLookupValues = "", strLookupIDS = "", strLookupAssignTOIds = "", strLookupAssignTOvalues = "";
                    try
                    {

                        if (status != "" && oListItem["status"].ToString().ToLower() != status.ToLower())
                            continue;
                        if (fmrtype != "" && oListItem["fmrtype"].ToString().ToLower() != fmrtype.ToLower())
                            continue;
                        if (stateid != "" && oListItem["stateid"].ToString().ToLower() != stateid.ToLower())
                            continue;
                        if (roleid != "" && oListItem["roleid"] != null && oListItem["roleid"].ToString().ToLower() != roleid.ToLower())
                            continue;
                        if (FY != "" && oListItem["FY"].ToString().ToLower() != FY.ToLower())
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
                        // new code implemented for eliminate the task geranration time 
                       //  string currentTaskID = getCurrentTaskIDofFMR(oListItem.Id.ToString(), strLookupAssignTOIds);
                        respmsg.Add(new fmrlist
                        {
                            id = oListItem.Id.ToString(),
                            title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                            assigned_to = (oListItem["ry3a"] != null) ? strLookupAssignTOvalues.TrimEnd(',') : "",
                            assigned_to_id = (oListItem["ry3a"] != null) ? strLookupAssignTOIds.TrimEnd(',') : "",
                            status = (oListItem["status"] != null) ? oListItem["status"].ToString() : "",
                            fmrtype = (oListItem["fmrtype"] != null) ? oListItem["fmrtype"].ToString() : "",
                            stateid = (oListItem["stateid"] != null) ? oListItem["stateid"].ToString() : "",
                            roleid = (oListItem["roleid"] != null) ? oListItem["roleid"].ToString() : "",
                            fy = (oListItem["FY"] != null) ? oListItem["FY"].ToString() : "",
                            sid = (oListItem["sid"] != null) ? oListItem["sid"].ToString() : "",
                            remarks = (oListItem["remarks"] != null) ? oListItem["remarks"].ToString() : "",
                            currenttaskid = (oListItem["currenttaskid"] != null) ? oListItem["currenttaskid"].ToString() : "",
                            Modified_By = (oListItem["Editor"] != null) ? fuvEditor.LookupValue : "",
                            Modified_By_id = (oListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                            Modified = (oListItem["Modified"] != null) ? oListItem["Modified"].ToString() : "",
                            Created_By = (oListItem["Author"] != null) ? fuvAuthor.LookupValue : "",
                            Created_By_id = (oListItem["Author"] != null) ? fuvAuthor.LookupId.ToString() : "",
                            Created = (oListItem["Created"] != null) ? oListItem["Created"].ToString() : "",
                            currentassign_to = (oListItem["currentAssignee"] != null) ? strLookupValues.TrimEnd(',') : "",
                            currentassign_to_id = (oListItem["currentAssignee"] != null) ? strLookupIDS.TrimEnd(',') : ""
                        });
                    }
                    catch(Exception ex)
                    {

                        return getHttpResponseMessage(JsonConvert.SerializeObject(ex.Message));
                    }


                }

                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));
            }
            catch (Exception ex)
            {


                return getErrormessage(ex.Message);
            }



        }



        private string getCurrentTaskIDofFMR(string FMRID, string Userid)
        {

            CamlQuery camlQuery = new CamlQuery();
            //camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";
            // camlQuery.ViewXml = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='ParentID'/><Value Type='Counter'>{0}</Value></Eq></Where></Query></View>", _parentID);
            // camlQuery.ViewXml = string.Format("<View><Query><Where><In><FieldRef Name='RelatedItems'/><Value Type='Number'>{0}</Value></In></Where></Query></View>", ReleatedItems);
            camlQuery.ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Choice'>Not Started</Value></Eq>"
                                               + "<Eq><FieldRef Name='AssignedTo'/><Value Type='UserMulti'>{0}</Value></Eq><Contains><FieldRef Name='RelatedItems'/><Value Type='Text'>:{1},</Value></Contains>"
                                                + "</Where></Query></View>", Userid.TrimEnd(','), FMRID);

            // camlQuery.ViewXml = string.Format("<View><Query><Where><Eq><FieldRefName='Status'/><ValueType='Choice'>Not Started</Value></Eq></Where></Query></View>", Userid);

            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential(strUSER, strPWD);
            try
            {
                Web web = clientContext.Web;
                clientContext.Load(web);
                //  var tasks;
                // clientContext.Load(tasks, c => c.Where(t => t.Parent != null && t.Parent.Id == parentId));
                List list = web.Lists.GetByTitle(cWfListName);
                ListItemCollection olists = list.GetItems(camlQuery);
                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(olists,
                     items => items.Include(
                        item => item.Id,
                         item => item["RelatedItems"]
                       ));
                clientContext.ExecuteQuery();
                foreach (ListItem oListItem in olists)
                {
                    if (oListItem["RelatedItems"] != null && oListItem["RelatedItems"].ToString() != "")
                    {

                        List<RelatedItemFieldValue> obj = JsonConvert.DeserializeObject<List<RelatedItemFieldValue>>(oListItem["RelatedItems"].ToString());
                        if (obj != null)
                            if (FMRID == obj[0].ItemId.ToString())
                                return oListItem.Id.ToString();
                    }


                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return "";
        }



        [Route("api/SupliPipflow/spsetFMR")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spsetFMR(string fmrid, string remarks, string Listname, string AssignedTo = "", string FY = "", string stateid = "", string fmrtype = "", string roleid = "")
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



                List oList = clientContext.Web.Lists.GetByTitle(cPipflowListName);

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
                oListItem["roleid"] = roleid;
                oListItem["stateid"] = stateid;
                if (fmrtype != null && fmrtype != "")
                    oListItem["fmrtype"] = fmrtype;
                //oListItem["Body"] = "Hello World!";

                oListItem.Update();

                clientContext.ExecuteQuery();
                //ListItem targetListItem = oList.(ListitemId);

                /*   isWait = true;
                   getLatestTaskIDByFMRNO(string.Format(SITE_API_URL + "/api/SupliPipflow/spgetTaskDetails?listname&taskuser={0}&ReleatedItems={1}&status=not started", "SPM", fmrid));
                   */
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }

            return getSuccessmessage("success");
        }
        [System.Web.Http.Route("api/SupliPipflow/BulkPushAPISJsonUpload")]
        [System.Web.Http.HttpPost] 
        public HttpResponseMessage BulkPushAPISJsonUpload()
        {
            // upload file name content should be filename__!fmrtype!_!roleid!_!stateid!.josn
            // fmr type as pipflow or suplli
            HttpResponseMessage result = null;
            var httpRequest = HttpContext.Current.Request;
            if (httpRequest.Files.Count > 0)
            {

                foreach (string file in httpRequest.Files)
                {
                    var postedFile = httpRequest.Files[file];
                    var filePath = HttpContext.Current.Server.MapPath("~/" + postedFile.FileName);
                    try
                    {
                        if (ClsGeneral.getConfigvalue("SUPPLI_UPLOAD_FILE_PATH") != "") filePath = ClsGeneral.getConfigvalue("SUPPLI_UPLOAD_FILE_PATH") + "/" + postedFile.FileName;
                        postedFile.SaveAs(filePath);

                    }
                    catch (Exception ex) { return getSuccessmessage(ex.Message); }
                }
                return getSuccessmessage("success");
            }
            else
            {
                return getErrormessage("failed");
            }

            //   return result;
        }
        [Route("api/SupliPipflow/spupdateFMR")]
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
                    oList = clientContext.Web.Lists.GetByTitle(cPipflowListName);

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


        [Route("api/SupliPipflow/spgetListItemByID")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spgetListItemByID(string Listname, string ListitemId)
        {

            // prepare site connection
            try
            {
                // global parameters

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><RowLimit>100000</RowLimit></View>";
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
                         item => item["stateid"],
                        item => item["roleid"],
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
                    roleid = (targetListItem["roleid"] != null) ? targetListItem["roleid"].ToString() : "",
                    stateid = (targetListItem["stateid"] != null) ? targetListItem["stateid"].ToString() : "",
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

        [Route("api/SupliPipflow/spsetSingleTaskItemByID")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spsetSingleTaskItemByID(string status, string percentComplete, string Comments, string createdby, string taskid, string assignevent = "", string AssignedTo = "", string areviewuserTo = "", string SPFmrID = "", string TASKTYPE = "", string callbackurl = "")
        {
            // prepare site connection
            string strcallbackurl = callbackurl;
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential("spm", "pip@123");
            if (AssignedTo == null) AssignedTo = "";
            if (percentComplete == null) percentComplete = "1";
            if (TASKTYPE == null) TASKTYPE = "1";
            status = "Not started";
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
                SP.List oList = clientContext.Web.Lists.GetByTitle(cWfListName);
                SP.ListItem list2 = oList.GetItemById(Int32.Parse(taskid));
                User createuser = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(createdby));
                User assignuser = null;
                clientContext.Load(createuser);
                int i = 0;

                // if assignedTo is null its related to sub task only 
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
                if (TASKTYPE == "1") percentComplete = "0.9";


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
                        list2["PercentComplete"] = percentComplete;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = "";
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = "1";

                    }
                    else
                    {

                        list2["PercentComplete"] = percentComplete;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = "";
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = "1";
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



                    var lookupValue = new FieldLookupValue();
                    lookupValue.LookupId = int.Parse(taskid); // Get parent item ID and assign it value in lookupValue.LookupId  
                    var lookupValueCollection = new FieldLookupValue[1];
                    lookupValueCollection.SetValue(lookupValue, 0);

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

                        oItem["AssignedTo"] = createuser;
                        // below column is userd to set the aditional review user store
                        oItem["areviewuser"] = AreveiweruserValueCollection;
                        if (TASKTYPE == "2")
                            oItem["Title"] = "Additional Review";
                        else if (TASKTYPE == "3")
                            oItem["Title"] = "ROP";
                        else
                            oItem["Title"] = "sub task";
                        oItem["ParentID"] = lookupValueCollection; // set chidl item ParentID field  
                        oItem["tasktype"] = TASKTYPE;
                        oItem["relateditem"] = SPFmrID;
                        oItem["PercentComplete"] = 0;
                        oItem.Update();
                        clientContext.ExecuteQuery();

                    }


                    //for close current task and assign to next user

                    list2["PercentComplete"] = percentComplete;
                    list2["Status"] = status;
                    list2["TaskOutcome"] = "";
                    list2["comments"] = Comments;
                    list2["event"] = assignevent;
                    list2["tasktype"] = TASKTYPE;
                    list2.Update();
                    clientContext.ExecuteQuery();


                }
                else if ((TASKTYPE == "2" || TASKTYPE == "3"))
                {

                    /// for closing or update current task id 
                    list2["PercentComplete"] = percentComplete;
                    list2["Status"] = status;
                    list2["TaskOutcome"] = status;
                    list2["comments"] = Comments;
                    list2["event"] = assignevent;
                    list2["tasktype"] = TASKTYPE;
                    list2.Update();
                    clientContext.ExecuteQuery();

                }





                // updated latest task id to FMR list for viewing
                //http://52.172.200.35:2020/sppipapidevtesting/api/SupliPipflow/spgetTaskDetails?listname&taskuser=&ReleatedItems=82&status=not started
                /*
                                if (SPFmrID != null && SPFmrID != "" && AssignedTo != null && AssignedTo != "")
                                {
                                    isWait = true;
                                    getLatestTaskIDByFMRNO(string.Format(SITE_API_URL + "/api/SupliPipflow/spgetTaskDetails?listname&taskuser={0}&ReleatedItems={1}&status=not started", AssignedTo, SPFmrID));
                                }

                                if (strcallbackurl != null && strcallbackurl != "")
                                {
                                    string strResp = ClsGeneral.DoWebGetRequest(strcallbackurl.Replace("~", "&"), "");
                                }
                                //end of the 

                                */
            }
            catch (Exception ex)
            {

                return getErrormessage(ex.Message);
            }
            return getSuccessmessage("Success");

        }


        // TASKTYPE 1 FOR NORMAL,2 FOR ADD REVIEW,3 FOR ROP
        // below for workflow task assign and reject and others tagas 
        [Route("api/SupliPipflow/spsetTaskItemByID_new")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spsetTaskItemByID_New(string status, string percentComplete, string Comments, string createdby, string taskid, string assignevent = "", string AssignedTo = "", string areviewuserTo = "", string SPFmrID = "", string TASKTYPE = "", string callbackurl = "")
        {
            // prepare site connection
            string strcallbackurl = callbackurl;
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential("spm", "pip@123");
            if (AssignedTo == null) AssignedTo = "";
            if (percentComplete == null) percentComplete = "1";
            if (TASKTYPE == null) TASKTYPE = "1";
            if (TASKTYPE == "1") percentComplete = "0.9";
            status = "Not Started";
            try
            {

                User createuser = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(createdby));
                User assignuser = null;
                clientContext.Load(createuser);

                //Get the list items from list
                SP.List oList = clientContext.Web.Lists.GetByTitle(cWfListName);
                SP.ListItem list2 = oList.GetItemById(Int32.Parse(taskid));
                clientContext.Load(list2);
                clientContext.ExecuteQuery();

                SP.ListItem list2History = list2;
                //  clientContext.ExecuteQuery();
                if (TASKTYPE == "1")
                    setPreviousTaskHistory(ref list2, SPFmrID, taskid, status);
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
                    list2["AssignedTo"] = userValueCollection;
                }



                //list2["AssignedTo"] = @"it1";
                //list2["Completed"] = true;



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
                        list2["PercentComplete"] = percentComplete;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = "";
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = "1";

                    }
                    else
                    {

                        list2["PercentComplete"] = percentComplete;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = "";
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = "1";
                        //list2["comments"] = Comments;
                    }
                    // list2["Status"] = "Rejected";
                    // list2["TaskOutcome"] = "Rejected";
                    list2.Update();
                    clientContext.ExecuteQuery();

                    spsetAddorupdteItemByID("", cPipflowListName, "", "", SPFmrID, "", taskid, createdby, AssignedTo);
                }

                if (areviewuserTo != null && areviewuserTo != "" && (TASKTYPE == "2" || TASKTYPE == "3"))
                {
                    ListItemCreationInformation oListItemCreationInformation = new ListItemCreationInformation();



                    var lookupValue = new FieldLookupValue();
                    lookupValue.LookupId = int.Parse(taskid); // Get parent item ID and assign it value in lookupValue.LookupId  
                    var lookupValueCollection = new FieldLookupValue[1];
                    lookupValueCollection.SetValue(lookupValue, 0);

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

                        oItem["AssignedTo"] = createuser;
                        // below column is userd to set the aditional review user store
                        oItem["areviewuser"] = AreveiweruserValueCollection;
                        if (TASKTYPE == "2")
                            oItem["Title"] = "Additional Review";
                        else if (TASKTYPE == "3")
                            oItem["Title"] = "ROP";
                        else
                            oItem["Title"] = "sub task";
                        oItem["ParentID"] = lookupValueCollection; // set chidl item ParentID field  
                        oItem["tasktype"] = TASKTYPE;
                        oItem["relateditem"] = SPFmrID;
                        oItem["PercentComplete"] = 0;
                        oItem.Update();
                        clientContext.ExecuteQuery();

                    }


                    //for close current task and assign to next user

                    list2["PercentComplete"] = percentComplete;
                    list2["Status"] = status;
                    list2["TaskOutcome"] = status;
                    list2["comments"] = Comments;
                    list2["event"] = assignevent;
                    list2["tasktype"] = TASKTYPE;
                    list2.Update();
                    clientContext.ExecuteQuery();


                }
                else if ((TASKTYPE == "2" || TASKTYPE == "3"))
                {

                    /// for closing or update current task id 
                    list2["PercentComplete"] = percentComplete;
                    list2["Status"] = status;
                    list2["TaskOutcome"] = status;
                    list2["comments"] = Comments;
                    list2["event"] = assignevent;
                    list2["tasktype"] = TASKTYPE;
                    list2.Update();
                    clientContext.ExecuteQuery();

                }





                // updated latest task id to FMR list for viewing
                //http://52.172.200.35:2020/sppipapidevtesting/api/SupliPipflow/spgetTaskDetails?listname&taskuser=&ReleatedItems=82&status=not started
                /*
                    if (SPFmrID != null && SPFmrID != "" && AssignedTo != null && AssignedTo != "")
                    {
                        isWait = true;
                        getLatestTaskIDByFMRNO(string.Format(SITE_API_URL + "/api/SupliPipflow/spgetTaskDetails?listname&taskuser={0}&ReleatedItems={1}&status=not started", AssignedTo, SPFmrID));
                    }
                    */
                if (strcallbackurl != null && strcallbackurl != "")
                {
                    string strResp = ClsGeneral.DoWebGetRequest(strcallbackurl.Replace("~", "&"), "");
                }
                //end of the 
            }
            catch (Exception ex)
            {

                return getErrormessage(ex.Message);
            }
            return getSuccessmessage("Success");

        }
        private void setPreviousTaskHistory(ref SP.ListItem _list2History, string SPFmrID, string taskid, string Status)
        {


            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential("spm", "pip@123");
            List oList = clientContext.Web.Lists.GetByTitle("Sworkflow_history");

            ListItemCreationInformation itemCreateInfo = new ListItemCreationInformation();
            ListItem oListItem = oList.AddItem(itemCreateInfo);
            oListItem["Title"] = _list2History["Title"];
            oListItem["comments"] = _list2History["PercentComplete"];
            oListItem["approveduser"] = _list2History["approveduser"];
            oListItem["areviewuser"] = _list2History["areviewuser"];
            oListItem["Assigned_x0020_To"] = _list2History["AssignedTo"];
            oListItem["event"] = _list2History["event"];
            oListItem["roleid"] = _list2History["roleid"];
            oListItem["stateid"] = _list2History["stateid"];
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
            clientContext.ExecuteQuery();

        }


        // TASKTYPE 1 FOR NORMAL,2 FOR ADD REVIEW,3 FOR ROP
        // below for workflow task assign and reject and others tagas 
        [Route("api/SupliPipflow/spsetTaskItemByID")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spsetTaskItemByID(string status, string percentComplete, string Comments, string createdby, string taskid, string assignevent = "", string AssignedTo = "", string areviewuserTo = "", string SPFmrID = "", string TASKTYPE = "", string callbackurl = "")
        {
            if (ConfigurationManager.AppSettings["IS_SINGLE_TASK"] != null && ConfigurationManager.AppSettings["IS_SINGLE_TASK"].ToString().ToUpper() == "Y")
            {
                return spsetTaskItemByID_New(status, percentComplete, Comments, createdby, taskid, assignevent, AssignedTo, areviewuserTo, SPFmrID, TASKTYPE, callbackurl);
            }

            // prepare site connection
            string strcallbackurl = callbackurl;
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential("spm", "pip@123");
            if (AssignedTo == null) AssignedTo = "";
            if (percentComplete == null) percentComplete = "1";
            if (TASKTYPE == null) TASKTYPE = "1";

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

                User createuser = clientContext.Web.EnsureUser(strDomainName + HttpUtility.UrlDecode(createdby));
                User assignuser = null;
                clientContext.Load(createuser);
                clientContext.ExecuteQuery();
                // new code implemented for eliminate the task geranration time 
                if (TASKTYPE == "1")
                    taskid = getCurrentTaskIDofFMR(SPFmrID, createuser.Id.ToString());

                //Get the list items from list
                SP.List oList = clientContext.Web.Lists.GetByTitle(cWfListName);

                SP.ListItem list2 = oList.GetItemById(Int32.Parse(taskid));

                int i = 0;

                // if assignedTo is null its related to sub task only 
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
                        list2["PercentComplete"] = percentComplete;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = status;
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = "1";

                    }
                    else
                    {

                        list2["PercentComplete"] = percentComplete;
                        list2["Status"] = status;
                        list2["TaskOutcome"] = status;
                        list2["comments"] = Comments;
                        list2["event"] = assignevent;
                        list2["tasktype"] = "1";
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



                    var lookupValue = new FieldLookupValue();
                    lookupValue.LookupId = int.Parse(taskid); // Get parent item ID and assign it value in lookupValue.LookupId  
                    var lookupValueCollection = new FieldLookupValue[1];
                    lookupValueCollection.SetValue(lookupValue, 0);

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

                        oItem["AssignedTo"] = createuser;
                        // below column is userd to set the aditional review user store
                        oItem["areviewuser"] = AreveiweruserValueCollection;
                        if (TASKTYPE == "2")
                            oItem["Title"] = "Additional Review";
                        else if (TASKTYPE == "3")
                            oItem["Title"] = "ROP";
                        else
                            oItem["Title"] = "sub task";
                        oItem["ParentID"] = lookupValueCollection; // set chidl item ParentID field  
                        oItem["tasktype"] = TASKTYPE;
                        oItem["relateditem"] = SPFmrID;
                        oItem["PercentComplete"] = 0;
                        oItem.Update();
                        clientContext.ExecuteQuery();

                    }


                    //for close current task and assign to next user

                    list2["PercentComplete"] = percentComplete;
                    list2["Status"] = status;
                    list2["TaskOutcome"] = status;
                    list2["comments"] = Comments;
                    list2["event"] = assignevent;
                    list2["tasktype"] = TASKTYPE;
                    list2.Update();
                    clientContext.ExecuteQuery();


                }
                else if ((TASKTYPE == "2" || TASKTYPE == "3"))
                {

                    /// for closing or update current task id 
                    list2["PercentComplete"] = percentComplete;
                    list2["Status"] = status;
                    list2["TaskOutcome"] = status;
                    list2["comments"] = Comments;
                    list2["event"] = assignevent;
                    list2["tasktype"] = TASKTYPE;
                    list2.Update();
                    clientContext.ExecuteQuery();

                }





                // updated latest task id to FMR list for viewing
                //http://52.172.200.35:2020/sppipapidevtesting/api/SupliPipflow/spgetTaskDetails?listname&taskuser=&ReleatedItems=82&status=not started
                /*
                    if (SPFmrID != null && SPFmrID != "" && AssignedTo != null && AssignedTo != "")
                    {
                        isWait = true;
                        getLatestTaskIDByFMRNO(string.Format(SITE_API_URL + "/api/SupliPipflow/spgetTaskDetails?listname&taskuser={0}&ReleatedItems={1}&status=not started", AssignedTo, SPFmrID));
                    }
                    */
                if (strcallbackurl != null && strcallbackurl != "")
                {
                    string strResp = ClsGeneral.DoWebGetRequest(strcallbackurl.Replace("~", "&"), "");
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

            //http://52.172.200.35:2020/sppipapidevtesting/api/SupliPipflow/spgetTaskDetails?listname&taskuser=&ReleatedItems=82&status=not started
            // http://sharepoint2/sites/teamsiteex/pipflowsitetesting/_api/web/lists/getbytitle('Workflow%20Tasks')/items?$top=1&$orderby=Id%20desc
            if (isWait)
            {
                int i_Taskwaittime = 10000;
                if (ConfigurationManager.AppSettings["TASK_WAIT_TIME"] != null)
                    i_Taskwaittime = int.Parse(ConfigurationManager.AppSettings["TASK_WAIT_TIME"]);
                Thread.Sleep(i_Taskwaittime);

            }

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
               spsetAddorupdteItemByID("", cPipflowListName, "","",strRelID,"",strTaskID);*/
            List<pipflow> pipfs = JsonConvert.DeserializeObject<List<pipflow>>(strResp);
            foreach (pipflow pipf in pipfs)
            {
                spsetAddorupdteItemByID("", cPipflowListName, "", "", pipf.RelatedItems, "", pipf.id); break;
            }
            return;

        }
        // below for workflow task assign and reject and others tagas 
        [Route("api/SupliPipflow/spsetAddorupdteItemByID")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spsetAddorupdteItemByID(string status, string Listname, string Comments, string createdby, string itemid, string keyvalue, string Taskid, string AssignTo = "", string CurAssignTo = "")
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
                        clientContext.ExecuteQuery();
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
                clientContext.ExecuteQuery();
            }
            catch (Exception ex)
            {

                return getErrormessage(ex.Message);
            }
            return getSuccessmessage("Success");

        }


        [Route("api/SupliPipflow/spgetWFEventDetailsByUser")]
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
                List list = web.Lists.GetByTitle(cPipdeptListName);

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

        public List<pipflow> getWFSHistoryTasks(ref List<pipflow> respWHmsg, string ReleatedItems, string Taskuser, string status, string TaskType)
        {
            try
            {
                CamlQuery camlQuery = new CamlQuery();
                //camlQuery.ViewXml = "<View><RowLimit>10000</RowLimit></View>";
                camlQuery.ViewXml = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq></Where></Query></View>", status);
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

                Web web = clientContext.Web;
                clientContext.Load(web);
                //  var tasks;
                // clientContext.Load(tasks, c => c.Where(t => t.Parent != null && t.Parent.Id == parentId));
                List list = web.Lists.GetByTitle(cWfSHListName);
                ListItemCollection olists = list.GetItems(camlQuery);
                clientContext.Load(olists,
                     items => items.Include(
                        item => item.Id,
                        item => item["Title"],
                        item => item["TaskOutcome"],
                        item => item["Created"],
                        item => item["relateditem"],
                        item => item["Assigned_x0020_To"],
                        item => item["Modified"],
                        item => item["Status"],
                        item => item["event"],
                        item => item["approveduser"],
                        item => item["comments"],
                        item => item["roleid"],
                        item => item["stateid"],
                        item => item["areviewuser"],
                        item => item["taskid"],
                        item => item["tasktype"]));
                clientContext.ExecuteQuery();
                //List<pipflow> respmsg = new List<pipflow>();

                foreach (ListItem oListItem in olists)
                {

                    FieldUserValue fuvAssignedTo = null;
                    FieldUserValue fuvapproveduser = null;
                    FieldUserValue fuvareviewuser = null;

                    //FieldUserValue fuvstatus = null;
                    //FieldUserValue fuvtasktype = null;



                    if (oListItem["Assigned_x0020_To"] != null)
                        foreach (FieldUserValue userValue in oListItem["Assigned_x0020_To"] as FieldUserValue[])
                        {
                            fuvAssignedTo = userValue;
                        }
                    if (oListItem["approveduser"] != null)
                        foreach (FieldUserValue userValue in oListItem["approveduser"] as FieldUserValue[])
                        {
                            fuvapproveduser = userValue;
                        }
                    if (oListItem["areviewuser"] != null)
                        foreach (FieldUserValue userValue in oListItem["areviewuser"] as FieldUserValue[])
                        {
                            //string test = userValue.LookupId;
                            fuvareviewuser = userValue;
                        }
                    if (oListItem["Assigned_x0020_To"] != null && Taskuser != "" && (TaskType == "" || TaskType == "1"))
                        if (replaceExtraLoginNameContent(fuvAssignedTo.LookupValue.ToLower()) != Taskuser.ToLower()) continue;

                    string RelItem = "";
                    if (oListItem["relateditem"] != null && oListItem["relateditem"].ToString() != "")
                    {
                        //if (oListItem["relateditem"].ToString().Length == 1)
                        //{
                        RelItem = oListItem["relateditem"].ToString();
                        //}
                        //else
                        //{
                        //    List<RelatedItemFieldValue> obj = JsonConvert.DeserializeObject<List<RelatedItemFieldValue>>(oListItem["relateditem"].ToString());
                        //    if (obj != null)
                        //        RelItem = obj[0].ItemId.ToString();
                        //}
                    }

                    respWHmsg.Add(new pipflow
                    {


                        id = (oListItem["taskid"] != null) ? oListItem["taskid"].ToString() : "",
                        title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                        taskoutcome = (oListItem["TaskOutcome"] != null) ? oListItem["TaskOutcome"].ToString() : "",
                        RelatedItems = (oListItem["relateditem"] != null) ? RelItem : "",
                        status = (oListItem["Status"] != null) ? oListItem["Status"].ToString() : "",
                        assigned_to = (oListItem["Assigned_x0020_To"] != null) ? replaceExtraLoginNameContent(fuvAssignedTo.LookupValue) : "",
                        //assigned_to_id = (oListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupValue: "",
                        //approeduser_to = (oListItem["approveduser"] != null) ? replaceExtraLoginNameContent(fuvapproveduser.LookupValue) : "",
                        approveduser_to_id = (oListItem["approveduser"] != null) ? fuvapproveduser.LookupValue : "",
                        areviewuser_to = (oListItem["areviewuser"] != null) ? replaceExtraLoginNameContent(fuvareviewuser.LookupValue) : "",
                        // event1 = (oListItem["event"] != null) ? oListItem["event"].ToString() : "",
                        tasktype = (oListItem["tasktype"] != null) ? oListItem["tasktype"].ToString() : "",
                        Created = (oListItem["Created"] != null) ? oListItem["Created"].ToString() : "",
                        stateid = (oListItem["stateid"] != null) ? oListItem["stateid"].ToString() : "",
                        roleid = (oListItem["roleid"] != null) ? oListItem["roleid"].ToString() : "",
                        Modified = (oListItem["Modified"] != null) ? oListItem["Modified"].ToString() : ""
                        // comments = (oListItem["comments"] != null) ? oListItem["comments"].ToString() : "",
                        // taskid= (oListItem["taskid"] != null) ? oListItem["taskid"].ToString() : "",
                    });
                }
                return respWHmsg;
            }

            catch (Exception ex)
            {
                string var = ex.ToString();
            }
            return null;
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
        [Route("api/SupliPipflow/spgetTaskDetailsByuser")]
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


                List list = web.Lists.GetByTitle(cWfListName);


                ListItem targetListItem = list.GetItemById(ListitemId);
                clientContext.Load(list);
                clientContext.Load(targetListItem);
                clientContext.ExecuteQuery();
                // GetDoucmentHistory(pathValue, cWfListName,int.Parse(ListitemId));
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

        [Route("api/SupliPipflow/spgetTaskDetails")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spgetTaskDetails(string Listname, string TaskType, string stateid,string sid,string roleid = "", string Taskuser = null, string ReleatedItems = null, string status = "")
        {
            // prepare site connection
            try
            {
                if (ClsGeneral.getConfigvalue("FROM_SSOM_URL") != "")
                    return getHttpResponseMessage(ClsGeneral.DoWebGetRequest(ClsGeneral.getConfigvalue("FROM_SSOM_URL") + "/api/Pipflow/spgetTaskDetails" + ControllerContext.Request.RequestUri.Query.ToString(), ""));

                if (TaskType == null) TaskType = "";
                if (roleid == null) roleid = "";
                if (stateid == null) stateid = "";
                if (status == null) status = "";
                // global parameters
                Taskuser = Taskuser == null ? "" : Taskuser;
                ReleatedItems = ReleatedItems == null ? "" : "," + ReleatedItems + ",";

                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = "<View><RowLimit>50000</RowLimit></View>";
                // camlQuery.ViewXml = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='ParentID'/><Value Type='Counter'>569</Value></Eq></Where></Query></View>", Taskuser);

                //< View Scope = "RecursiveAll" >< Query >< Where >< Eq >< FieldRefName = "ParentID" />< ValueType = "Counter" > 1 </ Value ></ Eq ></ Where ></ Query ></ View >
                // camlQuery.ViewXml = "<View><Where><Eq><FieldRef Name='AssignedTo' /><Value Type='User'>" + Taskuser + "</Value></Eq></Where></View>";
                //camlQuery.ViewXml = "<Where><And><Or><Membership Type='CurrentUserGroups'><FieldRef Name='AssignedTo' /></Membership><Eq><FieldRef Name='AssignedTo'  LookupId='TRUE' /><Value Type='Lookup'>27</Value></Eq></Or><Neq><FieldRef Name='Status' /><Value Type='Text'>Completed</Value></Neq></And></Where>";

                //camlQuery. = "<Where><Or><Eq><FieldRef Name='AssignedTo' LookupId='TRUE'/><Value Type='Integer'><UserID /></Value></Eq><Membership Type='CurrentUserGroups'><FieldRef Name='AssignedTo'/></Membership></Or></Where>";
                // prepare site connection
                camlQuery.ViewXml = "<View><RowLimit>50000</RowLimit></View>";

                camlQuery.ViewXml = "<View><RowLimit>500000</RowLimit><Query><Where><And>";
               // if (roleid != "")
                    camlQuery.ViewXml += "<And>";
                camlQuery.ViewXml += "<Eq><FieldRef Name='tasktype' /><Value Type='Number'>" + TaskType + "</Value></Eq>" +
                                    "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq>";
               // if (roleid != "")
                    camlQuery.ViewXml += "</And>";

                if (roleid != "" && sid != "")
                {
                    camlQuery.ViewXml += "<And>";
                    camlQuery.ViewXml += "<Eq><FieldRef Name='sid' /><Value Type='Number'>" + sid + "</Value></Eq>";
                    camlQuery.ViewXml += "<Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";
                    camlQuery.ViewXml += "</And>";
                }
                else if (roleid != "")
                   camlQuery.ViewXml += "<Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";

                else if(sid!="")
                    camlQuery.ViewXml += "<Eq><FieldRef Name='sid' /><Value Type='Number'>" + sid + "</Value></Eq>";

                camlQuery.ViewXml += "</And></Where></Query></View>";
                List<pipflow> respmsg = new List<pipflow>();
                if (ConfigurationManager.AppSettings["IS_SINGLE_TASK"] != null && ConfigurationManager.AppSettings["IS_SINGLE_TASK"].ToString().ToUpper() == "Y")
                {
                    //  getWFHistoryTasks(ref respmsg, ReleatedItems, Taskuser, status, TaskType);
                }
                ClientContext clientContext = new ClientContext(strSiteURL);
                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

                Web web = clientContext.Web;
                clientContext.Load(web);
                //  var tasks;
                // clientContext.Load(tasks, c => c.Where(t => t.Parent != null && t.Parent.Id == parentId));
                List list = web.Lists.GetByTitle(cWfListName);
                ListItemCollection olists = list.GetItems(camlQuery);
                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(olists,
                     items => items.Include(
                        item => item.Id,
                        item => item["Title"],
                        item => item["TaskOutcome"],
                         item => item["relateditem"],
                           item => item["Created"],
                             item => item["Modified"],
                        item => item["Status"],
                        item => item["Assigned_x0020_To"],
                        item => item["approveduser"],
                        item => item["areviewuser"],
                        item => item["Editor"],
                        item => item["Author"],
                         item => item["tasktype"],
                         item => item["roleid"],
                         item => item["stateid"],
                         item => item["sid"],
                         item => item["ID"]
                        ));
                clientContext.ExecuteQuery();


                foreach (ListItem oListItem in olists)
                {
                    /* if (TaskType != "" && TaskType != "1")
                     {
                         getSubTasks(ref respmsg, ReleatedItems, Taskuser, status, TaskType);
                         break;
                     }*/
                    // create and cast the FieldUserValue from the value
                    FieldUserValue fuvAssignedTo = null;
                    FieldUserValue fuvapproveduser = null;
                    FieldUserValue fuvareviewuser = null;
                    FieldUserValue fuvEditor = null;
                    FieldUserValue fuvAuthor = null;

                    if (oListItem["Assigned_x0020_To"] != null)
                        foreach (FieldUserValue userValue in oListItem["Assigned_x0020_To"] as FieldUserValue[])
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
                    if (oListItem["Assigned_x0020_To"] != null && Taskuser != "" && (TaskType == "" || TaskType == "1"))
                        if (replaceExtraLoginNameContent(fuvAssignedTo.LookupValue.ToLower()) != Taskuser.ToLower()) continue;

                    if (oListItem["Editor"] != null)
                        fuvEditor = (FieldUserValue)oListItem["Editor"];
                    if (oListItem["Author"] != null)
                        fuvAuthor = (FieldUserValue)oListItem["Author"];
                    string RelItem = "";
                    if (oListItem["relateditem"] != null && oListItem["relateditem"].ToString() != "")
                    {


                        RelItem = oListItem["relateditem"].ToString();
                    }

                    // related item to for listing the data filtering
                    if (ReleatedItems != "" && ReleatedItems.Contains("," + RelItem + ",") != true)
                        continue;


                    if (status != "" && oListItem["Status"].ToString().ToLower() != status.ToLower()) continue;
                    if (TaskType != "" && oListItem["tasktype"] != null && oListItem["tasktype"].ToString().ToLower() != TaskType.ToLower()) continue;
                    if (stateid != "" && oListItem["stateid"] != null && oListItem["stateid"].ToString().ToLower() != stateid.ToLower()) continue;
                    if (roleid != "" && oListItem["roleid"] != null && oListItem["roleid"].ToString().ToLower() != roleid.ToLower()) continue;

                    respmsg.Add(new pipflow
                    {
                       
                        id = (oListItem["ID"] != null) ? oListItem["ID"].ToString() : "",
                        title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                        taskoutcome = (oListItem["TaskOutcome"] != null) ? oListItem["TaskOutcome"].ToString() : "",
                        RelatedItems = (oListItem["relateditem"] != null) ? RelItem : "",
                        status = (oListItem["Status"] != null) ? oListItem["Status"].ToString() : "",
                        assigned_to = (oListItem["Assigned_x0020_To"] != null) ? replaceExtraLoginNameContent(fuvAssignedTo.LookupValue) : "",
                         approveduser_to = (oListItem["approveduser"] != null) ? fuvapproveduser.LookupValue : "",
                        areviewuser_to = (oListItem["areviewuser"] != null) ? replaceExtraLoginNameContent(fuvareviewuser.LookupValue) : "",
                        tasktype = (oListItem["tasktype"] != null) ? oListItem["tasktype"].ToString() : "",
                        stateid = (oListItem["stateid"] != null) ? oListItem["stateid"].ToString() : "",
                        roleid = (oListItem["roleid"] != null) ? oListItem["roleid"].ToString() : "",
                        sid = (oListItem["sid"] != null) ? oListItem["sid"].ToString() : "",
                        Created = (oListItem["Created"] != null) ? oListItem["Created"].ToString() : "",
                        Modified = (oListItem["Modified"] != null) ? oListItem["Modified"].ToString() : ""
                    });



                }

                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }


        }

        private string replaceExtraLoginNameContent(string _loginname)
        {
            if (_loginname.EndsWith("snc") && _loginname.Contains(",#"))
                return _loginname.Replace(_loginname, "snc");
            else if (_loginname.Contains(",#"))
                return _loginname.Replace(",#", "");
            else
                return _loginname;
        }
        private void getSubTasks(ref List<pipflow> respmsg, string ReleatedItems, string Taskuser, string status, string TaskType)
        {
            CamlQuery camlQuery = new CamlQuery();
            //camlQuery.ViewXml = "<View><RowLimit>1000</RowLimit></View>";
            // camlQuery.ViewXml = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='ParentID'/><Value Type='Counter'>{0}</Value></Eq></Where></Query></View>", _parentID);
            camlQuery.ViewXml = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='tasktype'/><Value Type='Number'>{0}</Value></Eq></Where></Query></View>", TaskType);
            //camlQuery.ViewXml = string.Format("<View Scope='RecursiveAll'></View>", _parentID);
            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

            Web web = clientContext.Web;
            clientContext.Load(web);
            //  var tasks;
            // clientContext.Load(tasks, c => c.Where(t => t.Parent != null && t.Parent.Id == parentId));
            List list = web.Lists.GetByTitle(cWfListName);
            ListItemCollection olists = list.GetItems(camlQuery);
            // Console.WriteLine("List ID::  " + list.Id);
            clientContext.Load(olists,
                 items => items.Include(
                    item => item.Id,
                    item => item["Title"],
                    item => item["TaskOutcome"],
                     item => item["RelatedItems"],
                     item => item["relateditem"],
                    item => item["Status"],
                    item => item["AssignedTo"],
                    item => item["approveduser"],
                    item => item["areviewuser"],
                    item => item["Editor"],
                     item => item["roleid"],
                         item => item["stateid"],
                    item => item["Author"],
                     item => item["tasktype"],
                      item => item["ParentID"]));
            clientContext.ExecuteQuery();
            foreach (ListItem oListItem in olists)
            {

                //  if (oListItem["tasktype"] != null && oListItem["tasktype"].ToString() != "" && TaskType != oListItem["tasktype"].ToString()) continue;
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

                if (oListItem["relateditem"] != null && ReleatedItems != "" && ReleatedItems.Contains("," + oListItem["relateditem"].ToString() + ",") != true)
                    continue;


                if (status != "" && oListItem["Status"].ToString().ToLower() != status.ToLower()) continue;

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

                if (oListItem["areviewuser"] == null)
                    continue;
                if (oListItem["areviewuser"] != null && Taskuser != "")
                    if (fuvareviewuser.LookupValue.ToLower() != Taskuser.ToLower()) continue;

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
                    RelatedItems = (oListItem["relateditem"] != null) ? oListItem["relateditem"].ToString() : "",
                    status = (oListItem["Status"] != null) ? oListItem["Status"].ToString() : "",
                    assigned_to = (oListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupValue : "",
                    assigned_to_id = (oListItem["AssignedTo"] != null) ? fuvAssignedTo.LookupId.ToString() : "",
                    approveduser_to = (oListItem["approveduser"] != null) ? fuvapproveduser.LookupValue : "",
                    approveduser_to_id = (oListItem["approveduser"] != null) ? fuvapproveduser.LookupId.ToString() : "",
                    areviewuser_to = (oListItem["areviewuser"] != null) ? fuvareviewuser.LookupValue : "",
                    areviewuser_to_id = (oListItem["areviewuser"] != null) ? fuvareviewuser.LookupId.ToString() : "",
                    Modified_By = (oListItem["Editor"] != null) ? fuvEditor.LookupValue : "",
                    Modified_By_id = (oListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                    roleid = (oListItem["roleid"] != null) ? oListItem["roleid"].ToString() : "",
                    stateid = (oListItem["stateid"] != null) ? oListItem["stateid"].ToString() : "",
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


        [System.Web.Http.Route("api/SupliPipflow/ADAddUser")]
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
                if (result.Result.Contains("already exists"))
                    return getHttpResponseMessage("already exists");
                else
                    return getHttpResponseMessage("success");
            }
            //return getHttpResponseMessage(result.ToString());
        }



        // start user active directory calls to servers
        //[System.Web.Http.Route("api/SupliPipflow/ADAddUser")]
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
        [System.Web.Http.Route("api/SupliPipflow/getADUsers")]
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
        [System.Web.Http.Route("api/SupliPipflow/ADUpdateUser")]
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

        /*public async Task<string> WebServiceAsync(int id)
        {
            var client = new HttpClient
            {
                BaseAddress =
                new Uri(WebServiceAddress)
            };
            string json = await client.GetStringAsync(id.ToString());
            var result = JsonConvert.DeserializeObject<string>(json);
            return result;
        }
       
        [System.Web.Http.Route("api/SupliPipflow/AsyncBulkPushAPIS")]
        [System.Web.Http.HttpPost]
        public async Task<HttpResponseMessage>  AsyncBulkPushAPIS(List<BulkpushAPIS> models)
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
                    oItem["callbackurl"] = BulkAPI.callbackurl;
                    oItem.Update();
                    //clientContext.Load(oItem);
                    
                    clientContext.ExecuteQuery();
                }
                catch (Exception ex)
                {

                    return getErrormessage(ex.Message);
                }

            }
            return await getSuccessmessage("Success");
        }*/
        [System.Web.Http.Route("api/SupliPipflow/VersionList")]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage VersionList()
        {
            string strData = "";
            using (ClientContext ctx = new ClientContext(strSiteURL))
            {
                CamlQuery camlQuery = new CamlQuery();
                Web web = ctx.Web;
                ctx.Load(web, w => w.ServerRelativeUrl, w => w.Lists);
                List list = web.Lists.GetByTitle(cPipflowListName);
                ctx.Load(list);
                ListItemCollection itemColl = list.GetItems(camlQuery);
                ctx.Load(itemColl);
                ctx.ExecuteQuery();
                foreach (ListItem item in itemColl)
                {
                    SP.File fileVersion = web.GetFileByServerRelativeUrl(web.ServerRelativeUrl + "/Lists/" + list.Title + "/" + item.Id + "_.000");
                    ctx.Load(fileVersion);
                    SP.FileVersionCollection fileVersionCollection = fileVersion.Versions;
                    ctx.Load(fileVersionCollection);
                    ctx.ExecuteQuery();
                    foreach (var version in fileVersion.Versions)
                    {
                        string versionValue = string.Empty;
                        /*  if (version.FieldValues["FieldName"] != null)
                          {
                               versionValue = version.FieldValues["FieldName"].ToString();
                          }*/
                    }


                    for (int iVersionCount = 0; iVersionCount < fileVersionCollection.Count; iVersionCount++)
                    {
                        SP.FileVersion version = fileVersionCollection[iVersionCount];
                        //  strData += version.ToString();

                        // strData += version.CreatedBy.Title;
                        strData += version.CheckInComment;
                        strData += version.Created.ToShortDateString() + " " +
                                         version.Created.ToShortTimeString();
                        strData += item.Id.ToString();
                        strData += version.VersionLabel;
                        strData += version.IsCurrentVersion;
                        // strData += version.SPFieldValues["Title"].ToString();
                        //fileVersionCollection.DeleteByID(version.ID);
                        //  ListItem oldItem = version.attr;
                        // ctx.Load(oldItem);
                        // ctx.ExecuteQuery();
                    }
                    ctx.ExecuteQuery();
                }
            }
            return getSuccessmessage(strData);
        }

        [System.Web.Http.Route("api/SupliPipflow/BulkPushAPIS")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage BulkPushAPIS(List<BulkpushAPIS> models)
        {


            if (ClsGeneral.getConfigvalue("FROM_SSOM_URL_INSERT") != "")
                return getHttpResponseMessage(ClsGeneral.DoPostWebreqeust(ClsGeneral.getConfigvalue("FROM_SSOM_URL") + "/api/Pipflow/BulkPushAPIS" + ControllerContext.Request.RequestUri.Query.ToString(), JsonConvert.SerializeObject(models)));


            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential(strUSER, strPWD);

            //Get the list items from list
            SP.List oList = clientContext.Web.Lists.GetByTitle("sbulkpushapis");
            ListItemCreationInformation oListItemCreationInformation = new ListItemCreationInformation();


            foreach (BulkpushAPIS BulkAPI in models)
            {
                // prepare site connection               

                try
                {
                    ListItem oItem = oList.AddItem(oListItemCreationInformation);
                    oItem["Title"] = BulkAPI.Title;
                    oItem["pushurl"] = BulkAPI.url;
                    string stateid = "0", roleid = "0";
                    if (ClsGeneral.getConfigvalue("REQESTFROM_API").ToUpper() != "Y")
                    {

                        var uri = new Uri(BulkAPI.url);
                        var query = HttpUtility.ParseQueryString(uri.Query);
                        dynamic QueryParams, QueryParam;
                        QueryParams = JArray.Parse(ClsGeneral.GetJsonStringFromQueryString(query.ToString().ToLower()));

                        QueryParam = QueryParams[0];

                        if (QueryParam.stateid != null) stateid = QueryParam.stateid.Value;

                        if (QueryParam.roleid != null) roleid = QueryParam.roleid.Value;

                        oItem["stateid"] = stateid;

                        if (BulkAPI.url.ToString().ToLower().Contains("/suplipipflow/spsetfmr?"))
                        {
                            oItem["status"] = "-3";
                        }
                        else if (BulkAPI.url.ToString().ToLower().Contains("/suplipipflow/spsettaskitembyid?"))
                        {
                            oItem["status"] = "-4";
                        }
                        else if (BulkAPI.url.ToString().ToLower().Contains("/pipflow/spsetfmr?"))
                        {
                            oItem["status"] = "-1";
                        }
                        else if (BulkAPI.url.ToString().ToLower().Contains("/pipflow/spsettaskitembyid?"))
                        {
                            oItem["status"] = "-2";
                        }// below two loop for suplimentary insertion to 

                        else // direct web request all apps
                        {
                            oItem["status"] = "-6";
                            oItem["log"] = "direct call";
                            if (oItem["roleid"] != null) oItem["roleid"] = roleid;
                        }
                    }
                    //oItem["callbackurl"] = BulkAPI.callbackurl;
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

        [Route("api/SupliPipflow/spgetBulkpushDetails")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spgetBulkpushDetails(string stateid = "", string status = "", string roleid = "", string sid = "")
        {

            // prepare site connection
            try
            {
                // global parameters<View><Query>
                if (ClsGeneral.getConfigvalue("FROM_SSOM_URL") != "")
                    return getHttpResponseMessage(ClsGeneral.DoWebGetRequest(ClsGeneral.getConfigvalue("FROM_SSOM_URL") + "/api/Pipflow/spgetBulkpushDetails" + ControllerContext.Request.RequestUri.Query.ToString(), ""));


                string strCamlQuery_temp = "<View><Query><Where><And>!WHERE!</And></Where></Query></View>";
                string strWhereText_temp = "<Eq><FieldRef Name='!NAME!'/><Value Type='!TYPE!'>!VALUE!</Value></Eq>";
                // string strWhereText_temp_withoutAND = "<Eq><FieldRef Name='!NAME!'/><Value Type='!TYPE!'>!VALUE!</Value></Eq>";
                string strCamlQuery = "";
                int serarchCount = 0;

                // if (1 == 1) strCamlQuery += strWhereText_temp.Replace("!NAME!", "1").Replace("!TYPE!", "Text").Replace("!VALUE!", "1");
                if (stateid != null && stateid != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "stateid").Replace("!TYPE!", "Number").Replace("!VALUE!", stateid); } else stateid = "";
                if (roleid != null && roleid != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "roleid").Replace("!TYPE!", "Number").Replace("!VALUE!", roleid); } else roleid = "";
                if (status != null && status != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "status").Replace("!TYPE!", "Number").Replace("!VALUE!", status); } else status = "";
                if (sid != null && sid != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "sid").Replace("!TYPE!", "Number").Replace("!VALUE!", sid); } else sid = "";


                CamlQuery camlQuery = new CamlQuery();
                /* old camlQuery.ViewXml = "<View><RowLimit>10000</RowLimit></View>";
                camlQuery.ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Choice'>Not Started</Value></Eq>"
                                              + "<Eq><FieldRef Name='AssignedTo'/><Value Type='UserMulti'>{0}</Value></Eq><Contains><FieldRef Name='RelatedItems'/><Value Type='Text'>:{1},</Value></Contains>");
                   + "</Where></Query></View>", Userid.TrimEnd(','), FMRID);*/

                strCamlQuery = "<View><RowLimit>100000</RowLimit></View>";

                if (serarchCount != 0)
                {
                    strCamlQuery = strCamlQuery_temp.Replace("!WHERE!", strCamlQuery);
                    if (serarchCount == 1) strCamlQuery = strCamlQuery.Replace("<And>", "").Replace("</And>", "");
                    // if (serarchCount > 2) strCamlQuery = strCamlQuery.Replace("<And>", "").Replace("</And>", "");
                    //  if (serarchCount == 0)
                    if (status != "" && stateid != "" && roleid != "" && sid!="")
                        camlQuery.ViewXml = "<View><RowLimit>500000</RowLimit><Query><Where><And>" +
                                      "<And><Eq><FieldRef Name='status' /><Value Type='Number'>" + status + "</Value></Eq>" +
                                      "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq></And>"+
                                      "<And><Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";
                    else if (status != "" && stateid != "" && roleid != "")
                        camlQuery.ViewXml = "<View><RowLimit>500000</RowLimit><Query><Where><And>" +
                                      "<And><Eq><FieldRef Name='status' /><Value Type='Number'>" + status + "</Value></Eq>" +
                                      "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq></And>";
                    else if (status != "" && stateid != "")
                        camlQuery.ViewXml = "<View><RowLimit>500000</RowLimit><Query><Where><And>" +
                                      "<Eq><FieldRef Name='status' /><Value Type='Number'>" + status + "</Value></Eq>" +
                                      "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq>";
                    else if (stateid != "" && roleid != "")
                        camlQuery.ViewXml = "<View><RowLimit>500000</RowLimit><Query><Where><And>" +
                                      "<Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>" +
                                      "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq>";
                    else if (status != "")
                        camlQuery.ViewXml = "<View><RowLimit>500000</RowLimit><Query><Where>" +
                             "<Eq><FieldRef Name='status' /><Value Type='Number'>" + status + "</Value></Eq>";

                    else if (stateid != "")
                        camlQuery.ViewXml = "<View><RowLimit>500000</RowLimit><Query><Where>" +
                             "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq>";
                    else if (roleid != "")
                        camlQuery.ViewXml = "<View><RowLimit>500000</RowLimit><Query><Where>" +
                             "<Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";

                    if (status != "" && stateid != "" && roleid != "" && sid != "")
                           camlQuery.ViewXml += "<Eq><FieldRef Name='sid' /><Value Type='Number'>" + sid + "</Value></Eq></And>";
                    else if (status != "" && stateid != "" && roleid != "")
                        camlQuery.ViewXml += "<Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";
                    if (serarchCount > 1)
                        camlQuery.ViewXml += "</And></Where></Query></View>";
                    else
                        camlQuery.ViewXml += "</Where></Query></View>";
                }



                ClientContext clientContext = new ClientContext(strSiteURL);

                clientContext.Credentials = new NetworkCredential(strUSER, strPWD);
                Web web = clientContext.Web;
                clientContext.Load(web);
                User user = clientContext.Web.CurrentUser;
                clientContext.Load(user);
                clientContext.ExecuteQuery();

                List list = web.Lists.GetByTitle(cBUlkPushListName);

                ListItemCollection olists;


                olists = list.GetItems(camlQuery);
                // Console.WriteLine("List ID::  " + list.Id);
                clientContext.Load(olists);
                clientContext.ExecuteQuery();
                List<BulkpushAPIDetails> respmsg = new List<BulkpushAPIDetails>();




                foreach (ListItem oListItem in olists)
                {
                    try
                    {

                        respmsg.Add(new BulkpushAPIDetails
                        {
                            id = oListItem.Id.ToString(),
                            Title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                            status = (oListItem["status"] != null) ? oListItem["status"].ToString() : "",
                            stateid = (oListItem["stateid"] != null) ? oListItem["stateid"].ToString() : "",
                            roleid = (oListItem["roleid"] != null) ? oListItem["roleid"].ToString() : "",
                            pushurl = (oListItem["pushurl"] != null) ? oListItem["pushurl"].ToString() : "",
                            log = (oListItem["log"] != null) ? oListItem["log"].ToString() : "",
                            Created = (oListItem["Created"] != null) ? oListItem["Created"].ToString() : "",
                            Modified = (oListItem["Modified"] != null) ? oListItem["Modified"].ToString() : ""

                        });
                    }
                    catch
                    {


                    }


                }

                return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));
            }
            catch (Exception ex)
            {


                return getErrormessage(ex.Message);
            }



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
