using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models.Options
{
    public class SecurityRoleAssignments
    {
        public bool IsSubjectAreaScoped { get; set; }
        public string SecurityGroupId { get; set; }
        public List<string> AllowActions { get; set; } = new List<string>();
    }
}
