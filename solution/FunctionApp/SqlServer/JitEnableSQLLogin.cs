/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace AdsGoFast
{
    public static class JitEnableSQLLogin
    {
        [FunctionName("JitEnableSQLLogin")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            //Logging.LogInformation("C# HTTP trigger function processed a request.");


            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            string Subscription = data?.subscriptionid;
            string SQLServer = data?.vmname;
            string LoginNameToEnable = data?.vmresourcegroup;


            AzureCredentials msiCred = Helpers.GetAzureCreds(Helpers.GlobalConfigs.UseMSI);
            Microsoft.Azure.Management.Fluent.Azure.IAuthenticated azureAuth = Microsoft.Azure.Management.Fluent.Azure.Configure().WithLogLevel(HttpLoggingDelegatingHandler.Level.BodyAndHeaders).Authenticate(msiCred);
            IAzure azure = azureAuth.WithSubscription(Subscription);



            return SQLServer != null
                ? (ActionResult)new OkObjectResult($"Function completed for VM:")
                : new BadRequestObjectResult("Please pass a name, resourcegroup and action to request body");
        }

        public class Helpers
        {
            private static ILogger log { get; set; }
            public static void SetLog(ILogger log)
            {
                Helpers.log = log;
            }

            public static string AuthenticateAsyncViaRest(bool UseMSI, string ResourceUrl = null, string AuthorityUrl = null, string ClientId = null, string ClientSecret = null, string Username = null, string Password = null, string Scope = null, string GrantType = null)
            {
                HttpResponseMessage result = new HttpResponseMessage();
                if (UseMSI == true)
                {
                    //Logging.LogInformation("AuthenticateAsyncViaRest is using MSI");                
                    using (HttpClient client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("Secret", Environment.GetEnvironmentVariable("MSI_SECRET"));
                        result = client.GetAsync(string.Format("{0}/?resource={1}&api-version={2}", Environment.GetEnvironmentVariable("MSI_ENDPOINT"), ResourceUrl, "2017-09-01")).Result;
                    }

                }
                else
                {
                    //Logging.LogInformation("AuthenticateAsyncViaRest is using Service Principal");
                    Uri oauthEndpoint = new Uri(AuthorityUrl);

                    using (HttpClient client = new HttpClient())
                    {
                        List<KeyValuePair<string, string>> body = new List<KeyValuePair<string, string>>();

                        if (ResourceUrl != null) { body.Add(new KeyValuePair<string, string>("resource", ResourceUrl)); }
                        if (ClientId != null) { body.Add(new KeyValuePair<string, string>("client_id", ClientId)); }
                        if (ClientSecret != null) { body.Add(new KeyValuePair<string, string>("client_secret", ClientSecret)); }
                        if (GrantType != null) { body.Add(new KeyValuePair<string, string>("grant_type", GrantType)); }
                        if (Username != null) { body.Add(new KeyValuePair<string, string>("username", Username)); }
                        if (Password != null) { body.Add(new KeyValuePair<string, string>("password", Password)); }
                        if (Scope != null) { body.Add(new KeyValuePair<string, string>("scope", Scope)); }

                        result = client.PostAsync(oauthEndpoint, new FormUrlEncodedContent(body)).Result;
                    }
                }
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    string content = result.Content.ReadAsStringAsync().Result;
                    var definition = new { access_token = "" };
                    var jobject = JsonConvert.DeserializeAnonymousType(content, definition);
                    return (jobject.access_token);
                }
                else
                {
                    string error = "AuthenticateAsyncViaRest Failed..";
                    try
                    {
                        string content = result.Content.ReadAsStringAsync().Result;
                        error = error + content;
                    }
                    catch
                    {

                    }
                    finally
                    {
                        //Logging.LogErrors(new Exception(error));
                        throw new Exception(error);
                    }
                }
            }


            public static class GlobalConfigs
            {

                public static string TenantId = Shared._DownstreamAuthOptionsDirect.TenantId;
                public static bool UseMSI = Shared._ApplicationOptions.UseMSI;
                public static string ApplicationId = Shared._DownstreamAuthOptionsDirect.ClientId;
                public static string AuthenticationKey = Shared._DownstreamAuthOptionsDirect.ClientSecret;
            }

            //ToDo: Consider Caching Configs so we dont fetch from key vault every time.
            public class KeyVaultConfigs
            {

                public static string GetMasterUserName(string KeyVaultToken, string KeyVault) { return GetKeyVaultSecret("MasterUserName", KeyVault, KeyVaultToken); }
                public static string GetMasterUserPassword(string KeyVaultToken, string KeyVault) { return GetKeyVaultSecret("MasterUserPassword", KeyVault, KeyVaultToken); }

            }

            public static string GetKeyVaultSecret(string Secret, string VaultName, string KeyVaultToken)
            {

                using (HttpClient client = new System.Net.Http.HttpClient())
                {
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", KeyVaultToken);
                    string queryString = string.Format("https://{0}.vault.azure.net/secrets/{1}?api-version=7.0", VaultName, Secret);
                    HttpResponseMessage result = client.GetAsync(queryString).Result;

                    if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        string content = result.Content.ReadAsStringAsync().Result;
                        var definition = new { value = "" };
                        var jobject = JsonConvert.DeserializeAnonymousType(content, definition);
                        return (jobject.value);
                    }
                    else
                    {
                        string error = "GetKeyVaultSecret Failed..";
                        try
                        {
                            string content = result.Content.ReadAsStringAsync().Result;
                            error = error + content;
                        }
                        catch
                        {

                        }
                        finally
                        {
                            //Logging.LogErrors(new Exception(error));
                            throw new Exception(error);
                        }
                    }
                }
            }

            public static void LogErrors(System.Exception e, Microsoft.Extensions.Logging.ILogger log)
            {
                //Logging.LogErrors(e);

            }

            public static string GetAzureRestApiToken(string ServiceURI, bool UseMSI)
            {
                if (UseMSI == true)
                {

                    AzureServiceTokenProvider tokenProvider = new AzureServiceTokenProvider();
                    //https://management.azure.com/                    
                    return tokenProvider.GetAccessTokenAsync(ServiceURI).Result;

                }
                else
                {


                    AuthenticationContext context = new AuthenticationContext("https://login.windows.net/" + Helpers.GlobalConfigs.TenantId);
                    ClientCredential cc = new ClientCredential(Helpers.GlobalConfigs.ApplicationId, Helpers.GlobalConfigs.AuthenticationKey);
                    AuthenticationResult result = context.AcquireTokenAsync(ServiceURI, cc).Result;
                    return result.AccessToken;
                }
            }

            public static Task<string> GetAzureRestApiTokenTask(string ServiceURI, bool UseMSI)
            {
                return Task.FromResult(GetAzureRestApiToken(ServiceURI, UseMSI));
            }

            public static AzureCredentials GetAzureCreds(bool UseMSI)
            {
                //MSI Login
                AzureCredentialsFactory f = new AzureCredentialsFactory();
                MSILoginInformation msi = new MSILoginInformation(MSIResourceType.AppService);
                AzureCredentials creds;


                if (UseMSI == true)
                {
                    //MSI
                    creds = f.FromMSI(msi, AzureEnvironment.AzureGlobalCloud);
                }
                else
                {
                    //Service Principal
                    creds = f.FromServicePrincipal(Helpers.GlobalConfigs.ApplicationId, Helpers.GlobalConfigs.AuthenticationKey, Helpers.GlobalConfigs.TenantId, AzureEnvironment.AzureGlobalCloud);

                }

                return creds;
            }
        }

    }
}
