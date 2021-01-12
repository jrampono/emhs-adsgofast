using System.Collections.Generic;

namespace WebApplication.Models.Options
{
    public class SecurityRole
    {
        public string SecurityGroupId { get; set; }
        public string Name { get; set; }
        public List<string> AllowActions { get; set; } = new List<string>();
    }
}
