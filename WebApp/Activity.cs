//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace WebApp
{
    using System;
    using System.Collections.Generic;
    
    public partial class Activity
    {
        public int activityId { get; set; }
        public Nullable<int> activityUserId { get; set; }
        public Nullable<int> activityProjectId { get; set; }
        public Nullable<int> activityAnnouncementId { get; set; }
        public System.DateTime activityDatetime { get; set; }
        public Nullable<int> activityCommentId { get; set; }
        public string activityContent { get; set; }
        public Nullable<int> activityRelatedUser { get; set; }
        public string activityAction { get; set; }
    }
}
