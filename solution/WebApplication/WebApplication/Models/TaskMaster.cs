using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    public partial class TaskMaster
    {
        public long TaskMasterId { get; set; }
        public string TaskMasterName { get; set; }
        public int TaskTypeId { get; set; }
        public long TaskGroupId { get; set; }
        public long ScheduleMasterId { get; set; }
        public long SourceSystemId { get; set; }
        public long TargetSystemId { get; set; }
        public int DegreeOfCopyParallelism { get; set; }
        public bool AllowMultipleActiveInstances { get; set; }
        public string TaskDatafactoryIr { get; set; }
        public string TaskMasterJson { get; set; }
        public bool ActiveYn { get; set; }
        public string DependencyChainTag { get; set; }
        public long DataFactoryId { get; set; }
    }
}
