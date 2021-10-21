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
using Microsoft.Extensions.Options;
using AdsGoFast.Models.Options;
using AdsGoFast.Models;

namespace AdsGoFast
{

    public  class GetActivityLevelLogsTimerTrigger
    {
        private readonly IOptions<ApplicationOptions> _appOptions;
        private readonly IAppInsightsContext _appInsightsContext;

        public GetActivityLevelLogsTimerTrigger(IOptions<ApplicationOptions> appOptions, IAppInsightsContext appInsightsContext)
        {
            _appOptions = appOptions;
            _appInsightsContext = appInsightsContext;
        }

        [FunctionName("GetActivityLevelLogs")]
        public  async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;

            if (_appOptions.Value.TimerTriggers.EnableGetADFStats)
            {
                using (FrameworkRunner FR = new FrameworkRunner(log, ExecutionId))
                {
                    FrameworkRunner.FrameworkRunnerWorker worker = GetActivityLevelLogsCore;
                    FrameworkRunner.FrameworkRunnerResult result = FR.Invoke("GetActivityLevelLogs", worker);
                }
            }
        }

        public dynamic GetActivityLevelLogsCore(Logging logging)
        {
            string AppInsightsWorkspaceId = _appOptions.Value.ServiceConnections.AppInsightsWorkspaceId;
            using var client = _appInsightsContext.httpClient.CreateClient(_appInsightsContext.httpClientName);

            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            using SqlConnection _conRead = TMD.GetSqlConnection();

            //Get Last Request Date
            var MaxTimesGenQuery = _conRead.QueryWithRetry(@"
                                    select max([timestamp]) maxtimestamp from ActivityLevelLogs");        

            foreach (var datafactory in MaxTimesGenQuery)
            {
                DateTimeOffset MaxAllowedLogTimeGenerated = DateTimeOffset.UtcNow.AddDays(-1*_appOptions.Value.ServiceConnections.AppInsightsMaxNumberOfDaysToRequest);
                DateTimeOffset MaxObservedLogTimeGenerated = DateTimeOffset.UtcNow.AddDays(-1*_appOptions.Value.ServiceConnections.AppInsightsMaxNumberOfDaysToRequest);
                if (datafactory.maxtimestamp != null)
                {
                    MaxObservedLogTimeGenerated = ((DateTimeOffset)datafactory.maxtimestamp).AddMinutes(-1* _appOptions.Value.ServiceConnections.AppInsightsMinutesOverlap);
                    //Make sure that we don't get more than max to ensure we dont get timeouts etc.
                    if ((MaxObservedLogTimeGenerated) <= MaxAllowedLogTimeGenerated)
                    {
                        MaxObservedLogTimeGenerated = MaxAllowedLogTimeGenerated;
                    }
                }

                //string workspaceId = datafactory.LogAnalyticsWorkspaceId.ToString();

                Dictionary<string, object> KqlParams = new Dictionary<string, object>
                {
                    {"MaxLogTimeGenerated", MaxObservedLogTimeGenerated.ToString("yyyy-MM-dd HH:mm:ss.ff K") }
                    //{"SubscriptionId", ((string)datafactory.SubscriptionUid.ToString()).ToUpper()},
                    //{"ResourceGroupName", ((string)datafactory.ResourceGroup.ToString()).ToUpper() },
                    //{"DataFactoryName", ((string)datafactory.Name.ToString()).ToUpper() },
                    //{"DatafactoryId", datafactory.Id.ToString()  }
                };

                string KQL = System.IO.File.ReadAllText(System.IO.Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.KQLTemplateLocation, "GetActivityLevelLogs.kql"));
                KQL = KQL.FormatWith(KqlParams, FormatWith.MissingKeyBehaviour.ThrowException, null, '{', '}');

                JObject JsonContent = new JObject();
                JsonContent["query"] = KQL;

                var postContent = new StringContent(JsonContent.ToString(), System.Text.Encoding.UTF8, "application/json");

                var response = client.PostAsync($"https://api.applicationinsights.io/v1/apps/{AppInsightsWorkspaceId}/query", postContent).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //Start to parse the response content
                    HttpContent responseContent = response.Content;
                    var content = response.Content.ReadAsStringAsync().Result;
                    var tables = ((JArray)(JObject.Parse(content)["tables"]));
                    if (tables.Count > 0)
                    {
                        DataTable dt = new DataTable();

                        var rows = (JArray)(tables[0]["rows"]);
                        var columns = (JArray)(tables[0]["columns"]);
                        foreach (JObject c in columns)
                        {
                            DataColumn dc = new DataColumn();
                            dc.ColumnName = c["name"].ToString();
                            dc.DataType = KustoDataTypeMapper[c["type"].ToString()];
                            dt.Columns.Add(dc);
                        }


                        foreach (JArray r in rows)
                        {
                            DataRow dr = dt.NewRow();
                            for (int i = 0; i < columns.Count; i++)
                            {
                                if (((Newtonsoft.Json.Linq.JValue)r[i]).Value != null)
                                {
                                    dr[i] = ((Newtonsoft.Json.Linq.JValue)r[i]).Value;
                                }
                                else
                                {
                                    dr[i] = DBNull.Value;
                                }
                            }
                            dt.Rows.Add(dr);
                        }

                        Table t = new Table();
                        t.Schema = "dbo";
                        string TableGuid = Guid.NewGuid().ToString();
                        t.Name = "#ActivityLevelLogs{TableGuid}";
                        using (SqlConnection _conWrite = TMD.GetSqlConnection())
                        {
                            TMD.BulkInsert(dt, t, true, _conWrite);
                            Dictionary<string, string> SqlParams = new Dictionary<string, string>
                            {
                                { "TempTable", t.QuotedSchemaAndName() },
                                { "DatafactoryId", "1"}
                            };

                            string MergeSQL = GenerateSQLStatementTemplates.GetSQL(System.IO.Path.Combine(Shared._ApplicationBasePath,Shared._ApplicationOptions.LocalPaths.SQLTemplateLocation), "MergeIntoActivityLevelLogs", SqlParams);
                            logging.LogInformation(MergeSQL.ToString());
                            _conWrite.ExecuteWithRetry(MergeSQL);
                            _conWrite.Close();
                            _conWrite.Dispose();
                        }

                    }

                    else
                    {
                        logging.LogErrors(new Exception("Kusto query failed getting ADFPipeline Stats."));
                    }
                }
            }

            return new { };

        }


        private static Dictionary<string, Type> KustoDataTypeMapper
        {
            get
            {
                // Add the rest of your CLR Types to SQL Types mapping here
                Dictionary<string, Type> dataMapper = new Dictionary<string, Type>
                    {
                        { "int", typeof(int) },
                        { "string", typeof(string) },
                        { "real", typeof(double) },
                        { "long", typeof(long) },
                        { "datetime", typeof(System.DateTime) },
                        { "guid", typeof(Guid) }

                    };

                return dataMapper;
            }
        }
    }

 
}







