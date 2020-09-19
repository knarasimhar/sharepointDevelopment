using Microsoft.SharePoint;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StateWiseListCreationConsoleAPP
{
    class Program
    {
        static void Main(string[] args)
        {

            using (SPSite oSPsite = new SPSite(getConfigvalue("SITE_URL")))
            {

                using (SPWeb oSPWeb = oSPsite.OpenWeb())
                {
                    oSPWeb.AllowUnsafeUpdates = true;
                    /*create list from custom ListTemplate present within ListTemplateGalery */
                    SPListTemplateCollection lstTemp = oSPsite.GetCustomListTemplates(oSPWeb);
                    SPListTemplate template = lstTemp["workflow_history_temp"];
                    oSPWeb.Lists.Add("workflow_history_2", "state 2", template);
                   // SPList newList = web.Lists["Splessons"];
                    oSPWeb.AllowUnsafeUpdates = false;

                }

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


