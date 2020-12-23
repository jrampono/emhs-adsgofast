using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WebApplication.Services;

namespace WebApplication.Pages
{
    public class ClaimsModel : PageModel
    {
        public IList<string> Roles { get; set; }
        protected ISecurityAccessProvider SecProvider { get; }

        public ClaimsModel(ISecurityAccessProvider secProvider)
        {
            SecProvider = secProvider;
        }

        public void OnGet()
        {
            Roles = SecProvider.GetUserGlobalRoles(HttpContext.User.Identity as ClaimsIdentity);
        }
    }
}
