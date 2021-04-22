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

        public SecurityAccessProvider()
        {

        }

        public string GetAccessToken(HttpRequest req)
            {
                var authorizationHeader = req.Headers?["Authorization"];
                string[] parts = authorizationHeader?.ToString().Split(null) ?? new string[0];
                if (parts.Length == 2 && parts[0].Equals("Bearer"))
                    return parts[1];
                return null;
            }
            internal static class Constants
            {
                internal static string audience = "http://localhost:7071"; // Get this value from the expose an api, audience uri section example https://appname.tenantname.onmicrosoft.com
                internal static string clientID = "128a1f5d-f665-423c-9a1c-6e634b5d46df"; // this is the client id, also known as AppID. This is not the ObjectID
                internal static string tenant = "microsoft.onmicrosoft.com"; // this is your tenant name
                internal static string tenantid = "72f988bf-86f1-41af-91ab-2d7cd011db47"; // this is your tenant id (GUID)

                // rest of the values below can be left as is in most circumstances
                internal static string aadInstance = "https://login.microsoftonline.com/{0}/v2.0";
                internal static string authority = string.Format(System.Globalization.CultureInfo.InvariantCulture, aadInstance, tenant);
                internal static List<string> validIssuers = new List<string>()
                    {
                        $"https://login.microsoftonline.com/{tenant}/",
                        $"https://login.microsoftonline.com/{tenant}/v2.0",
                        $"https://login.windows.net/{tenant}/",
                        $"https://login.microsoft.com/{tenant}/",
                        $"https://sts.windows.net/{tenantid}/"
                    };
            }

            public async Task<System.Security.Claims.ClaimsPrincipal> ValidateAccessToken(string accessToken, ILogger log)
            {
                var audience = Constants.audience;
                var clientID = Constants.clientID;
                var tenant = Constants.tenant;
                var tenantid = Constants.tenantid;
                var aadInstance = Constants.aadInstance;
                var authority = Constants.authority;
                var validIssuers = Constants.validIssuers;

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
