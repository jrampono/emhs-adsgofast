using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using System.Threading;
using System.Threading.Tasks;
using WebApplication.Models.Options;

namespace WebApplication.Services
{
    public class AzureAuthenticationCredentialProvider
    {
        private readonly IOptions<ApplicationOptions> _appOptions;
        private readonly IOptions<MicrosoftIdentityOptions> _authOptions;

        public AzureAuthenticationCredentialProvider(IOptions<ApplicationOptions> appOptions, IOptions<MicrosoftIdentityOptions> authOptions)
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
        public async Task<string> GetAzureRestApiToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            TokenCredential credential = null;
            if (!string.IsNullOrEmpty(_authOptions.Value.ClientSecret))
            {
                credential = new ClientSecretCredential(_authOptions.Value.TenantId, _authOptions.Value.ClientId, _authOptions.Value.ClientSecret);
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
    }

}
