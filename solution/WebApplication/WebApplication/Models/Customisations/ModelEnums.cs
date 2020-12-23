namespace WebApplication.Models
{
    public static partial class ModelEnums
    {
        public enum DependencyType
        {
            TasksMatchedByTagAndSchedule,
            EntireGroup
        }
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
