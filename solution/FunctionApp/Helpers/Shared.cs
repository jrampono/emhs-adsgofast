/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Configuration;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using AdsGoFast.Models.Options;
using AdsGoFast.Services;

namespace AdsGoFast
{


    public partial class Shared
    {
        public static string _ApplicationBasePath {get;set;}
        // Hack for now -- need to add to Dependency Injection 
        public static ApplicationOptions _ApplicationOptions { get; set; }

        public static DownstreamAuthOptionsDirect _DownstreamAuthOptionsDirect { get; set; }
        public static AzureAuthenticationCredentialProvider _AzureAuthenticationCredentialProvider { get; set; }

        public static partial class Azure
        {

            public static class AzureSDK
            {
                
                //Gets AzureCredentials Object Using SDK Helper Classes 
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
                        creds = f.FromServicePrincipal(Shared._DownstreamAuthOptionsDirect.ClientId, Shared._DownstreamAuthOptionsDirect.ClientSecret, Shared._DownstreamAuthOptionsDirect.TenantId, AzureEnvironment.AzureGlobalCloud);

                    }

                    return creds;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            public static class Storage
            {                
                
                public static async void DeleteBlobFolder(string BlobStorageAccountName, string BlobStorageContainerName, string BlobStorageFolderPath, TokenCredential TokenCredential, Logging logging)
                {
                    try
                    {
                        using (HttpClient SourceClient = new HttpClient())
                        {
                            StorageCredentials storageCredentials = new StorageCredentials(TokenCredential);
                            CloudStorageAccount SourceStorageAccount = new CloudStorageAccount(storageCredentials, BlobStorageAccountName, "core.windows.net", true);

                            CloudBlobClient BlobClient = SourceStorageAccount.CreateCloudBlobClient();
                            CloudBlobContainer Container = BlobClient.GetContainerReference(BlobStorageContainerName);

                            CloudBlobDirectory Directory = Container.GetDirectoryReference(BlobStorageFolderPath);

                            BlobContinuationToken ContinuationToken = null;
                            List<IListBlobItem> Files = new List<IListBlobItem>();
                            do
                            {
                                BlobResultSegment response = await Directory.ListBlobsSegmentedAsync(ContinuationToken);
                                ContinuationToken = response.ContinuationToken;
                                Files.AddRange(response.Results);
                            }
                            while (ContinuationToken != null);


                            foreach (IListBlobItem f in Files)
                            {
                                CloudBlockBlob SourceBlob = (CloudBlockBlob)f;
                                await SourceBlob.DeleteIfExistsAsync();
                            }

                        }
                    }
                    catch (Exception e)
                    {
                        logging.LogErrors(e);
                        logging.LogErrors(new Exception("Initiation of Delete Failed:"));
                        throw e;

                    }
                }

                //public static async Task<Newtonsoft.Json.Linq.JObject> CheckCopyStatus(string BlobStorageAccountName, string BlobStorageContainerName, string BlobStorageFolderPath, TokenCredential TokenCredential)
                //{
                //    try
                //    {
                //        using (var SourceClient = new HttpClient())
                //        {       
                //            StorageCredentials storageCredentials = new StorageCredentials(TokenCredential);
                //            CloudStorageAccount StorageAccount = new CloudStorageAccount(storageCredentials, BlobStorageAccountName, "core.windows.net",true);

                //            CloudBlobClient BlobClient = StorageAccount.CreateCloudBlobClient();
                //            CloudBlobContainer Container = BlobClient.GetContainerReference(BlobStorageContainerName);

                //            CloudBlobDirectory Directory = Container.GetDirectoryReference(BlobStorageFolderPath);

                //            BlobContinuationToken ContinuationToken = null;
                //            List<IListBlobItem> Files = new List<IListBlobItem>();
                //            do
                //            {
                //                var response = await Directory.ListBlobsSegmentedAsync(ContinuationToken);
                //                ContinuationToken = response.ContinuationToken;
                //                Files.AddRange(response.Results);
                //            }
                //            while (ContinuationToken != null);



                //            int CopySucceededCount = 0;
                //            int CopyFailedCount = 0;
                //            int CopyInProgressCount = 0;

                //            foreach(var f in Files)
                //            {
                //                if (f.GetType() == typeof(CloudBlockBlob))
                //                {CloudBlockBlob Blob = (CloudBlockBlob)f;
                //                    Blob.FetchAttributesAsync().Wait();
                //                    if(Blob.CopyState == null)
                //                    {

                //                    }
                //                    else 
                //                    {
                //                        if (Blob.CopyState.Status == CopyStatus.Success)
                //                        {
                //                            CopySucceededCount += 1;
                //                        }
                //                        else 
                //                        {
                //                            if (Blob.CopyState.Status == CopyStatus.Failed || Blob.CopyState.Status == CopyStatus.Aborted || Blob.CopyState.Status == CopyStatus.Invalid)
                //                            {
                //                                CopyFailedCount += 1;
                //                            }
                //                            else 
                //                            {
                //                                CopyInProgressCount += 1;
                //                            }
                //                        }
                //                    }

                //                }        
                //            }

                //            Newtonsoft.Json.Linq.JObject result = Newtonsoft.Json.Linq.JObject.Parse(@"{CopySucceededCount:0, CopyFailedCount:0, CopyInProgressCount:0}");
                //            result["CopySucceededCount"] = CopySucceededCount;
                //            result["CopyFailedCount"] = CopyFailedCount;
                //            result["CopyInProgressCount"] = CopyInProgressCount;

                //            return result;                                
                //        }
                //    }
                //    catch (Exception e)
                //    {                           
                //        Logging.LogErrors(e);                            
                //        Logging.LogErrors(new Exception("Check of Copy Status Failed:"));
                //        throw e;            

                //    }
                //}

                //public static async void CopyBlobFolder(string SourceBlobStorageAccountName, string SourceBlobStorageContainerName, string SourceBlobStorageFolderPath, string TargetBlobStorageAccountName, string TargetBlobStorageContainerName, string TargetBlobStorageFolderPath, TokenCredential SourceTokenCredential, TokenCredential TargetTokenCredential)
                //{
                //        try
                //    {
                //        using (var SourceClient = new HttpClient())
                //        {       
                //            StorageCredentials storageCredentials = new StorageCredentials(SourceTokenCredential);
                //            CloudStorageAccount SourceStorageAccount = new CloudStorageAccount(storageCredentials, SourceBlobStorageAccountName, "core.windows.net",true);

                //            CloudBlobClient SourceBlobClient = SourceStorageAccount.CreateCloudBlobClient();
                //            CloudBlobContainer SourceContainer = SourceBlobClient.GetContainerReference(SourceBlobStorageContainerName);

                //            CloudBlobDirectory SourceDirectory = SourceContainer.GetDirectoryReference(SourceBlobStorageFolderPath);

                //            BlobContinuationToken SourceContinuationToken = null;
                //            List<IListBlobItem> SourceFiles = new List<IListBlobItem>();
                //            do
                //            {
                //                var response = await SourceDirectory.ListBlobsSegmentedAsync(SourceContinuationToken);
                //                SourceContinuationToken = response.ContinuationToken;
                //                SourceFiles.AddRange(response.Results);
                //            }
                //            while (SourceContinuationToken != null);

                //            using (var TargetClient = new HttpClient())
                //            {    
                //                StorageCredentials TargetStorageCredentials = new StorageCredentials(SourceTokenCredential);        
                //                CloudStorageAccount TargetStorageAccount = new CloudStorageAccount(storageCredentials, TargetBlobStorageAccountName, "core.windows.net",true);

                //                CloudBlobClient blobClient = TargetStorageAccount.CreateCloudBlobClient();
                //                CloudBlobContainer container = blobClient.GetContainerReference(TargetBlobStorageContainerName);

                //                CloudBlobDirectory TargetDirectory = container.GetDirectoryReference(TargetBlobStorageFolderPath);

                //                foreach(var f in SourceFiles)
                //                {
                //                    CloudBlockBlob SourceBlob = (CloudBlockBlob)f;
                //                    SourceBlob.FetchAttributesAsync().Wait();
                //                    var SplitName = SourceBlob.Name.Split('/');
                //                    var FileName = SplitName.Last();
                //                    CloudBlockBlob TargetBlob = TargetDirectory.GetBlockBlobReference(FileName);
                //                    await TargetBlob.StartCopyAsync(SourceBlob);  
                //                }                    
                //            }
                //        }
                //    }
                //    catch (Exception e)
                //    {                           
                //        Logging.LogErrors(e);                            
                //        Logging.LogErrors(new Exception("Initiation of Copy Failed:"));
                //        throw e;            

                //    }

                //}
                public static void UploadContentToBlob(string content, string BlobStorageAccountName, string BlobStorageContainerName, string BlobStorageFolderPath, string TargetFileName, TokenCredential tokenCredential)
                {
                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            //Write Content to blob
                            // Todo May need to make code async for larger files
                            //Need to make sure that MSI / Service Principal has write priveledges to blob storage account OR that you use SAS URI fetched from Key Vault
                            using (HttpClient storageclient = new HttpClient())
                            {
                                StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);
                                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, BlobStorageAccountName, "core.windows.net", true);

                                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                                CloudBlobContainer container = blobClient.GetContainerReference(BlobStorageContainerName);

                                CloudBlobDirectory directory = container.GetDirectoryReference(BlobStorageFolderPath);
                                CloudBlockBlob blob = directory.GetBlockBlobReference(TargetFileName);

                                blob.UploadTextAsync(content).Wait();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //Logging.LogErrors(e);                            
                        //Logging.LogErrors(new Exception("UploadContentToBlob Failed:"));
                        throw e;

                    }
                }

