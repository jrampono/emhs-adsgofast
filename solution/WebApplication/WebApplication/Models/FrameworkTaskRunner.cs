using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    public partial class FrameworkTaskRunner
    {
        public int TaskRunnerId { get; set; }
        public string TaskRunnerName { get; set; }
        public bool ActiveYn { get; set; }
        public string Status { get; set; }
        public int? MaxConcurrentTasks { get; set; }
    }
}
