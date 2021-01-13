namespace WebApplication.Models
{
    public partial class TaskGroupDependency
    {
        public long AncestorTaskGroupId { get; set; }
        public long DescendantTaskGroupId { get; set; }
        public string DependencyType { get; set; }
    }
}
