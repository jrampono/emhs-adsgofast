/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/

using AdsGoFast.Models.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AdsGoFast.Services
{ 

    public class SecurityAccessProvider : ISecurityAccessProvider
    {
        private readonly IOptions<DownstreamAuthOptionsViaAppReg> _options;
        private readonly IOptions<ApplicationOptions> _appOptions;
        public SecurityAccessProvider(IOptions<DownstreamAuthOptionsViaAppReg> options, IOptions<ApplicationOptions> appOptions)
        {
            _options = options;
            _appOptions = appOptions;
        }


        public bool IsAuthorised(HttpRequest req, ILogger log)
        {
            bool ret = false;
            string token = GetAccessToken(req, log);
            var principal = ValidateAccessToken(token, log).Result;
            foreach (var r in _appOptions.Value.ServiceConnections.CoreFunctionsAllowedRoles)
            {
                if (principal.IsInRole(r))
                {
                    ret = true;
                }
            }

            if (ret == false)
            {
                log.LogWarning("Authorisation Failed for Principal");
                foreach (var claim in principal.Claims)
                {
                    log.LogWarning(claim.Type + ":" + claim.Value); 
                }
            }

            return ret;
        
        }

        private static string GetAccessToken(HttpRequest req, ILogger log)
            {
                var authorizationHeader = req.Headers?["Authorization"];
                string[] parts = authorizationHeader?.ToString().Split(null) ?? new string[0];
                if (parts.Length == 2 && parts[0].Equals("Bearer"))
                    return parts[1];
                return null;
            }
            

        private async Task<System.Security.Claims.ClaimsPrincipal> ValidateAccessToken(string accessToken, ILogger log)
        {
        var audience = _options.Value.Audience;
        var clientID = _options.Value.ClientId;
        var tenant = _options.Value.Tenant;
        var tenantid = _options.Value.TenantId;
        var authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, "https://login.microsoftonline.com/{0}/v2.0", tenant);
        var validIssuers = new List<string>()
            {
                $"https://login.microsoftonline.com/{tenant}/",
                $"https://login.microsoftonline.com/{tenant}/v2.0",
                $"https://login.windows.net/{tenant}/",
                $"https://login.microsoft.com/{tenant}/",
                $"https://sts.windows.net/{tenantid}/"
            };

        // Debugging purposes only, set this to false for production
        Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;

            Microsoft.IdentityModel.Protocols.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration> configManager =
                new Microsoft.IdentityModel.Protocols.ConfigurationManager<Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration>(
                    $"{authority}/.well-known/openid-configuration",
                    new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfigurationRetriever());

            Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration config = null;
            config = await configManager.GetConfigurationAsync();

            Microsoft.IdentityModel.Tokens.ISecurityTokenValidator tokenValidator = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();

            // Initialize the token validation parameters
            Microsoft.IdentityModel.Tokens.TokenValidationParameters validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                // App Id URI and AppId of this service application are both valid audiences.
                ValidAudiences = new[] { audience, clientID },

                // Support Azure AD V1 and V2 endpoints.
                ValidIssuers = validIssuers,
                IssuerSigningKeys = config.SigningKeys
            };

            try
            {
                Microsoft.IdentityModel.Tokens.SecurityToken securityToken;
                var claimsPrincipal = tokenValidator.ValidateToken(accessToken, validationParameters, out securityToken);
                return claimsPrincipal;
            }
            catch (Exception ex)
            {
                log.LogInformation(ex.Message);
            }
            return null;
        }


        
    }
}
