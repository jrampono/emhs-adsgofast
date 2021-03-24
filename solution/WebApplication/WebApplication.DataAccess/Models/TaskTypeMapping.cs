namespace WebApplication.Models
{
    public partial class TaskTypeMapping
    {
        public int TaskTypeMappingId { get; set; }
        public int TaskTypeId { get; set; }
        public string MappingType { get; set; }
        public string MappingName { get; set; }
        public string SourceSystemType { get; set; }
        public string SourceType { get; set; }
        public string TargetSystemType { get; set; }
        public string TargetType { get; set; }
        public string TaskDatafactoryIr { get; set; }
        public string TaskTypeJson { get; set; }
        public bool ActiveYn { get; set; }
        public string TaskMasterJsonSchema { get; set; }
        public string TaskInstanceJsonSchema { get; set; }
    }
}
