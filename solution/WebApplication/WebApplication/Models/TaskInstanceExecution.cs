using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    public partial class TaskInstanceExecution
    {
        public Guid ExecutionUid { get; set; }
        public long TaskInstanceId { get; set; }
        public Guid? DatafactorySubscriptionUid { get; set; }
        public string DatafactoryResourceGroup { get; set; }
        public string DatafactoryName { get; set; }
        public string PipelineName { get; set; }
        public Guid? AdfRunUid { get; set; }
        public DateTimeOffset? StartDateTime { get; set; }
        public DateTimeOffset? EndDateTime { get; set; }
        public string Status { get; set; }
        public string Comment { get; set; }
    }
}
