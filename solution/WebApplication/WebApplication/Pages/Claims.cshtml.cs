using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WebApplication.Models.Options;

namespace WebApplication.Pages
{
    public class ClaimsModel : PageModel
    {
        public IEnumerable<SecurityRole> Roles { get; set; }
        public void OnGet()
        {
            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddJsonFile("appSettings.json");
            var config = configBuilder.Build();
            var optionsModel = ConfigurationBinder.Get<SecurityModelOptions>(config.GetSection("SecurityModelOptions"));
            var options = Options.Create(optionsModel);
            var UserClaims = HttpContext.User.Claims;            
            var groups = UserClaims.Where(x => x.Type == "groups").Select(v => v.Value);
            Roles = options.Value.SecurityRoles.Where(x => groups.Any(a => a == x.SecurityGroupId));

        
        }
    }
}
