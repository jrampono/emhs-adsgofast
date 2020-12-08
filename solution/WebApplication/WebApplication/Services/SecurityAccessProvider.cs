using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using WebApplication.Models.Options;

namespace WebApplication.Services
{
    public class SecurityAccessProvider
    {
        private readonly IOptions<SecurityModelOptions> _options;

        public SecurityAccessProvider(IOptions<SecurityModelOptions> options)
        {
            _options = options;
        }

        public bool CanPerformOperation(string controllerName, string actionName, ClaimsIdentity identity)
        {
            if (!identity.IsAuthenticated)
                return false;

            var groups = identity.Claims.Where(x => x.Type == "groups").Select(v => v.Value);

            var role = _options.Value.SecurityRoles.FirstOrDefault(x => groups.Any(a => a == x.SecurityGroupId)) ?? new SecurityRole();

            var operationName = $"{controllerName}.{actionName}";

            return (role.AllowActions.Any(x => x == operationName) ||
                _options.Value.GlobalAllowActions.Any(x => x == operationName) ||
                _options.Value.GlobalAllowActions.Any(x => x == controllerName))
                && !_options.Value.GlobalDenyActions.Any(x => x == controllerName)
                && !_options.Value.GlobalDenyActions.Any(x => x == operationName);
        }

        public IEnumerable<SecurityRole> GetUserRoles(ClaimsIdentity identity)
        {            
            var groups = identity.Claims.Where(x => x.Type == "groups").Select(v => v.Value);

            IEnumerable<SecurityRole> roles = _options.Value.SecurityRoles.Where(x => groups.Any(a => a == x.SecurityGroupId));

            return roles;
        }
    }
}
