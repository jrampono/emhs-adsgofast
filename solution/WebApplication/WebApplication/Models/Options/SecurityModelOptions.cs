using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Models.Options
{
    public class SecurityModelOptions
    {
        public List<SecurityRole> SecurityRoles { get; set; } = new List<SecurityRole>();
        public List<string> GlobalAllowActions { get; set; } = new List<string>();
        public List<string> GlobalDenyActions { get; set; } = new List<string>();
    }
}
