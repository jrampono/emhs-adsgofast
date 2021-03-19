/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.ChangeFeed;
using Azure.Core;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Azure.Identity;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using AdsGoFast.SqlServer;
using System.Data;

namespace AdsGoFast
{

    public static class ProcessChangeFeedTrigger
    {
        [FunctionName("ProcessChangeFeed")]
        public static async Task Run([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            
            //var pcf = new ProcessChangeFeed("adsgofasttransientstg", DateTimeOffset.UtcNow.AddDays(-10), DateTimeOffset.UtcNow);
            
        }
    }


    public class ProcessChangeFeed
    {
        public ProcessChangeFeed(string BlobStorageAccountName, DateTimeOffset start, DateTimeOffset end)
        {
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            
            var res = ProcessChangeFeedTask(BlobStorageAccountName, start, end).Result;
            DataTable dt = new DataTable();
            dt.Columns.Add("EventTime", typeof(DateTimeOffset));
            dt.Columns.Add("EventType", typeof(string));
            dt.Columns.Add("Subject", typeof(string));
            dt.Columns.Add("Topic", typeof(string));
            dt.Columns.Add("EventData.BlobOperationName", typeof(string));
            dt.Columns.Add("EventData.BlobType", typeof(string));            
            foreach (var r in res)
            {
                DataRow dr = dt.NewRow();
                dr["EventTime"] = r.EventTime;
                dr["EventType"]= r.EventType.ToString();
                dr["Subject"] = r.Subject;
                dr["Topic"] = r.Topic;
                dr["EventData.BlobOperationName"] = r.EventData.BlobOperationName.ToString();
                dr["EventData.BlobType"] = r.EventData.BlobType.ToString();
                dt.Rows.Add(dr);
            }
            Table DestTable = new Table();
            DestTable.Name = "AzureStorageChangeFeed";
            DestTable.Schema = "dbo";
            TMD.BulkInsert(dt, DestTable, true);
                





        }

        private async Task<List<BlobChangeFeedEvent>> ProcessChangeFeedTask
        (string BlobStorageAccountName, DateTimeOffset start, DateTimeOffset end)
        {
           
            // Get a new blob service client.
            string BlobUrl = $"https://{BlobStorageAccountName}.blob.core.windows.net/";
            // Get a credential and create a client object for the blob container. Note using new Azure Core credential flow
            BlobServiceClient blobServiceClient = new BlobServiceClient(new Uri(BlobUrl),
                                                                            new DefaultAzureCredential());

            // Get a new change feed client.
            BlobChangeFeedClient changeFeedClient = blobServiceClient.GetChangeFeedClient();
            List<BlobChangeFeedEvent> changeFeedEvents = new List<BlobChangeFeedEvent>();

            IAsyncEnumerator<Page<BlobChangeFeedEvent>> enumerator = changeFeedClient
                .GetChangesAsync(start, end)
                .AsPages(pageSizeHint: 10)
                .GetAsyncEnumerator();

            await enumerator.MoveNextAsync();

            foreach (BlobChangeFeedEvent changeFeedEvent in enumerator.Current.Values)
            {
                changeFeedEvents.Add(changeFeedEvent);                
            }
                

            return (changeFeedEvents);
        }

        private void SaveCursor(string cursor)
        {
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            using SqlConnection _con = TMD.GetSqlConnection();
            TMD.ExecuteSql("Select null");
        }
    }
}

