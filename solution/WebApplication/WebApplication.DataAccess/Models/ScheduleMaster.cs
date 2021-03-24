namespace WebApplication.Models
{
    [System.ComponentModel.DataAnnotations.DisplayColumn("ScheduleDesciption")]
    public partial class ScheduleMaster
    {
        public long ScheduleMasterId { get; set; }    
        public string ScheduleDesciption { get; set; }
        public string ScheduleCronExpression { get; set; }
        public bool ActiveYn { get; set; }
    }
}
