using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    public partial class TaskMasterWaterMark
    {
        public long TaskMasterId { get; set; }
        public string TaskMasterWaterMarkColumn { get; set; }
        public string TaskMasterWaterMarkColumnType { get; set; }
        public DateTime? TaskMasterWaterMarkDateTime { get; set; }
        public long? TaskMasterWaterMarkBigInt { get; set; }
        public string TaskWaterMarkJson { get; set; }
        public bool ActiveYn { get; set; }
        public DateTimeOffset UpdatedOn { get; set; }
    }
}
