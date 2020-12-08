using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication.Models
{ 

    public partial class TaskMaster
    {
        public virtual List<TaskInstance> TaskInstances { get; set; }
        public virtual TaskType TaskType { get; set; }

        public virtual TaskGroup TaskGroup { get; set; }

        public virtual ScheduleMaster ScheduleMaster { get; set; }

        public virtual SourceAndTargetSystems SourceSystem { get; set; }

        public virtual SourceAndTargetSystems TargetSystem { get; set; }


    }

    public static partial class ModelEnums
    {
        public enum TaskStatus
        {
            Succeeded,
            FailedRetry,
            FailedNoRetry,
            Untried,
            InProgress
        } 
    }
}
