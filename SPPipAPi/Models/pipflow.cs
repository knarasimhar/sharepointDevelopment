﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SPPipAPi.Models
{
    public class pipflow
    {
        public string id { get; set; }
        public string title { get; set; }
        public string status { get; set; }
        public string remarks { get; set; }
        public string taskoutcome { get; set; }

        public string RelatedItems { get; set; }

        public string Modified_By { get; set; }

        public string Modified_By_id { get; set; }
        public string Created_By { get; set; }

        public string Created_By_id { get; set; }

        public string assigned_to { get; set; }

        public string assigned_to_id { get; set; }
        public string Modified_Date { get; set; }

        public string currentassign_to_id { get; set; }
        public string currentassign_to { get; set; }


    }
    //[{"ItemId":18,"WebId":"f122e31d-c3d0-4fb9-9abc-bede942a2f82","ListId":"d98c9f6b-757f-4c28-a656-a0a4eac0492e"}]

    public class pipflowevents
    {
        public string id { get; set; }
        public string title { get; set; }
        public string assigned_to { get; set; }

        public string assigned_to_id { get; set; }

        public string approved_to_id { get; set; }
        public string approved_to { get; set; }

        public string rejected_to_id { get; set; }
        public string rejected_to { get; set; }

        public string flowevent { get; set; }
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