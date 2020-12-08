using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models.Options
{
    public class SecurityRole
    {
        public string SecurityGroupId { get; set; }
        public string Name { get; set; }
        public List<string> AllowActions { get; set; } = new List<string>();
    }
}