                public static string ReadFile(string BlobStorageAccountName, string BlobStorageContainerName, string BlobStorageFolderPath, string TargetFileName, TokenCredential tokenCredential)
                {

                    try
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            using (HttpClient storageclient = new HttpClient())
                            {

                                StorageCredentials storageCredentials = new StorageCredentials(tokenCredential);
                                CloudStorageAccount storageAccount = new CloudStorageAccount(storageCredentials, BlobStorageAccountName, "core.windows.net", true);

                                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                                CloudBlobContainer container = blobClient.GetContainerReference(BlobStorageContainerName);
                   
                                CloudBlobDirectory directory = container.GetDirectoryReference(BlobStorageFolderPath);
                                CloudBlockBlob blob = directory.GetBlockBlobReference(TargetFileName);
                                return blob.DownloadTextAsync().Result;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        //Logging.LogErrors(e);                            
                        //Logging.LogErrors(new Exception("Read file from blob Failed:"));
                        throw e;

                    }

                }


            }

 

            /// <summary>
            /// You can take this class and drop it into another project and use this code
            /// to create the headers you need to make a REST API call to Azure Storage.
            /// </summary>
            internal static class AzureStorageAuthenticationHelper
            {
                /// <summary>
                /// This creates the authorization header. This is required, and must be built 
                ///   exactly following the instructions. This will return the authorization header
                ///   for most storage service calls.
                /// Create a string of the message signature and then encrypt it.
                /// </summary>
                /// <param name="storageAccountName">The name of the storage account to use.</param>
                /// <param name="storageAccountKey">The access key for the storage account to be used.</param>
                /// <param name="now">Date/Time stamp for now.</param>
                /// <param name="httpRequestMessage">The HttpWebRequest that needs an auth header.</param>
                /// <param name="ifMatch">Provide an eTag, and it will only make changes
                /// to a blob if the current eTag matches, to ensure you don't overwrite someone else's changes.</param>
                /// <param name="md5">Provide the md5 and it will check and make sure it matches the blob's md5.
                /// If it doesn't match, it won't return a value.</param>
                /// <returns></returns>
                internal static System.Net.Http.Headers.AuthenticationHeaderValue GetAuthorizationHeader(
                   string storageAccountName, string storageAccountKey, DateTime now,
                   HttpRequestMessage httpRequestMessage, string ifMatch = "", string md5 = "")
                {
                    // This is the raw representation of the message signature.
                    HttpMethod method = httpRequestMessage.Method;
                    String MessageSignature = String.Format("{0}\n\n\n{1}\n{5}\n\n\n\n{2}\n\n\n\n{3}{4}",
                              method.ToString(),
                              (method == HttpMethod.Get || method == HttpMethod.Head) ? String.Empty
                                : httpRequestMessage.Content.Headers.ContentLength.ToString(),
                              ifMatch,
                              GetCanonicalizedHeaders(httpRequestMessage),
                              GetCanonicalizedResource(httpRequestMessage.RequestUri, storageAccountName),
                              md5);

                    // Now turn it into a byte array.
                    byte[] SignatureBytes = System.Text.Encoding.UTF8.GetBytes(MessageSignature);

                    // Create the HMACSHA256 version of the storage key.
                    HMACSHA256 SHA256 = new HMACSHA256(Convert.FromBase64String(storageAccountKey));

                    // Compute the hash of the SignatureBytes and convert it to a base64 string.
                    string signature = Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes));

