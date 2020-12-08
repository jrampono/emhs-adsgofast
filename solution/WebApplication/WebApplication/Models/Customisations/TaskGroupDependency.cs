using static WebApplication.Models.ModelEnums;

namespace WebApplication.Models
{

    public partial class TaskGroupDependency
    {
        //public virtual DependencyType DependencyType { get; set; }

        public virtual TaskGroup AncestorTaskGroup { get; set; }

        public virtual TaskGroup DescendantTaskGroup { get; set; }


    }

    public static partial class ModelEnums
    {
        public enum DependencyType
        {
            TasksMatchedByTagAndSchedule,
            EntireGroup
        } 
    }
}
