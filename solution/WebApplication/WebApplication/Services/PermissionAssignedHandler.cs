using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplication.Services
{
    public class PermissionAssignedHandler : AuthorizationHandler<PermissionAssignedViaRole>
    {
        private readonly SecurityAccessProvider _provider;
        
        public PermissionAssignedHandler(SecurityAccessProvider provider)
        {
            _provider = provider;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                               PermissionAssignedViaRole requirement)
        {
            if (context.Resource is Endpoint endpoint)
            {
                var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();

                if (_provider.CanPerformOperation(actionDescriptor.ControllerName, actionDescriptor.ActionName, ((ClaimsIdentity)context.User.Identity)))
                {
                    context.Succeed(requirement);
                }
            }

            return Task.CompletedTask;
        }
    }
}
