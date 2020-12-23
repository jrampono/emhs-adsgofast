using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models.Options
{
    public class SecurityModelOptions
    {
        public IDictionary<string, SecurityRoleAssignments> SecurityRoles { get; set; } = new Dictionary<string, SecurityRoleAssignments>();
        public List<string> GlobalAllowActions { get; set; } = new List<string>();
        public List<string> GlobalDenyActions { get; set; } = new List<string>();
        public List<string> ReaderActions { get; set; }
        public List<string> WriterActions { get; set; }
    }
}
