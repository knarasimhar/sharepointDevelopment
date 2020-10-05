
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SharePoint.Client;

namespace copylist_items
{
    class Program
    {
        static String strSiteURL = "http://sharepoint2/sites/teamsiteex/PipFlowSite", strDestSiteURL="", strUSER = "spuser2", strPWD = "User@123#";
        static List _cache_sourceList = null;

        static void Main(string[] args)
        {
            if (getConfigvalue("SITE_URL") != "")
                strSiteURL = getConfigvalue("SITE_URL");
            if (getConfigvalue("DEST_SITE_URL") != "")
                strDestSiteURL = getConfigvalue("DEST_SITE_URL");
            if (getConfigvalue("SITE_URL_USER") != "")
                strUSER = getConfigvalue("SITE_URL_USER");
            if (getConfigvalue("SITE_URL_PWD") != "")
                strPWD = getConfigvalue("SITE_URL_PWD");
           

            //deleteitems();
            if (args.Length !=0 && args[0]!=null)
                CopyItemsFromOneListToAnotherList(args[0]);
            else
                CopyItemsFromOneListToAnotherList("1");
        }
        public static void CopyItemsFromOneListToAnotherList(string sateid)
        {
            try
            {
                using (ClientContext ctx = new ClientContext(strSiteURL))
                {
                    NetworkCredential Credentials = new NetworkCredential(strUSER, strPWD);
                    //ctx.Credentials = new SharePointOnlineCredentials(GetSPOAccountName(), GetSPOSecureStringPassword());
                    ctx.RequestTimeout = int.MaxValue;
                    ctx.Load(ctx.Web);
                    ctx.ExecuteQuery();
                    string strSourceList = getConfigvalue("sourceListname"), strDestinationList = getConfigvalue("destinationListname");
                    List sourceList = ctx.Web.Lists.GetByTitle(strSourceList);
                    ctx.Load(sourceList);
                    ctx.ExecuteQuery();
                  
                    // for destination is different sub site then
                    ClientContext Destctx;
                    if (strDestSiteURL != "")
                    {
                         Destctx = new ClientContext(strDestSiteURL);
                    
                    }
                    else
                         Destctx = new ClientContext(strSiteURL);

                    Destctx.RequestTimeout = int.MaxValue;
                    List destList = Destctx.Web.Lists.GetByTitle(strDestinationList);

                    Destctx.Load(destList);
                    Destctx.ExecuteQuery();

                    ctx.Load(sourceList);
                    ctx.ExecuteQuery();
                    CamlQuery oQuery = new CamlQuery();
                    //oQuery.ViewXml = "<Query><Where><And><Geq><FieldRef Name='stateid'/><Value Type='Number'>" + 0 + "</Value></Geq><Leq><FieldRef Name='stateid'/><Value Type='Number'>" + 4 + "</Value></Leq></And></Where></Query>";
                    oQuery.ViewXml = ("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='stateid'/><Value Type='Number'>"+ sateid + "</Value></Eq></Where></Query></View>");
                    ListItemCollection listItems = sourceList.GetItems(oQuery);

                    ctx.Load(listItems);
                    ctx.ExecuteQuery();
                    int i = 0; 
                    List oList1 = ctx.Web.Lists.GetByTitle(strSourceList);
                    foreach (ListItem oListItem in listItems)
                    {
                        //if ()
                        //{
                       // List sourceListNew = ctx.Web.Lists.GetByTitle(strSourceList);
                      
                     
                        ListItem item = oList1.GetItemById(oListItem.Id);
                        ctx.Load(item);
                        ctx.ExecuteQuery();
                        //}
                        ListItemCreationInformation newItemInfo = new ListItemCreationInformation();
                        ListItem newItem = destList.AddItem(newItemInfo);
                      //  newItem["idno"] = item.Id;
                        newItem["Title"] = item["Title"];
                        newItem["approveduser"] = item["approveduser"];
                        newItem["areviewuser"] = item["areviewuser"];
                        newItem["Assigned_x0020_To"] = item["Assigned_x0020_To"];
                        newItem["event"] = item["event"];
                        newItem["relateditem"] = item["relateditem"];
                        newItem["tasktype"] = item["tasktype"];
                        newItem["comments"] = item["comments"];
                        newItem["TaskOutcome"] = item["TaskOutcome"];
                        newItem["Status"] = item["Status"];
                        newItem["taskid"] = item["taskid"];
                        newItem["stateid"] = item["stateid"];
                        newItem["roleid"] = item["roleid"];
                        newItem["Modified"] = item["Modified"];
                        newItem["Created"] = item["Created"];
                        newItem["Author"] = item["Author"];
                        newItem["Editor"] = item["Editor"];
                        newItem.Update();
                        Destctx.Load(newItem);
                        i++;
                        Console.WriteLine(i + " Item process id " + item.Id);
                        if (i % 100 == 0)
                        {
                            var watch1 = System.Diagnostics.Stopwatch.StartNew();
                            watch1.Start();
                            // the code that you want to measure comes here
                            Destctx.ExecuteQuery();
                            watch1.Stop();
                            var elapsedMs1 = watch1.ElapsedMilliseconds;
                            Console.WriteLine("Bulk push call " + i + " Time Take " + elapsedMs1);
                        }
                    }
                    var watch = System.Diagnostics.Stopwatch.StartNew();
                    watch.Start();
                    // the code that you want to measure comes here
                    Destctx.ExecuteQuery();
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Console.WriteLine("Last call "+  i + " Time Take " + elapsedMs);
                    //ctx.ExecuteQuery();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }

            
               
                
        }
        public static String getConfigvalue(String key)
        {
            if (ConfigurationSettings.AppSettings[key] != null)
                return ConfigurationSettings.AppSettings[key];
            else
                return "";
        }
        public static void deleteitems()
        {
            try
            {
                using (ClientContext ctx = new ClientContext("http://sharepoint1/testlocal/"))
                {
                    NetworkCredential Credentials = new NetworkCredential(@"daplsp\spuser1", "%Dalab^!065#");
                    //ctx.Credentials = new SharePointOnlineCredentials(GetSPOAccountName(), GetSPOSecureStringPassword());
                    ctx.Load(ctx.Web);
                    ctx.ExecuteQuery();

                    List destList = ctx.Web.Lists.GetByTitle("workflow_history_20");
                    ctx.Load(destList);
                    ctx.ExecuteQuery();
                    CamlQuery oQuery = new CamlQuery();
                    oQuery.ViewXml = "<View><RowLimit>20000</RowLimit></View>";
                    ListItemCollection listItems = destList.GetItems(oQuery);

                    ctx.Load(listItems);
                    ctx.ExecuteQuery();

                    for (int i = 1; i < listItems.Count; i++)
                    {
                        listItems[i].DeleteObject();
                        ctx.ExecuteQuery();
                        Console.WriteLine(i);
                    }


                }

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }    
    }

}


