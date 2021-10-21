/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Newtonsoft.Json.Linq;
using System;
using Azure.Storage.Sas;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Identity;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Helpers.Errors.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.IO;
using FormatWith;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage.Table;

namespace AdsGoFast
{
    public static class GetSASUriSendEmailHttpTrigger
    {
        [FunctionName("GetSASUriSendEmailHttpTrigger")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req, ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = SendEmailSASUri;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "GetSASUriSendEmailHttpTrigger", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }
        }

        public static async Task<JObject> SendEmailSASUri(HttpRequest req, Logging logging)
        {
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic taskInformation = JsonConvert.DeserializeObject(requestBody);

            string _TaskInstanceId = taskInformation["TaskInstanceId"].ToString();
            string _ExecutionUid = taskInformation["ExecutionUid"].ToString();

            try
            {

                //Get SAS URI
                string _blobStorageAccountName = taskInformation["Source"]["StorageAccountName"].ToString();
                string _blobStorageContainerName = taskInformation["Source"]["StorageAccountContainer"].ToString();
                string _blobStorageFolderPath = taskInformation["Source"]["RelativePath"].ToString();
                string _dataFileName = taskInformation["Source"]["DataFileName"].ToString();
                int _accessDuration = (int)taskInformation["Source"]["SasURIDaysValid"];
                string _targetSystemUidInPHI = taskInformation["Source"]["TargetSystemUidInPHI"];
                string _FileUploaderWebAppURL = taskInformation["Source"]["FileUploaderWebAppURL"];

                string SASUri = Storage.CreateSASToken(_blobStorageAccountName, _blobStorageContainerName, _blobStorageFolderPath, _dataFileName, _accessDuration);

                //Send Email
                string _emailRecipient = taskInformation["Target"]["EmailRecipient"].ToString();
                string _emailRecipientName = taskInformation["Target"]["EmailRecipientName"].ToString();
                string _emailTemplateFileName = taskInformation["Target"]["EmailTemplateFileName"].ToString();
                string _senderEmail = taskInformation["Target"]["SenderEmail"].ToString();
                string _senderDescription = taskInformation["Target"]["SenderDescription"].ToString();
                string _subject = taskInformation["Target"]["EmailSubject"].ToString();

                //Get Plain Text and Email Subject from Template Files 
                Dictionary<string, string> Params = new Dictionary<string, string>
                {
                    { "NAME", _emailRecipientName },
                    { "SASTOKEN", SASUri },
                    { "FileUploaderUrl", _FileUploaderWebAppURL },
                    { "TargetSystemUidInPHI", _targetSystemUidInPHI },

                };
                string _plainTextContent = System.IO.File.ReadAllText(System.IO.Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.HTMLTemplateLocation,_emailTemplateFileName + ".txt"));
                _plainTextContent = _plainTextContent.FormatWith(Params, MissingKeyBehaviour.ThrowException, null, '{', '}');

                string _htmlContent = System.IO.File.ReadAllText(System.IO.Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.HTMLTemplateLocation, _emailTemplateFileName + ".html"));
                _htmlContent = _htmlContent.FormatWith(Params, MissingKeyBehaviour.ThrowException, null, '{', '}');

