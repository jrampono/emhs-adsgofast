using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication.Models
{    
    public partial class ScheduleMaster
    {
        public virtual List<TaskMaster> TaskMasters { get; set; }
        public virtual List<ScheduleInstance> ScheduleInstances { get; set; }
    }
}
