using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Threading;
using System.Threading.Tasks;
using AdsGoFast.Models.Options;

namespace AdsGoFast.Services
{
    public class AzureAuthenticationCredentialProvider
    {
        private readonly IOptions<ApplicationOptions> _appOptions;
        private readonly IAuthOptions _authOptions;

        public AzureAuthenticationCredentialProvider(IOptions<ApplicationOptions> appOptions, IAuthOptions authOptions)
        {
            _appOptions = appOptions;
            _authOptions = authOptions;
        }

        /// <summary>
        /// Returns a token to authenticate with Azure services
        /// </summary>
        /// <param name="serviceUri"></param>
        /// <returns>An access token for the requested endpoint/scope</returns>
        /// <remarks>
        /// If the web application is configured with ClientId and Secret using the standard MicrosoftIdentityOptions
        /// Then ClientCredentials are used to provide the identity requesting a token otherwise it will fall through to use
        /// an DefaultAzureCredential object. The following credential types if enabled will be tried, in order:
        /// - EnvironmentCredential
        /// - ManagedIdentityCredential
        /// - SharedTokenCacheCredential
        /// - VisualStudioCredential
        /// - VisualStudioCodeCredential
        /// - AzureCliCredential
        /// - InteractiveBrowserCredential
        /// </remarks>
        public async Task<string> GetMsalRestApiToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            TokenCredential credential = null;
            if (!string.IsNullOrEmpty(_authOptions.ClientSecret))
            {
                credential = new ClientSecretCredential(_authOptions.TenantId, _authOptions.ClientId, _authOptions.ClientSecret);
            }
            else
            {
                var defaultAzureCredentialOptions = new DefaultAzureCredentialOptions()
                {
                    ExcludeAzureCliCredential = true,
                    ExcludeManagedIdentityCredential = !_appOptions.Value.UseMSI
                };
                credential = new DefaultAzureCredential(defaultAzureCredentialOptions);
            }

            var result = await credential.GetTokenAsync(requestContext, cancellationToken);

            return result.Token;
        }

        /// <summary>
        /// Manual method of failing back through authentication options. Used to support AppInsights connectivity as GetMsalRestApiToken only supports a default scope.
        /// </summary>
        /// <param name="ServiceURI"></param>
        /// <returns></returns>
        public string GetAzureRestApiToken(string ServiceURI)
        {
            if (_appOptions.Value.UseMSI)
            {
                return GetAzureRestApiToken(ServiceURI, _appOptions.Value.UseMSI, null, null);
            }
            else
            {
                //By Default Use Local SP Credentials
                return GetAzureRestApiToken(ServiceURI, _appOptions.Value.UseMSI, _authOptions.ClientId, _authOptions.ClientSecret);
            }
        }

        public string GetAzureRestApiToken(string ServiceURI, bool UseMSI)
        {
            if (UseMSI)
            {
                return GetAzureRestApiToken(ServiceURI, UseMSI, null, null);
            }
            else
            {
                //By Default Use Local SP Credentials
                return GetAzureRestApiToken(ServiceURI, UseMSI, _authOptions.ClientId, _authOptions.ClientSecret);
            }
        }

        public string GetAzureRestApiToken(string ServiceURI, bool UseMSI, string ApplicationId, string AuthenticationKey)
        {
            if (UseMSI == true)
            {

                Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider tokenProvider = new Microsoft.Azure.Services.AppAuthentication.AzureServiceTokenProvider();
                //https://management.azure.com/                    
                return tokenProvider.GetAccessTokenAsync(ServiceURI).Result;

            }
            else
            {

                AuthenticationContext context = new AuthenticationContext("https://login.windows.net/" + _authOptions.TenantId);
                ClientCredential cc = new ClientCredential(ApplicationId, AuthenticationKey);
                AuthenticationResult result = context.AcquireTokenAsync(ServiceURI, cc).Result;
                return result.AccessToken;
            }
        }
    }

}
