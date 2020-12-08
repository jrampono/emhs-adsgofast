using System;
using System.Collections.Generic;

namespace WebApplication.Models
{
    public partial class ScheduleInstance
    {
        public long ScheduleInstanceId { get; set; }
        public long? ScheduleMasterId { get; set; }
        public DateTime? ScheduledDateUtc { get; set; }
        public DateTimeOffset? ScheduledDateTimeOffset { get; set; }
        public bool ActiveYn { get; set; }
    }
}