                    // This is the actual header that will be added to the list of request headers.
                    // You can stop the code here and look at the value of 'authHV' before it is returned.
                    System.Net.Http.Headers.AuthenticationHeaderValue authHV = new System.Net.Http.Headers.AuthenticationHeaderValue("SharedKey",
                        storageAccountName + ":" + Convert.ToBase64String(SHA256.ComputeHash(SignatureBytes)));
                    return authHV;
                }

                /// <summary>
                /// Put the headers that start with x-ms in a list and sort them.
                /// Then format them into a string of [key:value\n] values concatenated into one string.
                /// (Canonicalized Headers = headers where the format is standardized).
                /// </summary>
                /// <param name="httpRequestMessage">The request that will be made to the storage service.</param>
                /// <returns>Error message; blank if okay.</returns>
                private static string GetCanonicalizedHeaders(HttpRequestMessage httpRequestMessage)
                {
                    var headers = from kvp in httpRequestMessage.Headers
                                  where kvp.Key.StartsWith("x-ms-", StringComparison.OrdinalIgnoreCase)
                                  orderby kvp.Key
                                  select new { Key = kvp.Key.ToLowerInvariant(), kvp.Value };

                    System.Text.StringBuilder sb = new System.Text.StringBuilder();

                    // Create the string in the right format; this is what makes the headers "canonicalized" --
                    //   it means put in a standard format. http://en.wikipedia.org/wiki/Canonicalization
                    foreach (var kvp in headers)
                    {
                        System.Text.StringBuilder headerBuilder = new System.Text.StringBuilder(kvp.Key);
                        char separator = ':';

                        // Get the value for each header, strip out \r\n if found, then append it with the key.
                        foreach (string headerValues in kvp.Value)
                        {
                            string trimmedValue = headerValues.TrimStart().Replace("\r\n", String.Empty);
                            headerBuilder.Append(separator).Append(trimmedValue);

                            // Set this to a comma; this will only be used 
                            //   if there are multiple values for one of the headers.
                            separator = ',';
                        }
                        sb.Append(headerBuilder.ToString()).Append("\n");
                    }
                    return sb.ToString();
                }

                /// <summary>
                /// This part of the signature string represents the storage account 
                ///   targeted by the request. Will also include any additional query parameters/values.
                /// For ListContainers, this will return something like this:
                ///   /storageaccountname/\ncomp:list
                /// </summary>
                /// <param name="address">The URI of the storage service.</param>
                /// <param name="accountName">The storage account name.</param>
                /// <returns>String representing the canonicalized resource.</returns>
                private static string GetCanonicalizedResource(Uri address, string storageAccountName)
                {
                    // The absolute path is "/" because for we're getting a list of containers.
                    System.Text.StringBuilder sb = new System.Text.StringBuilder("/").Append(storageAccountName).Append(address.AbsolutePath);

                    // Address.Query is the resource, such as "?comp=list".
                    // This ends up with a NameValueCollection with 1 entry having key=comp, value=list.
                    // It will have more entries if you have more query parameters.
                    System.Collections.Specialized.NameValueCollection values = System.Web.HttpUtility.ParseQueryString(address.Query);

                    foreach (var item in values.AllKeys.OrderBy(k => k))
                    {
                        sb.Append('\n').Append(item).Append(':').Append(values[item]);
                    }

                    return sb.ToString().ToLower();

                }
            }


        }
            public static class GlobalConfigs
            {
                
                public static string GetStringRequestParam(string Name, HttpRequest req, string reqbody)
                {
                    try
                    {

                        string ret;

                        if (req.Method == HttpMethod.Get.ToString())
                        {
                            ret = req.Query[Name].ToString();
                        }
                        else
                        {
                            dynamic parsed = JsonConvert.DeserializeObject(reqbody);
                            ret = parsed[Name].Value.ToString();
                        }
                        return ret;
                    }
                    catch (Exception e)
                    {
                        // Logging.LogErrors(new Exception ("Could not bind input parameter " + Name + " using the request querystring nor the request body."));
                        throw e;
                    }
                }


                public static string GetStringConfig(string ConfigName)
                {
                    return (string)GetConfig(ConfigName);
                }
                public static bool GetBoolConfig(string ConfigName)
                {
                    return System.Convert.ToBoolean(GetConfig(ConfigName));
                }

                public static short GetInt16Config(string ConfigName)
                {
                    return System.Convert.ToInt16(GetConfig(ConfigName));
                }


                private static object GetConfig(string ConfigName)
                {
                    object Ret;
                try
                {

                    //Todo refactor config helper to use dependency injection
                    var config = new ConfigurationBuilder()
                      .SetBasePath(_ApplicationBasePath)
                      .AddEnvironmentVariables()
                      .AddUserSecrets("3956e7aa-4d13-430a-bb5f-a5f8f5a450ee", true)
                      .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                      .Build();
                    
                    Ret = config[ConfigName];
                }
                catch (Exception e)
                {
                    //Logging.LogErrors(new Exception ("Could not find global config " + ConfigName));
                    throw (e);
                }

                    return Ret;
                }

                public static string KeyVaultToken { get; set; }
                //public static void SetKeyVaultToken()
                //{
                //    string resource = null;
                //    resource = "https://vault.azure.net";

                //    Logging.LogInformation("Getting Key Vault Token");  
                //    //Always Set Using Local Dev SP or MSI 
                //    KeyVaultToken = Shared.Azure.AuthenticateAsyncViaRest(Shared.GlobalConfigs.GetBoolConfig("UseMSI"),resource, "https://login.microsoftonline.com/"+Shared.GlobalConfigs.GetStringConfig("TenantId")+"/oauth2/token", Shared.GlobalConfigs.GetStringConfig("ApplicationId"), Shared.GlobalConfigs.GetStringConfig("AuthenticationKey"), null, null,"https://vault.azure.net/.default","client_credentials"); 
                //}

                public static string GetKeyVaultSecret(string Secret, string VaultName, string KeyVaultToken)
                {

                    using (HttpClient client = new HttpClient())
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
                            string error = "GetKeyVaultSecret Failed - Secret Name: " + Secret;
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


            }

            //ToDo: Consider Caching Configs so we dont fetch from key vault every time.


        
         public static class JsonHelpers
            {
                public static string GetStringValueFromJSON(Logging logging, string PropertyName, Newtonsoft.Json.Linq.JObject SourceObject, string DefaultValue, bool LogErrorIfNotFound)
                {
                    string ret = "";
                    if ((SourceObject.TryGetValue(PropertyName, out JToken TokenDump)) == true)
                    {
                        ret = TokenDump.ToString();
                    }
                    else
                    {
                        if (LogErrorIfNotFound)
                        {
                            logging.LogWarning(string.Format("Json Element '{0}' not found",PropertyName)); 
                        }
                        ret = DefaultValue;
                    }
                    return ret;
                }

                public static dynamic GetDynamicValueFromJSON(Logging logging, string PropertyName, Newtonsoft.Json.Linq.JObject SourceObject, string DefaultValue, bool LogErrorIfNotFound)
                {
                    dynamic ret;
                    if ((SourceObject.TryGetValue(PropertyName, out JToken TokenDump)) == true)
                    {
                        ret = TokenDump.ToString();
                    }
                    else
                    {
                        if (LogErrorIfNotFound)
                        {
                            logging.LogWarning(string.Format("Json Element '{0}' not found", PropertyName)); 
                        }
                        ret = DefaultValue;
                    }
                    return ret;
                }

                public static bool CheckForJSONProperty(Logging logging, string Property, Newtonsoft.Json.Linq.JObject O)
                {
                    bool ret = false;
                    ret = O.TryGetValue(Property, out JToken TokenDump);
                    return ret;

                }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="logging"></param>
            /// <param name="propertyList"></param>
            /// <param name="JSONString"></param>
            /// <returns></returns>
            public static bool BuildJsonObjectWithValidation(Logging logging, ref JsonObjectPropertyList propertyList, string JSONString, ref JObject TargetJObject)
            {
                bool ObjectValid = false;
                if (IsValidJson(JSONString))
                {
                    ObjectValid = true;
                    JObject jobj = JObject.Parse(JSONString);
                    foreach (JsonObjectProperty p in propertyList)
                    {
                        if (CheckForJSONProperty(logging, p.SourcePropertyName, jobj))
                        {
                            TargetJObject[p.TargetPropertyName] = jobj[(p.SourcePropertyName)];
                            p.WasFound = true;
                        }
                        else
                        {
                            if (p.RaiseErrorIfNotFound)
                            {
                                logging.LogErrors(new Exception("Property " + p.SourcePropertyName + " was not found."));
                                ObjectValid = false;
                            }
                            else
                            {
                                if (p.RaiseWarningIfNotFound)
                                {                                    
                                    logging.LogWarning("Property " + p.SourcePropertyName + " was not found.");
                                }
                            }

                        p.WasFound = false;
                        }
                    }
                }
                else
                {
                    ObjectValid = false;
                }
                return ObjectValid;
            }

            public static bool ValidateJsonUsingSchema(Logging logging, string SchemaAsString, string JsonObjectToValidate, string ErrorComment)
            {

                bool ret = true;
                var schema = NJsonSchema.JsonSchema.FromJsonAsync(SchemaAsString).Result;
                var schemaData = schema.ToJson();
                var errors = schema.Validate(JsonObjectToValidate);
                if (errors.Any())
                {
                    
                    foreach (var error in errors)
                    {
                        //Supressing Errors on non required 
                        if (error.Schema.RequiredProperties.Count != 0)
                        {
                            ret = false;
                            logging.LogErrors(new Exception(ErrorComment + "Error: " + error.Path + " : " + error.Kind));
                        }
                    }
                }
                return ret;
            }

            public static bool IsValidJson(string strInput)
            {
                if (string.IsNullOrWhiteSpace(strInput)) { return false; }
                strInput = strInput.Trim();
                if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                    (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
                {
                    try
                    {
                        var obj = JToken.Parse(strInput);
                        return true;
                    }
                    catch (JsonReaderException jex)
                    {
                        //Exception in parsing json
                        
                        return false;
                    }
                    catch (Exception ex) //some other exception
                    {                        
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            public class JsonObjectPropertyList: IEnumerable<JsonObjectProperty>
            {
                public JsonObjectPropertyList()
                {
                    Properties = new List<JsonObjectProperty>();
                }

                public void Add(string SourcePropertyName) {
                    Properties.Add(new JsonObjectProperty(SourcePropertyName));
                }

                public void Add(string SourcePropertyName, bool raiseErrorIfNotFound, bool raiseWarningIfNotFound)
                {
                    Properties.Add(new JsonObjectProperty(SourcePropertyName, raiseErrorIfNotFound, raiseWarningIfNotFound));
                }

                public void Add(string SourcePropertyName, string TargetPropertyName, bool raiseErrorIfNotFound, bool raiseWarningIfNotFound)
                {
                    Properties.Add(new JsonObjectProperty(SourcePropertyName, TargetPropertyName, raiseErrorIfNotFound, raiseWarningIfNotFound));
                }

                public IEnumerator<JsonObjectProperty> GetEnumerator()
                {
                    return ((IEnumerable<JsonObjectProperty>)Properties).GetEnumerator();
                }

                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
                {
                    return ((System.Collections.IEnumerable)Properties).GetEnumerator();
                }

                public List<JsonObjectProperty> Properties { get; set; }



            }
            public class JsonObjectProperty 
            {
                public JsonObjectProperty(string sourcePropertyName, bool raiseErrorIfNotFound, bool raiseWarningIfNotFound)
                {
                    this.SourcePropertyName = sourcePropertyName;
                    this.TargetPropertyName = sourcePropertyName;
                    this.RaiseErrorIfNotFound = raiseErrorIfNotFound;
                    this.RaiseWarningIfNotFound = raiseWarningIfNotFound;
                }

                public JsonObjectProperty(string sourcePropertyName)
                {
                    this.SourcePropertyName = sourcePropertyName;
                    this.TargetPropertyName = sourcePropertyName;
                    this.RaiseErrorIfNotFound = true;
                    this.RaiseWarningIfNotFound = true;
                }

                public JsonObjectProperty(string sourcePropertyName, string targetPropertyName, bool raiseErrorIfNotFound, bool raiseWarningIfNotFound)
                {
                    this.SourcePropertyName = sourcePropertyName;
                    this.TargetPropertyName = targetPropertyName;
                    this.RaiseErrorIfNotFound = raiseErrorIfNotFound;
                    this.RaiseWarningIfNotFound = raiseWarningIfNotFound;
                }
                public string SourcePropertyName { get; set; }
                public string TargetPropertyName { get; set; }

                public bool RaiseErrorIfNotFound { get; set; }
                public bool RaiseWarningIfNotFound { get; set; }

                public bool WasFound { get; set; }
            }

        }

        }
    
}

    
