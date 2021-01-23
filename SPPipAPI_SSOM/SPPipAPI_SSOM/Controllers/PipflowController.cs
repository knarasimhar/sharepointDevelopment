
using Newtonsoft.Json;
using SPPipAPI_SSOM.Models;
using System;
using System.Collections.Generic;

using System.Net;
using System.Net.Http;

using System.Text;
using System.Web;
using System.Web.Http;
using System.Configuration;
using System.Security;

using System.Web.Http.Cors;

using Newtonsoft.Json.Linq;
using System.Collections;
using Microsoft.SharePoint;

namespace SPPipAPI_SSOM.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class PipflowController : ApiController
    {
        // GET api/values/5
        String strSiteURL = "http://sharepoint2/sites/teamsiteex/PipFlowSite", strUSER = "spuser2", strPWD = "User@123#", strADUserURL = "", SitepathValue = "";
        string SITE_API_URL = "";
        string strDomainName = ConfigurationManager.AppSettings["DomainName"].ToString();
        Boolean isWait = false;

        string cPipflowListName = "pipflow1";
        string cPipdeptListName = "pipdept";
        string cWfListName = "workflow_history";
        string cWfHListName = "workflow_history";
        string cBulkListName = "bulkpushapis";



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

        }



        [Route("api/Pipflow/spgetListByName")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spgetListByName(string Listname, string FY, string fmrtype, string stateid, string roleid = "", string status = "", string ListitemId = "")
        {

            // prepare site connection
            try
            {


                List<fmrlist> respmsg = null;
                using (SPSite site = new SPSite(strSiteURL))
                {


                    using (SPWeb web = site.OpenWeb())
                    {
                        // string strWhereText_temp_withoutAND = "<Eq><FieldRef Name='!NAME!'/><Value Type='!TYPE!'>!VALUE!</Value></Eq>";

                        if (status == null) status = "";


                        SPQuery camlQuery = new SPQuery();

                      /*  SPquery.Query = "<Where><And>" +
                              "<And><Eq><FieldRef Name='fmrtype' /><Value Type='Number'>" + fmrtype + "</Value></Eq>" +
                              "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq></And>";
                        if (roleid != "")
                            SPquery.Query += "<And><Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";
                        SPquery.Query += "<Eq><FieldRef Name='FY' /><Value Type='Text'>" + FY + "</Value></Eq>";
                        if (roleid != "")
                            SPquery.Query += "</And>";
                        SPquery.Query += "</And></Where>";*/

                        string strCamlQuery_temp = "<Where><And>!WHERE!</And></Where></Query></View>";
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



                       
                        /* old camlQuery.ViewXml = "<View><RowLimit>10000</RowLimit></View>";
                        camlQuery.ViewXml = string.Format("<View><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Choice'>Not Started</Value></Eq>"
                                                      + "<Eq><FieldRef Name='AssignedTo'/><Value Type='UserMulti'>{0}</Value></Eq><Contains><FieldRef Name='RelatedItems'/><Value Type='Text'>:{1},</Value></Contains>");
                           + "</Where></Query></View>", Userid.TrimEnd(','), FMRID);*/
                        strCamlQuery = strCamlQuery_temp.Replace("!WHERE!", strCamlQuery);
                        if (serarchCount == 1) strCamlQuery = strCamlQuery.Replace("<And>", "").Replace("</And>", "");
                        // if (serarchCount > 2) strCamlQuery = strCamlQuery.Replace("<And>", "").Replace("</And>", "");
                        //  if (serarchCount == 0)
                        strCamlQuery = "<View><RowLimit>100000</RowLimit></View>";

                        camlQuery.ViewXml = "<Where><And>" +
                                      "<And><Eq><FieldRef Name='fmrtype' /><Value Type='Number'>" + fmrtype + "</Value></Eq>" +
                                      "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq></And>";
                        if (roleid != "")
                            camlQuery.ViewXml += "<And><Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";
                        camlQuery.ViewXml += "<Eq><FieldRef Name='FY' /><Value Type='Text'>" + FY + "</Value></Eq>";
                        if (roleid != "")
                            camlQuery.ViewXml += "</And>";
                        camlQuery.ViewXml += "</And></Where>";

                        SPListItemCollection olists = web.Lists[cPipflowListName].GetItems(camlQuery);

                        respmsg = new List<fmrlist>();

                        foreach (SPListItem oListItem in olists)
                        {
                            string strLookupValues = "", strLookupAssignTOIds = "", strLookupAssignTOvalues = "";
                            try
                            {

                                if (status != "" && oListItem["status"].ToString().ToLower() != status.ToLower())
                                    continue;
                                if (fmrtype != "" && oListItem["fmrtype"].ToString().ToLower() != fmrtype.ToLower())
                                    continue;
                                if (stateid != "" && oListItem["stateid"].ToString().ToLower() != stateid.ToLower())
                                    continue;

                                if (FY != "" && oListItem["FY"].ToString().ToLower() != FY.ToLower())
                                    continue;

                                // new code implemented for eliminate the task geranration time 
                                //  string currentTaskID = getCurrentTaskIDofFMR(oListItem.Id.ToString(), strLookupAssignTOIds);
                                respmsg.Add(new fmrlist
                                {
                                    id = oListItem.ID.ToString(),
                                    title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                                    assigned_to = (oListItem["ry3a"] != null) ? strLookupAssignTOvalues.TrimEnd(',') : "",
                                    assigned_to_id = (oListItem["ry3a"] != null) ? strLookupAssignTOIds.TrimEnd(',') : "",
                                    status = (oListItem["status"] != null) ? oListItem["status"].ToString() : "",
                                    fmrtype = (oListItem["fmrtype"] != null) ? oListItem["fmrtype"].ToString() : "",
                                    stateid = (oListItem["stateid"] != null) ? oListItem["stateid"].ToString() : "",
                                    roleid = (oListItem["roleid"] != null) ? oListItem["roleid"].ToString() : "",
                                    fy = (oListItem["FY"] != null) ? oListItem["FY"].ToString() : "",
                                    remarks = (oListItem["remarks"] != null) ? oListItem["remarks"].ToString() : "",
                                    currenttaskid = (oListItem["currenttaskid"] != null) ? oListItem["currenttaskid"].ToString() : "",
                                    Modified_By = (oListItem["Editor"] != null) ? oListItem["Editor"].ToString() : "",
                                    //Modified_By_id = (oListItem["Editor"] != null) ? fuvEditor.LookupId.ToString() : "",
                                    Modified = (oListItem["Modified"] != null) ? oListItem["Modified"].ToString() : "",
                                    Created_By = (oListItem["Author"] != null) ? oListItem["Modified"].ToString() : "",
                                    //Created_By_id = (oListItem["Author"] != null) ? fuvAuthor.LookupId.ToString() : "",
                                    Created = (oListItem["Created"] != null) ? oListItem["Created"].ToString() : "",
                                    currentassign_to = (oListItem["currentAssignee"] != null) ? strLookupValues.TrimEnd(',') : "",
                                    //currentassign_to_id = (oListItem["currentAssignee"] != null) ? strLookupIDS.TrimEnd(',') : ""
                                });
                            }
                            catch { }
                        }


                        return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));

                    }
                }

            }
            catch (Exception ex)
            {


                return getErrormessage(ex.Message);
            }
        }

        [Route("api/Pipflow/spgetBulkpushDetails")]

        [HttpGet, HttpPost]
        public HttpResponseMessage spgetBulkpushDetails(string stateid = "", string status = "", string roleid = "")
        {

            // prepare site connection
            try
            {
                 List<BulkpushAPIDetails> respmsg = null;
                using (SPSite site = new SPSite(strSiteURL))
                {


                    using (SPWeb web = site.OpenWeb())
                    {
                        // global parameters<View><Query>
                        if (ClsGeneral.getConfigvalue("FROM_SSOM_URL") != "")
                            return getHttpResponseMessage(ClsGeneral.DoWebGetRequest(ClsGeneral.getConfigvalue("FROM_SSOM_URL") + "/api/Pipflow/spgetBulkpushDetails" + ControllerContext.Request.RequestUri.Query.ToString(), ""));


                        string strCamlQuery_temp = "<Where><And>!WHERE!</And></Where></Query></View>";
                        string strWhereText_temp = "<Eq><FieldRef Name='!NAME!'/><Value Type='!TYPE!'>!VALUE!</Value></Eq>";
                        // string strWhereText_temp_withoutAND = "<Eq><FieldRef Name='!NAME!'/><Value Type='!TYPE!'>!VALUE!</Value></Eq>";
                        string strCamlQuery = "";
                        int serarchCount = 0;

                        // if (1 == 1) strCamlQuery += strWhereText_temp.Replace("!NAME!", "1").Replace("!TYPE!", "Text").Replace("!VALUE!", "1");
                        if (stateid != null && stateid != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "stateid").Replace("!TYPE!", "Number").Replace("!VALUE!", stateid); } else stateid = "";
                        if (roleid != null && roleid != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "roleid").Replace("!TYPE!", "Number").Replace("!VALUE!", roleid); } else roleid = "";
                        if (status != null && status != "") { serarchCount++; strCamlQuery += strWhereText_temp.Replace("!NAME!", "status").Replace("!TYPE!", "Number").Replace("!VALUE!", status); } else status = "";



                        SPQuery camlQuery = new SPQuery();
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
                            if (status != "" && stateid != "" && roleid != "")
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
                            if (status != "" && stateid != "" && roleid != "")
                                camlQuery.ViewXml += "<Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";
                            if (serarchCount > 1)
                                camlQuery.ViewXml += "</And></Where></Query></View>";
                            else
                                camlQuery.ViewXml += "</Where></Query></View>";
                        }

                        SPListItemCollection olists = web.Lists[cPipflowListName].GetItems(camlQuery);
                        
                        respmsg = new List<BulkpushAPIDetails>();

                        foreach (SPListItem oListItem in olists)
                        {

                         try
                            {

                                respmsg.Add(new BulkpushAPIDetails
                                {
                                    id = oListItem.ID.ToString(),
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
                }
            }
            catch (Exception ex)
            {


                return getErrormessage(ex.Message);
            }



        }

        [System.Web.Http.Route("api/Pipflow/getGroupbyStates")]
        [System.Web.Http.HttpGet]
        public HttpResponseMessage getGroupbyStates(string status, string roleids)
        {
            // prepare site connection
            string strInRoleids = "";

            foreach (string roleid in roleids.Split(','))
            {
                strInRoleids += "<Value Type='Number'>" + roleid + "</Value>";
            }
            try
            {
                using (SPSite site = new SPSite(strSiteURL))
                {
                    using (SPWeb web = site.OpenWeb())
                    {


                        if (status == null) status = "";

                        SPQuery SPquery = new SPQuery();



                        SPquery.Query = "<GroupBy Collapse=\"TRUE\" GroupLimit=\"200\"><FieldRef Name=\"stateid\"/><FieldRef Name=\"roleid\"/></GroupBy>"
                            + "<Where><And>" +
                           "<Eq><FieldRef Name='Status'/><Value Type='Text'>" + status + "</Value></Eq>"
                          + "<In><FieldRef Name='roleid' /><Values>"
                          + strInRoleids + "</Values></In>"
                               //   " " </ Value >
                               // "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + 1 + "</Value></Eq>" 
                               //  "<And><Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>" +
                               + "</And></Where>"

                                + "  <ViewFields>"
              + "<FieldRef Name='stateid' />"
              + "<FieldRef Name='roleid' />"
              + "</ViewFields>";

                        web.AllowUnsafeUpdates = true;
                        SPListItemCollection olists = web.Lists[cWfListName].GetItems(SPquery);


                        //var q = web.Lists[cWfListName].RenderListData(SPquery.ViewXml);
                        // Console.WriteLine("List ID::  " + list.Id);

                        string strresp = ListColAsJson(olists);
                        web.AllowUnsafeUpdates = false;
                        return getHttpResponseMessage(strresp);
                    }
                }
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }


        }

        public string ListColAsJson(SPListItemCollection IndListItem)
        {
            //workout how many rows we have
            int rowcnt = IndListItem.Count;
            //variables for the field names, value & to build the json
            string fld_name = "";
            string fval = "";
            string json = "{\"rows\":[";
            //Loop through the list item collection
            int i = 0;
            foreach (SPListItem oListItem in IndListItem)
            {
                int fcount = oListItem.Properties.Count;
                //Loop through the fields in this item
                for (int j = 0; j < fcount; j++)
                {
                    // get field name & try to get a handle on its contents
                    fld_name = oListItem.Name[j].GetType().FullName;
                    try
                    {
                        fval = HttpUtility.HtmlEncode(oListItem.Name[j].ToString());
                    }
                    catch
                    {
                        fval = "Missing or invalid Value";
                    }
                    //try catch
                    json += '"' + fld_name + '"' + ":" + '"' + fval + '"' + ",";
                }
                //for j field loop
                // counter ensures we have commas after each row except last
                i++;
                if (i < IndListItem.Count - 1)
                {
                    json += "},";
                }
                else
                {
                    json += "}";
                }
                //if test for comma
            }
            //foreach row
            json += "]}";
            return json;
        }


       
        [Route("api/Pipflow/spgetTaskDetails")]
        [HttpGet, HttpPost]
        public HttpResponseMessage spgetTaskDetails(string Listname, string TaskType, string stateid, string roleid = "", string Taskuser = null, string ReleatedItems = null, string status = "")
        {
            // prepare site connection
            try
            {
                using (SPSite site = new SPSite(strSiteURL))
                {
                    using (SPWeb web = site.OpenWeb())
                    {

                        if (TaskType == null) TaskType = "";
                        if (roleid == null) roleid = "";
                        if (stateid == null) stateid = "";
                        if (status == null) status = "";
                        // global parameters
                        Taskuser = Taskuser == null ? "" : Taskuser;
                        ReleatedItems = ReleatedItems == null ? "" : "," + ReleatedItems + ",";
                        SPQuery SPquery = new SPQuery();

                        SPquery.Query = "<Where><And>";
                        if (roleid != "")
                            SPquery.Query += "<And>";
                        SPquery.Query += "<Eq><FieldRef Name='tasktype' /><Value Type='Number'>" + TaskType + "</Value></Eq>" +
                                    "<Eq><FieldRef Name='stateid' /><Value Type='Number'>" + stateid + "</Value></Eq>";
                        if (roleid != "")
                            SPquery.Query += "</And>";
                        if (roleid != "")
                            SPquery.Query += "<Eq><FieldRef Name='roleid' /><Value Type='Number'>" + roleid + "</Value></Eq>";
                        SPquery.Query += "</And></Where>";


                        List<pipflow> respmsg = new List<pipflow>();

                        foreach (SPListItem oListItem in web.Lists[cWfListName].GetItems(SPquery))
                        {
                            /* if (TaskType != "" && TaskType != "1")
                             {
                                 getSubTasks(ref respmsg, ReleatedItems, Taskuser, status, TaskType);
                                 break;
                             }*/
                            // create and cast the FieldUserValue from the value



                            // assigned to for listing the data

                            if (status != "" && oListItem["Status"].ToString().ToLower() != status.ToLower()) continue;
                            if (TaskType != "" && oListItem["tasktype"] != null && oListItem["tasktype"].ToString().ToLower() != TaskType.ToLower()) continue;
                            if (stateid != "" && oListItem["stateid"] != null && oListItem["stateid"].ToString().ToLower() != stateid.ToLower()) continue;
                            if (roleid != "" && oListItem["roleid"] != null && oListItem["roleid"].ToString().ToLower() != roleid.ToLower()) continue;

                            respmsg.Add(new pipflow
                            {

                                id = (oListItem["ID"] != null) ? oListItem["ID"].ToString() : "",
                                title = (oListItem["Title"] != null) ? oListItem["Title"].ToString() : "",
                                taskoutcome = (oListItem["TaskOutcome"] != null) ? oListItem["TaskOutcome"].ToString() : "",
                                RelatedItems = (oListItem["relateditem"] != null) ? oListItem["relateditem"].ToString() : "",
                                status = (oListItem["Status"] != null) ? oListItem["Status"].ToString() : "",
                                assigned_to = (oListItem["Assigned_x0020_To"] != null) ? oListItem["Assigned_x0020_To"].ToString() : "",
                                approveduser_to = (oListItem["approveduser"] != null) ? oListItem["approveduser"].ToString() : "",
                                areviewuser_to = (oListItem["areviewuser"] != null) ? oListItem["areviewuser"].ToString() : "",
                                tasktype = (oListItem["tasktype"] != null) ? oListItem["tasktype"].ToString() : "",
                                stateid = (oListItem["stateid"] != null) ? oListItem["stateid"].ToString() : "",
                                roleid = (oListItem["roleid"] != null) ? oListItem["roleid"].ToString() : "",
                                Created = (oListItem["Created"] != null) ? oListItem["Created"].ToString() : "",
                                Modified = (oListItem["Modified"] != null) ? oListItem["Modified"].ToString() : ""
                            });



                        }

                        return getHttpResponseMessage(JsonConvert.SerializeObject(respmsg));
                    }
                }
            }
            catch (Exception ex)
            {
                return getErrormessage(ex.Message);
            }


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


        [System.Web.Http.Route("api/Pipflow/IOTBulkPushAPIS")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage IOTBulkPushAPIS([FromBody] JToken postData, HttpRequestMessage request)
        {
            using (SPSite site = new SPSite(strSiteURL))
            {
                using (SPWeb web = site.OpenWeb())
                {
                    web.AllowUnsafeUpdates = true;
                    SPListItem oItem = web.Lists[cBulkListName].AddItem();
                    try
                    {

                        oItem["Title"] = "IOT DATA";
                        oItem["status"] = "-9";
                        oItem["pushurl"] = postData.ToString();

                        string strLog = "";
                        //   List<IotDevice> iotDobj = new List<IotDevice>

                        double finalDistance = 0, distance, ratio;
                        Hashtable _htDivices = new Hashtable();
                        List<IotDevice> obj = JsonConvert.DeserializeObject<List<IotDevice>>(postData.ToString());

                        foreach (IotDevice sobj in obj)
                        {
                            if (sobj.type.ToLower() == "gateway")
                            { strLog = sobj.mac + ",gateway,"; break; }

                        }

                        foreach (string strDevic in ClsGeneral.getConfigvalue("macs").Split(','))
                        {

                            foreach (IotDevice sobj in obj)
                            {

                                if (sobj.mac == strDevic && sobj.ibeaconTxPower != null)
                                {

                                    ratio = double.Parse(sobj.rssi) * 1.0 / (double.Parse(sobj.ibeaconTxPower));
                                    // var distance = 0;
                                    if (ratio < 1.0)
                                    {
                                        distance = Math.Pow(ratio, 10);
                                    }
                                    else
                                    {
                                        distance = (0.89976) * Math.Pow(ratio, 7.7095) + 0.111;
                                        // return distance;
                                    }
                                    // double distance = Math.Pow(10, ((double.Parse(sobj.ibeaconTxPower) - double.Parse(sobj.rssi)) / (10 * 2)));
                                    //  strLog += sobj.mac + "," + Math.Round(distance,2).ToString() + ",";
                                    if (_htDivices.Contains(strDevic))
                                        _htDivices[strDevic] = double.Parse(_htDivices[strDevic].ToString()) + distance;
                                    else
                                        _htDivices.Add(strDevic, distance);

                                    if (_htDivices.Contains(strDevic + "_count"))
                                        _htDivices[strDevic + "_count"] = int.Parse(_htDivices[strDevic + "_count"].ToString()) + 1;
                                    else
                                        _htDivices.Add(strDevic + "_count", 1);
                                    // break;
                                }
                            }
                            if (_htDivices.Contains(strDevic))
                                finalDistance = Math.Round(double.Parse(_htDivices[strDevic].ToString()) / int.Parse(_htDivices[strDevic + "_count"].ToString()), 2);
                            strLog += strDevic + "," + finalDistance.ToString() + ",";
                        }
                        oItem["log"] = strLog;
                    }
                    catch (Exception ex)
                    {
                        oItem["pushurl"] = ex.Message;
                      
                        return getErrormessage(ex.Message);
                    }

                    oItem.Update();
                    web.AllowUnsafeUpdates = false;
                }

            }
            return getSuccessmessage("Success");
        }



        [System.Web.Http.Route("api/Pipflow/BulkPushAPIS")]
        [System.Web.Http.HttpPost]
        public HttpResponseMessage BulkPushAPIS(List<BulkpushAPIS> models)
        {
            string strResp = "Success";
            using (SPSite site = new SPSite(strSiteURL))
            {

                using (SPWeb web = site.OpenWeb())
                {
                    SPSecurity.RunWithElevatedPrivileges(delegate ()
                    {
                        using (SPSite ElevatedsiteColl = new SPSite(site.ID))
                        {
                            using (SPWeb ElevatedSite = ElevatedsiteColl.OpenWeb(web.ID))
                            {
                                web.AllowUnsafeUpdates = true;
                                foreach (BulkpushAPIS BulkAPI in models)
                                {
                                    // prepare site connection               

                                    try
                                    {
                                        SPListItem oItem = web.Lists[cBulkListName].AddItem();
                                        oItem["Title"] = BulkAPI.Title;
                                        oItem["pushurl"] = BulkAPI.url;
                                        string stateid = "0";

                                        var uri = new Uri(BulkAPI.url);
                                        var query = HttpUtility.ParseQueryString(uri.Query);
                                        dynamic QueryParams, QueryParam;
                                        QueryParams = JArray.Parse(ClsGeneral.GetJsonStringFromQueryString(query.ToString().ToLower()));

                                        QueryParam = QueryParams[0];

                                        if (QueryParam.stateid != null) stateid = QueryParam.stateid.Value;

                                        oItem["stateid"] = stateid;

                                        if (BulkAPI.url.ToString().ToLower().Contains("/pipflow/spsetfmr?"))
                                        {
                                            oItem["status"] = "-1";
                                        }
                                        else if (BulkAPI.url.ToString().ToLower().Contains("/pipflow/spsettaskitembyid?"))
                                        {
                                            oItem["status"] = "-2";
                                        }// below two loop for suplimentary insertion to 
                                        else if (BulkAPI.url.ToString().ToLower().Contains("/suplipipflow/spsetfmr?"))
                                        {
                                            oItem["status"] = "-3";
                                        }
                                        else if (BulkAPI.url.ToString().ToLower().Contains("/suplipipflow/spsettaskitembyid?"))
                                        {
                                            oItem["status"] = "-4";
                                        }

                                        //oItem["callbackurl"] = BulkAPI.callbackurl;
                                        oItem.Update();

                                    }
                                    catch (Exception ex)
                                    {

                                        strResp = ex.Message;
                                    }

                                }
                            }
                        }
                    });

                    web.AllowUnsafeUpdates = true;
                }


            }
            return getSuccessmessage(strResp);
        }

        [System.Web.Http.Route("api/Pipflow/BulkPushAPISJsonUpload")]
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
                        if (ClsGeneral.getConfigvalue("UPLOAD_FILE_PATH") != "") filePath = ClsGeneral.getConfigvalue("UPLOAD_FILE_PATH") + "/" + postedFile.FileName;
                  
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
    }
}





