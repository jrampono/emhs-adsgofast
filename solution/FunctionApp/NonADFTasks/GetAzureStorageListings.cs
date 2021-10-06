/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using AdsGoFast.SqlServer;
using AdsGoFast.TaskMetaData;
using Cronos;
using Dapper;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using FormatWith;
using Microsoft.Azure.Management.Network.Fluent.Models;
using AdsGoFast;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace AdsGoFast
{

    public static class AZStorageCacheFileListGenerateSAS
    {
        /// <summary>
        /// Use this function to generate the SASURI for the StorageFileCache Tasks
        /// </summary>
        
        /// <returns></returns>
        [FunctionName("AZStorageCacheFileListGenerateSAS")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {

            //Get SASURI for table access
            string requestBody = new System.IO.StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            
           
            string _storageAccountName = data["StorageAccountName"].ToString();
            string _storageAccountKey = data["StorageAccountKey"].ToString();

            StorageCredentials _storageCredentials = new StorageCredentials(_storageAccountName, _storageAccountKey);
            CloudStorageAccount SourceStorageAccount = new CloudStorageAccount(storageCredentials: _storageCredentials, accountName: _storageAccountName, endpointSuffix: "core.windows.net", useHttps: true);
            CloudTableClient client = SourceStorageAccount.CreateCloudTableClient();
            var FileListTable = client.GetTableReference("Filelist");

            SharedAccessTablePolicy policy = new SharedAccessTablePolicy()
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddYears(1),
                Permissions = SharedAccessTablePermissions.Add | SharedAccessTablePermissions.Update
            };

            var sasToken = FileListTable.GetSharedAccessSignature(policy);
            var sasCredentials = new StorageCredentials(sasToken);

            //Run in Debug and break on this point so that you can grab the SASURI
            return new OkObjectResult(new { });

        }


    }

    public static class AZStorageCacheFileListHttpTrigger
    {
        [FunctionName("AZStorageCacheFileListHttpTrigger")]
        public static async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = GetAzureStorageListings.GetAzureStorageListingsCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "AZStorageCacheFileListHttpTrigger", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }


        }


    }

    public static class GetAzureStorageListings
    {
        public class FilelistEntity : TableEntity
        {
            public FilelistEntity() { }
        }


        public static dynamic GetAzureStorageListingsCore(HttpRequest req, Logging logging)
        {
            string requestBody = new System.IO.StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic taskInformation = JsonConvert.DeserializeObject(requestBody);
            string _TaskInstanceId = taskInformation["TaskInstanceId"].ToString();
            string _ExecutionUid = taskInformation["ExecutionUid"].ToString();
            try
            {

                string _storageAccountName = taskInformation["Source"]["StorageAccountName"];
                //The name is actually the base url so we need to parse it to get the name only
                _storageAccountName = _storageAccountName.Split('.')[0].Replace("https://", "");
                string _storageAccountToken = taskInformation["Source"]["StorageAccountToken"];
                Int64 _SourceSystemId = taskInformation["Source"]["SystemId"];

                TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
                using SqlConnection _con = TMD.GetSqlConnection();

                var res = _con.QueryWithRetry(string.Format("Select Max(PartitionKey) MaxPartitionKey from AzureStorageListing where SystemId = {0}", _SourceSystemId.ToString()));

                string MaxPartitionKey = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd hh:mm");

                foreach (var r in res)
                {
                    if (r.MaxPartitionKey != null)
                    {
                        MaxPartitionKey = DateTime.Parse(r.MaxPartitionKey).AddMinutes(-1).ToString("yyyy-MM-dd hh:mm");
                    }
                }

                using (HttpClient SourceClient = new HttpClient())
                {

                    //Now use the SAS URI to connect rather than the MSI / Service Principal as AD Based Auth not yet avail for tables
                    var _storageCredentials = new StorageCredentials(_storageAccountToken);
                    var SourceStorageAccount = new CloudStorageAccount(storageCredentials: _storageCredentials, accountName: _storageAccountName, endpointSuffix: "core.windows.net", useHttps: true);
                    var client = SourceStorageAccount.CreateCloudTableClient();

                    CloudTable table = client.GetTableReference("Filelist");

                    TableQuery<DynamicTableEntity> query = new TableQuery<DynamicTableEntity>().Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.GreaterThan, MaxPartitionKey.ToString()));

                    DataTable dt = new DataTable();
                    DataColumn dc = new DataColumn();
                    dc.ColumnName = "PartitionKey";
                    dc.DataType = typeof(string);
                    dt.Columns.Add(dc);
                    DataColumn dc1 = new DataColumn();
                    dc1.ColumnName = "RowKey";
                    dc1.DataType = typeof(string);
                    dt.Columns.Add(dc1);
                    DataColumn dc2 = new DataColumn();
                    dc2.ColumnName = "SystemId";
                    dc2.DataType = typeof(Int64);
                    dt.Columns.Add(dc2);
                    DataColumn dc3 = new DataColumn();
                    dc3.ColumnName = "FilePath";
                    dc3.DataType = typeof(string);
                    dt.Columns.Add(dc3);

                    string Filelist = "";
                    TableContinuationToken token = null;
                    do
                    {
                        TableQuerySegment<DynamicTableEntity> resultSegment = table.ExecuteQuerySegmentedAsync(query, token).Result;
                        token = resultSegment.ContinuationToken;

                        //load into data table
                        foreach (var entity in resultSegment.Results)
                        {

                            DataRow dr = dt.NewRow();
                            dr["PartitionKey"] = entity.PartitionKey;
                            dr["RowKey"] = entity.RowKey;
                            dr["SystemId"] = _SourceSystemId;
                            dr["FilePath"] = entity.Properties["FilePath"].StringValue;
                            dt.Rows.Add(dr);
                            Filelist += entity.Properties["FilePath"].StringValue + System.Environment.NewLine;

                        }
                    } while (token != null);

                    
                    if (dt.Rows.Count > 0)
                    {
                        Table t = new Table();
                        t.Schema = "dbo";
                        string TableGuid = Guid.NewGuid().ToString();
                        t.Name = $"#AzureStorageListing{TableGuid}";

                        TMD.BulkInsert(dt, t, true, _con);
                        Dictionary<string, string> SqlParams = new Dictionary<string, string>
                        {
                            { "TempTable", t.QuotedSchemaAndName() },
                            { "SourceSystemId", _SourceSystemId.ToString()}
                        };
                        
                        string MergeSQL = GenerateSQLStatementTemplates.GetSQL(System.IO.Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.SQLTemplateLocation), "MergeIntoAzureStorageListing", SqlParams);
                        _con.ExecuteWithRetry(MergeSQL, 120);
                        if ((JArray)taskInformation["Alerts"] != null)
                        {
                            foreach (JObject Alert in (JArray)taskInformation["Alerts"])
                            {
                                //Only Send out for Operator Level Alerts
                                if (Alert["AlertCategory"].ToString() == "Task Specific Operator Alert")
                                {
                                    AlertOperator(_SourceSystemId, Alert["AlertEmail"].ToString(), "", Filelist);
                                }
                            }
                        }
                    }
                    
                    _con.Close();
                    _con.Dispose();
                    TMD.LogTaskInstanceCompletion(System.Convert.ToInt64(_TaskInstanceId), System.Guid.Parse(_ExecutionUid), TaskMetaData.BaseTasks.TaskStatus.Complete, Guid.Empty, "");


                }
            }

            catch (Exception e)
            {
                logging.LogErrors(e);
                TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
                TMD.LogTaskInstanceCompletion(System.Convert.ToInt64(_TaskInstanceId), System.Guid.Parse(_ExecutionUid), TaskMetaData.BaseTasks.TaskStatus.FailedRetry, Guid.Empty, "Failed when trying to Generate Sas URI and Send Email");

                JObject Root = new JObject
                {
                    ["Result"] = "Failed"
                };

                return Root;

            }
            return new { };

        }

        public static void AlertOperator(Int64 SourceSystemId, string EmailRecipient, string EmailRecipientName, string FileList)
        {
            //Send Email 
            string _senderEmail = System.Environment.GetEnvironmentVariable("DefaultSentFromEmailAddress");
            string _senderDescription = System.Environment.GetEnvironmentVariable("DefaultSentFromEmailName"); ;
            string _subject = "New files in Watched Azure Storage account. SourceSystemId is "+ SourceSystemId.ToString();

            string _plainTextContent = "New files have been dropped into a watched Azure Storage account. SourceSystemId is " + SourceSystemId.ToString() + ". Files found are: " + System.Environment.NewLine + FileList;

            var apiKey = System.Environment.GetEnvironmentVariable("SENDGRID_APIKEY");
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress(_senderEmail, _senderDescription),
                Subject = _subject,
                PlainTextContent = _plainTextContent
            };
            msg.AddTo(new EmailAddress(EmailRecipient, EmailRecipientName));
            var res = client.SendEmailAsync(msg).Result;

        }

       
    }
}









