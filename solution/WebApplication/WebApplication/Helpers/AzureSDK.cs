using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication.Helpers
{
    public class AzureSDK
    {
        bool _UseMSI { get; set; }
        string _ApplicationId { get; set; }

        string _AuthenticationKey { get; set; }

        string _TenantId { get; set; }

        public AzureSDK(bool _UseMSI,string _ApplicationId, string _AuthenticationKey, string _TenantId)
        {
            this._UseMSI = _UseMSI;
            this._ApplicationId = _ApplicationId;
            this._AuthenticationKey = _AuthenticationKey;
            this._TenantId = _TenantId;

        }

        //Gets RestAPI Token for various Azure Resources using the SDK Helper Classes
        public string GetAzureRestApiToken(string ServiceURI)
        {
            if (_UseMSI)
            {
                return GetAzureRestApiToken(ServiceURI, _UseMSI, null, null);
            }
            else
            {
                //By Default Use Local SP Credentials
                return GetAzureRestApiToken(ServiceURI, _UseMSI, _ApplicationId, _AuthenticationKey);
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
                return GetAzureRestApiToken(ServiceURI, UseMSI, _ApplicationId, _AuthenticationKey);
            }
        }

        public string GetAzureRestApiToken(string ServiceURI, bool UseMSI, string ApplicationId, string AuthenticationKey)
        {
            if (UseMSI == true)
            {

                AzureServiceTokenProvider tokenProvider = new AzureServiceTokenProvider();
                //https://management.azure.com/                    
                return tokenProvider.GetAccessTokenAsync(ServiceURI).Result;

            }
            else
            {

                AuthenticationContext context = new AuthenticationContext("https://login.windows.net/" + _TenantId);
                ClientCredential cc = new ClientCredential(ApplicationId, AuthenticationKey);
                AuthenticationResult result = context.AcquireTokenAsync(ServiceURI, cc).Result;
                return result.AccessToken;
            }
        }        
    }

}
