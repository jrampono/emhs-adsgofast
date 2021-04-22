using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdsGoFast.Services
{    
    public interface ISecurityAccessProvider
    {
        string GetAccessToken(HttpRequest req);
        public Task<System.Security.Claims.ClaimsPrincipal> ValidateAccessToken(string accessToken, ILogger log);
    }
  
}
