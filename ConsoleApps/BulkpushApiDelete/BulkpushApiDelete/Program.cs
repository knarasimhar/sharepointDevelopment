using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Client;
using System.Configuration;

namespace UpdateListItem
{
    class Program
    {
      
        static void Main(string[] args)
        {
            String strSiteURL = "http://sharepoint2/sites/teamsiteex/PipFlowSite", strUSER = "spuser2", strPWD = "User@123#";


        
            if (getConfigvalue("SITE_URL") != null)
                strSiteURL = getConfigvalue("SITE_URL").ToString();
          
            if (getConfigvalue("SITE_URL_USER") != null)
                strUSER = getConfigvalue("SITE_URL_USER").ToString();
            if (getConfigvalue("SITE_URL_PWD") != null)
                strPWD = getConfigvalue("SITE_URL_PWD").ToString();

            ClientContext clientContext = new ClientContext(strSiteURL);
            clientContext.Credentials = new NetworkCredential(strUSER, strPWD);
            Web web = clientContext.Web;
            clientContext.Load(web);
            //  var tasks;
            string strStatus = "9";
            foreach (string arg in args)
            {
                strStatus = arg;
            }
            // clientContext.Load(tasks, c => c.Where(t => t.Parent != null && t.Parent.Id == parentId));
           // Console.WriteLine("please enter the list Name to delete");
            string deleteListname ="bulkpushapis";
            List oList;
            if (args.Length>1)
                oList = web.Lists.GetByTitle(args[0]);
            else 
            oList = web.Lists.GetByTitle(deleteListname);

            CamlQuery oQuery = new CamlQuery();
            //oQuery.ViewXml=  string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq></And></Where></Query></View>", status, ReleatedItems);
            oQuery.ViewXml = "<View><RowLimit>20000</RowLimit></View>";

            if (args.Length > 1)
                oQuery.ViewXml = "<View><Query><Where><Eq><FieldRef Name='stateid'/><Value Type='Number'>" + args[1] + "</Value></Eq></Where></Query></View>";

            else
                oQuery.ViewXml = "<View><Query><Where><Eq><FieldRef Name='status'/><Value Type='Number'>" + strStatus + "</Value></Eq></Where></Query></View>";
            //oQuery.ViewXml=("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='status'/><Value Type='Number'>1</Value></Eq><RowLimit>500</RowLimit></Where></Query></View>");



            ListItemCollection oItems = oList.GetItems(oQuery);
            clientContext.Load(oItems);
            clientContext.ExecuteQuery();
            int i = 1;
            foreach (ListItem oListItem in  oItems)
            {

                try
                {
                    List oList1;
                    if (args.Length > 1)
                         oList1 = clientContext.Web.Lists.GetByTitle(args[0]);
                    else
                         oList1 = clientContext.Web.Lists.GetByTitle("bulkpushapis");
                    ListItem oListItem1 = oList1.GetItemById(oListItem.Id);
                   // oListItem["Title"] = "Custom Title updated Programmatically.";
                    oListItem1.DeleteObject();
                    clientContext.ExecuteQuery();
                    Console.WriteLine("SNO :" + i.ToString() + " Process id " + oListItem.Id.ToString() + " Title :" + oListItem["Title"]);

                    /*  oListItem.DeleteObject();

                      clientContext.Load(oListItem);
                      Console.WriteLine("SNO :" + i.ToString() + " Process id " + oItems[i].Id.ToString() + " Title :" + oItems[i]["Title"] + " Status :" + oItems[i]["status"]);
                      if (i % 500 == 0)
                      {
                          var watch = System.Diagnostics.Stopwatch.StartNew();
                          clientContext.ExecuteQuery();
                          watch.Stop();
                          Console.WriteLine("100  Execution time is  :" + watch.Elapsed.ToString());

                          clientContext.Dispose();
                      }*/
                    i++;
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Delete exception data " + ex.Message);
                }
            }
          //  clientContext.ExecuteQuery();
          //  clientContext.Dispose();
            //foreach (ListItem listItem in oItems)
            //{
            //    listItem.DeleteObject();
            //    clientContext.ExecuteQuery();
            //}



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