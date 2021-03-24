namespace WebApplication.Models
{
    public partial class TaskType
    {
        public int TaskTypeId { get; set; }
        public string TaskTypeName { get; set; }
        //public string TaskExecutionType { get; set; }
        public string TaskTypeJson { get; set; }
        public bool ActiveYn { get; set; }
    }
}
