using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPPipAPi.Models
{
    public class fmrlist
    {
        public string id { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string remarks { get; set; }
        public string taskoutcome { get; set; }

        public string RelatedItems { get; set; }

        public string Modified_By { get; set; }

        public string Modified_By_id { get; set; }

        public string Modified { get; set; }
        public string Created_By { get; set; }

        public string Created_By_id { get; set; }

        public string Created { get; set; }


        public string assigned_to { get; set; }

        public string assigned_to_id { get; set; }

        public string approveduser_to_id { get; set; }
        public string approveduser_to { get; set; }

        public string areviewuser_to_id { get; set; }
        public string areviewuser_to { get; set; }

        public string currentassign_to_id { get; set; }
        public string currentassign_to { get; set; }
        public string currenttaskid { get; set; }

        public string fy { get; set; }

        public string fmrtype { get; set; }

        public string stateid { get; set; }
        public string roleid { get; set; }

        public string sid { get; set; }
    }

    

    public class pipflow
    {
        public string id { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string remarks { get; set; }
        public string taskoutcome { get; set; }
      

        public string stateid { get; set; }

        public string roleid { get; set; }

        public string sid { get; set; }
        public string RelatedItems { get; set; }

        public string Modified_By { get; set; }

        public string Modified_By_id { get; set; }
        public string Modified { get; set; }
        public string Created_By { get; set; }

        public string Created_By_id { get; set; }
        public string Created { get; set; }
        public string assigned_to { get; set; }

        public string assigned_to_id { get; set; }


        public string currentassign_to_id { get; set; }
        public string currentassign_to { get; set; }

        public string approveduser_to_id { get; set; }
        public string approveduser_to { get; set; }

        public string areviewuser_to_id { get; set; }
        public string areviewuser_to { get; set; }


        public string tasktype { get; set; }


        public string ParentID { get; set; }

        public static string getCAMLQry(string ReleatedItems, string Taskuser, string status, string TaskType,string roleid, string stateid)
        {
            string camlQuery = "";
            if (ReleatedItems != null && ReleatedItems != "") 
            {
                ReleatedItems = ReleatedItems.Substring(1, ReleatedItems.Length - 2);
            }
             if (status != null && status != ""&& ReleatedItems != null && ReleatedItems != ""&& Taskuser != null && Taskuser != "" && TaskType !=null && TaskType !="")
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{2}</Value></Eq><And><Eq><FieldRef Name='tasktype'/><Value Type ='Integer'>{3}</Value></Eq></And></And></And></Where></Query></View>", status, ReleatedItems, Taskuser, TaskType);
               if (roleid != null && roleid != "" && stateid != null && stateid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{2}</Value></Eq><And><Eq><FieldRef Name='tasktype'/><Value Type ='Integer'>{3}</Value></Eq><And><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{4}</Value></Eq><And><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{5}</Value></Eq></And></And></And></And></And></Where></Query></View>", status, ReleatedItems, Taskuser, TaskType, roleid, stateid);                        
                }
                if (roleid != null && roleid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{2}</Value></Eq><And><Eq><FieldRef Name='tasktype'/><Value Type ='Integer'>{3}</Value></Eq><And><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{4}</Value></Eq></And></And></And></And></Where></Query></View>", status, ReleatedItems, Taskuser, TaskType, roleid);
                }
                if (stateid != null && stateid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{2}</Value></Eq><And><Eq><FieldRef Name='tasktype'/><Value Type ='Integer'>{3}</Value></Eq><And><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{4}</Value></Eq></And></And></And></And></Where></Query></View>", status, ReleatedItems, Taskuser, TaskType, stateid);
                }
            }
            else if(status != null && status != "" && ReleatedItems != null && ReleatedItems != "" && Taskuser != null && Taskuser != "")
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{2}</Value></Eq></And></And></Where></Query></View>", status, ReleatedItems, Taskuser);
                if (roleid != null && roleid != "" && stateid != null && stateid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{2}</Value></Eq><And><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{3}</Value></Eq><And><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{4}</Value></Eq></And></And></And></And></Where></Query></View>", status, ReleatedItems, Taskuser, roleid, stateid);
                }
                if (roleid != null && roleid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{2}</Value></Eq><And><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{3}</Value></Eq></And></And></And></Where></Query></View>", status, ReleatedItems, Taskuser, roleid);
                }
                if (stateid != null && stateid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{2}</Value></Eq><And><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{3}</Value></Eq></And></And></And></Where></Query></View>", status, ReleatedItems, Taskuser, stateid);
                }
            }
            else if(status != null && status != "" && ReleatedItems != null && ReleatedItems != "")
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq></And></Where></Query></View>", status, ReleatedItems);
                if (roleid != null && roleid != "" && stateid != null && stateid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{2}</Value></Eq><And><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{3}</Value></Eq></And></And></And></Where></Query></View>", status, ReleatedItems, roleid, stateid);
                }
                if (roleid != null && roleid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{2}</Value></Eq></And></And></Where></Query></View>", status, ReleatedItems, roleid);
                }
                if (stateid != null && stateid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{2}</Value></Eq></And></And></Where></Query></View>", status, ReleatedItems, stateid);
                }
            }
            else if(Taskuser != null && Taskuser != "" && TaskType != null && TaskType != "" )
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{0}</Value></Eq><And><Eq><FieldRef Name='tasktype'/><Value Type ='Integer'>{1}</Value></Eq></And></Where></Query></View>", Taskuser, TaskType);
                if (roleid != null && roleid != "" && stateid != null && stateid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{2}</Value></Eq><And><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{3}</Value></Eq></And></And></And></Where></Query></View>", status, ReleatedItems, roleid, stateid);
                }
                if (roleid != null && roleid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{2}</Value></Eq></And></Where></Query></View>", status, ReleatedItems, roleid);
                }
                if (stateid != null && stateid != "")
                {
                    camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{2}</Value></Eq></And></Where></Query></View>", status, ReleatedItems, stateid);
                }
            }
            else if(Taskuser != null && Taskuser != "" && status != null && status != "")
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{0}</Value></Eq><And><Eq><FieldRef Name='Status'/><Value Type='Text'>{1}</Value></Eq></And></Where></Query></View>", Taskuser, status);
            }
            else if(ReleatedItems != null && ReleatedItems != "" && status != null && status != "")
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='Status'/><Value Type='Text'>{1}</Value></Eq></And></Where></Query></View>", ReleatedItems, status);
            }
            else if(status != null && status != "")
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq></Where></Query></View>", status);
            }
             else if(roleid != null && roleid != "" && stateid != null && stateid !="" )
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{0}</Value></Eq><And><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{1}</Value></Eq></And></Where></Query></View>", roleid, stateid);
            }
             else if(roleid != null && roleid != "")
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='roleid'/><Value Type='Integer'>{0}</Value></Eq></Where></Query></View>", roleid);
            }
            else if (stateid != null && stateid != "")
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='stateid'/><Value Type='Integer'>{0}</Value></Eq></Where></Query></View>", stateid);
            }
            else
            {
                camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='Status'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{1}</Value></Eq><And><Eq><FieldRef Name='Assigned_x0020_To'/><Value Type ='User'>{2}</Value></Eq><And><Eq><FieldRef Name='tasktype'/><Value Type ='Integer'>{3}</Value></Eq></And></And></And></Where></Query></View>", status, ReleatedItems, Taskuser, TaskType);
                //camlQuery = string.Format("<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name='relateditem'/><Value Type='Text'>{0}</Value></Eq><And><Eq><FieldRef Name='tasktype'/><Value Type ='Integer'>{1}</Value></Eq></And></Where></Query></View>", ReleatedItems,TaskType);
            }
            return camlQuery;
        }

    }


    /*
     * 
       Console.Write(fileVersion.CreatedBy.Title);
                        Console.Write(fileVersion.CheckInComment);
                        Console.Write(fileVersion.VersionLabel);
                        Console.Write(fileVersion.IsCurrentVersion);
     */
    public class comments
    {
        public string id { get; set; }
        public string title { get; set; }

        public string CheckInComment { get; set; }

        public string VersionLabel { get; set; }

        public string IsCurrentVersion { get; set; }
        /*   public string assigned_to { get; set; }

           public string assigned_to_id { get; set; }

           public string approved_to_id { get; set; }
           public string approved_to { get; set; }

           public string rejected_to_id { get; set; }
           public string rejected_to { get; set; }

           public string flowevent { get; set; }*/
}
//[{"ItemId":18,"WebId":"f122e31d-c3d0-4fb9-9abc-bede942a2f82","ListId":"d98c9f6b-757f-4c28-a656-a0a4eac0492e"}]
public class pipflowevents
    {
        public string id { get; set; }
        public string title { get; set; }

        public string arole { get; set; }

        public string rrole { get; set; }

        public string flowevent { get; set; }
        /*   public string assigned_to { get; set; }

           public string assigned_to_id { get; set; }

           public string approved_to_id { get; set; }
           public string approved_to { get; set; }

           public string rejected_to_id { get; set; }
           public string rejected_to { get; set; }

           public string flowevent { get; set; }*/
    }
    public class BulkpushAPIS
    {
        #region Properties
        public string Title { get; set; }
        public string url { get; set; }

        public string callbackurl { get; set; }
        #endregion
    }
    public class RelatedItemFieldValue
    {
        #region Properties
        public int ItemId { get; set; }
        public Guid WebId { get; set; }
        public Guid ListId { get; set; }
        #endregion
    }
}