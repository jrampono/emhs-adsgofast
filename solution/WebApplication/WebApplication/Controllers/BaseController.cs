using Humanizer;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    public class BaseController : Controller
    {
        private readonly SecurityAccessProvider _securityAccessProvider;

        protected string Name = "Base";

        public BaseController(SecurityAccessProvider securityAccessProvider)
        {
            Name = GetType().Name.Replace("Controller", string.Empty);
            _securityAccessProvider = securityAccessProvider;
        }

        public IEnumerable<Models.Options.SecurityRole> GetUserAppRoles(ClaimsIdentity identity)
        {
           return _securityAccessProvider.GetUserRoles(identity);
        }

        protected JArray GetSecurityFilteredActions(string actions)
        {
            var actionsList = actions.Split(",");
            JArray results = new JArray();
            foreach (var item in actionsList)
            {
                if (_securityAccessProvider.CanPerformOperation(Name, item, User.Identity as ClaimsIdentity))
                {
                    results.Add(item);
                }
            }
            return results;
        }

        protected void HumanizeColumns(JArray cols)
        {
            foreach (var col in cols)
            {
                var ob = (JObject)col;
                var name = ob.Property("name").Value.ToString();
                if (!string.IsNullOrEmpty(name))
                {
                    ob.Property("name").Value = name.Humanize(LetterCasing.Title);
                }
            }
        }
    }
}
