using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    public partial class TaskGroup
    {
        public long TaskGroupId { get; set; }
        public string TaskGroupName { get; set; }
        public int TaskGroupPriority { get; set; }
        public int TaskGroupConcurrency { get; set; }
        public string TaskGroupJson { get; set; }
        public int SubjectAreaId {get; set;}
        public bool ActiveYn { get; set; }
    }

    
}