                var apiKey = System.Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
                var client = new SendGridClient(new SendGridClientOptions { ApiKey = apiKey, HttpErrorAsException = true });
                var msg = new SendGridMessage()
                {
                    From = new EmailAddress(_senderEmail, _senderDescription),
                    Subject = _subject,
                    PlainTextContent = _plainTextContent,
                    HtmlContent = _htmlContent
                };
                msg.AddTo(new EmailAddress(_emailRecipient, _emailRecipientName));
                try
                {
                    var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
                    logging.LogInformation($"SendGrid Response StatusCode - {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    SendGridErrorResponse errorResponse = JsonConvert.DeserializeObject<SendGridErrorResponse>(ex.Message);
                    logging.LogInformation($"Error Message - {ex.Message}");
                    throw new Exception("Could not send email");
                }

                //Update Task Instace

                TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
                TMD.LogTaskInstanceCompletion(System.Convert.ToInt64(_TaskInstanceId), System.Guid.Parse(_ExecutionUid), TaskMetaData.BaseTasks.TaskStatus.Complete, Guid.Empty, "");

                JObject Root = new JObject
                {
                    ["Result"] = "Complete"
                };

                return Root;
            }
            catch (Exception TaskException)
            {
                logging.LogErrors(TaskException);
                TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
                TMD.LogTaskInstanceCompletion(System.Convert.ToInt64(_TaskInstanceId), System.Guid.Parse(_ExecutionUid), TaskMetaData.BaseTasks.TaskStatus.FailedRetry, Guid.Empty, "Failed when trying to Generate Sas URI and Send Email");

                JObject Root = new JObject
                {
                    ["Result"] = "Failed"
                };

                return Root;

            }

        }

    }

    public static class TestSasUriSendEmailHttpTrigger
    {
        [FunctionName("TestSASUriSendEmailHttpTrigger")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req, ILogger log, ExecutionContext context)
        {
  
            string Path = req.Query["Path"].ToString();
            var sas = "https://adsgofasttransientstg.blob.core.windows.net/" + Path + "?" ;
            foreach (var i in req.Query)
            {
                if (i.Key != "Path" && i.Key != "TargetSystemUidInPHI")
                {
                    sas += "&" + i.Key + "=" + i.Value;
                }
            }           

            var cloudBlockBlob = new Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob(new Uri(sas));
            await cloudBlockBlob.UploadTextAsync("Hello World");

            //Write to Filelist table so that downstream tasks can be triggered efficiently
            var _storageCredentials = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials("?sv=2018-03-28&tn=Filelist&sig=MFbvVgbNLs3UjqAPfU%2BYwQqxcTYwCPnNKCwCUp4XRmo%3D&se=2021-09-06T23%3A15%3A57Z&sp=au");
            var SourceStorageAccount = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(storageCredentials: _storageCredentials, accountName: "adsgofasttransientstg", endpointSuffix: "core.windows.net", useHttps: true);
            var client = SourceStorageAccount.CreateCloudTableClient();
            Microsoft.WindowsAzure.Storage.Table.CloudTable table = client.GetTableReference("Filelist");
            var _dict = new Dictionary<string, EntityProperty>();
            _dict.Add("FilePath", new EntityProperty(Path));
            var _tableEntity = new DynamicTableEntity(DateTime.UtcNow.ToString("yyyy-MM-dd hh:mm"),Guid.NewGuid().ToString(), null, _dict);
            var _tableOperation = TableOperation.Insert(_tableEntity);
            try
            {
                await table.ExecuteAsync(_tableOperation);
            }
            catch (Microsoft.WindowsAzure.Storage.StorageException ex)
            { 
            
            }
            return new OkObjectResult(new { });
        }
    }

    public static class Storage
    {
        public static string CreateSASToken(string BlobStorageAccountName, string BlobStorageContainerName, string BlobStorageFolderPath, string DataFileName, int accessDuration)
        {

            // Get a credential and create a client object for the blob container. Note using new Azure Core credential flow
            BlobServiceClient blobClient = new BlobServiceClient(new Uri(BlobStorageAccountName),
                                                                            new DefaultAzureCredential());
            
            //blobClient.GetProperties();
            var startDate = DateTimeOffset.UtcNow.AddMinutes(-1);
            var endDate = DateTimeOffset.UtcNow.AddDays(accessDuration);

            // Get a user delegation key for the Blob service that's valid for seven days.
            // You can use the key to generate any number of shared access signatures over the lifetime of the key.
            UserDelegationKey key = blobClient.GetUserDelegationKey(startDate,endDate);
            Uri BlobUri = new Uri(BlobStorageAccountName);
            BlobContainerClient containerClient = new BlobContainerClient(BlobUri, new DefaultAzureCredential());
            // Create a SAS token
            BlobSasBuilder sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = BlobStorageContainerName,
                BlobName = String.Format("{0}{1}",BlobStorageFolderPath,DataFileName),
                Resource = "b",
                StartsOn = startDate,
                ExpiresOn = endDate,
                Protocol = SasProtocol.Https
            };

            // Specify read permissions for the SAS.
            BlobSasPermissions perms = BlobSasPermissions.Create | BlobSasPermissions.Write;
            
            sasBuilder.SetPermissions(perms);

            // Use the key to get the SAS token.
            string sasToken = sasBuilder.ToSasQueryParameters(key, BlobUri.Host.Split('.')[0]).ToString();

            // Construct the full URI, including the SAS token.
            UriBuilder fullUri = new UriBuilder()
            {
                Scheme = "https",
                Host = string.Format("{0}.blob.core.windows.net", BlobUri.Host.Split('.')[0]),
                Path = string.Format("{0}/{1}{2}", BlobStorageContainerName, BlobStorageFolderPath,DataFileName),
                Query = sasToken
            };

            string retvar = "&Path=" + Uri.EscapeDataString(fullUri.Path.ToString());
            retvar += "&" + sasToken;

            return retvar;
        }

    }
}

