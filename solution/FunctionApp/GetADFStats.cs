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
using System.IO;

namespace AdsGoFast
{

    public  class GetADFActivityRunsTimerTrigger
    {
        private readonly IOptions<ApplicationOptions> _appOptions;
        public GetADFActivityRunsTimerTrigger(IOptions<ApplicationOptions> appOptions)
        {
            _appOptions = appOptions;
        }

        [FunctionName("GetADFActivityRunsTimerTrigger")]
        public  async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;

            if (_appOptions.Value.TimerTriggers.EnableGetADFStats)
            {
                using (FrameworkRunner FR = new FrameworkRunner(log, ExecutionId))
                {
                    FrameworkRunner.FrameworkRunnerWorker worker = GetADFStats.GetADFActivityRuns;
                    FrameworkRunner.FrameworkRunnerResult result = FR.Invoke("GetADFActivityRunsTimerTrigger", worker);
                }
            }
        }
    }

    public  class GetADFPipelineRunsTimerTrigger
    {
        private readonly IOptions<ApplicationOptions> _appOptions;
        public GetADFPipelineRunsTimerTrigger(IOptions<ApplicationOptions> appOptions)
        {
            _appOptions = appOptions;
        }

        [FunctionName("GetADFPipelineRunsTimerTrigger")]
        public  async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;

            if (_appOptions.Value.TimerTriggers.EnableGetADFStats)
            {
                using (FrameworkRunner FR = new FrameworkRunner(log, ExecutionId))
                {
                    FrameworkRunner.FrameworkRunnerWorker worker = GetADFStats.GetADFPipelineRuns;
                    FrameworkRunner.FrameworkRunnerResult result = FR.Invoke("GetADFPipelineRunsTimerTrigger", worker);
                }
            }
        }
    }

    public  class GetADFActivityErrorsTimerTrigger
    {
        private readonly IOptions<ApplicationOptions> _appOptions;
        public GetADFActivityErrorsTimerTrigger(IOptions<ApplicationOptions> appOptions)
        {
            _appOptions = appOptions;
        }

        [FunctionName("GetADFActivityErrors")]
        public  async Task Run([TimerTrigger("0 */5 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;

            if (_appOptions.Value.TimerTriggers.EnableGetADFStats)
            {
                using (FrameworkRunner FR = new FrameworkRunner(log, ExecutionId))
                {
                    FrameworkRunner.FrameworkRunnerWorker worker = GetADFStats.GetADFActivityErrors;
                    FrameworkRunner.FrameworkRunnerResult result = FR.Invoke("GetADFActivityErrorsTimerTrigger", worker);
                }
            }
        }
    }

    public static class GetADFStats
    {
        public static dynamic GetADFPipelineRuns(Logging logging)
        {

            //string workspaceId = System.Environment.GetEnvironmentVariable("LogAnalyticsWorkspaceId");
            using var client = new HttpClient();
            string token = Shared.Azure.AzureSDK.GetAzureRestApiToken("https://api.loganalytics.io");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            using SqlConnection _conRead = TMD.GetSqlConnection();

            //Get Last Request Date
            var MaxTimesGen = _conRead.QueryWithRetry(@"
                                    Select a.*,  MaxPipelineTimeGenerated from 
                                        DataFactory a left join 
                                        ( Select b.DataFactoryId, MaxPipelineTimeGenerated = Max(MaxPipelineTimeGenerated) 
                                        from ADFPipelineRun b
                                        group by b.DatafactoryId) b on a.Id = b.DatafactoryId");

            DateTimeOffset MaxPipelineTimeGenerated = DateTimeOffset.UtcNow.AddDays(-30);


            foreach (var datafactory in MaxTimesGen)
            {
                if (datafactory.MaxPipelineTimeGenerated != null)
                {
                    MaxPipelineTimeGenerated = ((DateTimeOffset)datafactory.MaxPipelineTimeGenerated).AddMinutes(-180);
                }

                string workspaceId = datafactory.LogAnalyticsWorkspaceId.ToString();

                Dictionary<string, object> KqlParams = new Dictionary<string, object>
                {
                    {"MaxPipelineTimeGenerated", MaxPipelineTimeGenerated.ToString("yyyy-MM-dd HH:mm:ss.ff K") },
                    {"SubscriptionId", ((string)datafactory.SubscriptionUid.ToString()).ToUpper()},
                    {"ResourceGroupName", ((string)datafactory.ResourceGroup.ToString()).ToUpper() },
                    {"DataFactoryName", ((string)datafactory.Name.ToString()).ToUpper() },
                    {"DatafactoryId", datafactory.Id.ToString()  }
                };

                string KQL = System.IO.File.ReadAllText(Path.Combine(Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.KQLTemplateLocation), "GetADFPipelineRuns.kql"));
                KQL = KQL.FormatWith(KqlParams, FormatWith.MissingKeyBehaviour.ThrowException, null, '{', '}');

                JObject JsonContent = new JObject();
                JsonContent["query"] = KQL;

                var postContent = new StringContent(JsonContent.ToString(), System.Text.Encoding.UTF8, "application/json");

                var response = client.PostAsync($"https://api.loganalytics.io/v1/workspaces/{workspaceId}/query", postContent).Result;

                logging.LogInformation(response.ToString());

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
                                dr[i] = ((Newtonsoft.Json.Linq.JValue)r[i]).Value;
                            }
                            dt.Rows.Add(dr);
                        }

                        Table t = new Table();
                        t.Schema = "dbo";
                        string TableGuid = Guid.NewGuid().ToString();
                        t.Name = $"#ADFPipelineRun{TableGuid}";
                        using (SqlConnection _conWrite = TMD.GetSqlConnection())
                        {
                            TMD.BulkInsert(dt, t, true, _conWrite);
                            Dictionary<string, string> SqlParams = new Dictionary<string, string>
                            {
                                { "TempTable", t.QuotedSchemaAndName() },
                                { "DatafactoryId", datafactory.Id.ToString()}
                            };

                            string MergeSQL = GenerateSQLStatementTemplates.GetSQL(Shared.GlobalConfigs.GetStringConfig("SQLTemplateLocation"), "MergeIntoADFPipelineRun", SqlParams);

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

        public static dynamic GetADFActivityErrors(Logging logging)
        {
            
            using var client = new HttpClient();
            string token = Shared.Azure.AzureSDK.GetAzureRestApiToken("https://api.loganalytics.io");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            using SqlConnection _conRead = TMD.GetSqlConnection();

            //Get Last Request Date
            //ToDo Add DataFactoryId field to ADFActivityErrors
            var MaxTimesGen = _conRead.QueryWithRetry(@"                                  
                                                       Select a.*, MaxTimeGenerated MaxTimeGenerated from 
                                                        Datafactory a left join 
                                                        ( Select DataFactoryId, MaxTimeGenerated = Max(TimeGenerated) 
                                                        from ADFActivityErrors b
                                                        group by DataFactoryId
                                                        ) b on a.Id = b.DatafactoryId
                                                        ");

            DateTimeOffset MaxTimeGenerated = DateTimeOffset.UtcNow.AddDays(-30);


            foreach (var datafactory in MaxTimesGen)
            {
                if (datafactory.MaxTimeGenerated != null)
                {
                    MaxTimeGenerated = ((DateTimeOffset)datafactory.MaxTimeGenerated).AddMinutes(-5);
                }

                string workspaceId = datafactory.LogAnalyticsWorkspaceId.ToString();

                Dictionary<string, object> KqlParams = new Dictionary<string, object>
                {
                    {"MaxActivityTimeGenerated", MaxTimeGenerated.ToString("yyyy-MM-dd HH:mm:ss.ff K") },
                    {"SubscriptionId", ((string)datafactory.SubscriptionUid.ToString()).ToUpper()},
                    {"ResourceGroupName", ((string)datafactory.ResourceGroup.ToString()).ToUpper() },
                    {"DataFactoryName", ((string)datafactory.Name.ToString()).ToUpper() },
                    {"DatafactoryId", datafactory.Id.ToString()  }
                };

                string KQL = System.IO.File.ReadAllText(Path.Combine(Path.Combine(Shared._ApplicationBasePath,Shared._ApplicationOptions.LocalPaths.KQLTemplateLocation),"GetADFActivityErrors.kql"));
                KQL = KQL.FormatWith(KqlParams, FormatWith.MissingKeyBehaviour.ThrowException, null, '{', '}');

                JObject JsonContent = new JObject();
                JsonContent["query"] = KQL;

                var postContent = new StringContent(JsonContent.ToString(), System.Text.Encoding.UTF8, "application/json");

                var response = client.PostAsync($"https://api.loganalytics.io/v1/workspaces/{workspaceId}/query", postContent).Result;
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
                        t.Name = $"#ADFActivityErrors{TableGuid}";
                        using (SqlConnection _conWrite = TMD.GetSqlConnection())
                        {
                            TMD.BulkInsert(dt, t, true, _conWrite);
                            Dictionary<string, string> SqlParams = new Dictionary<string, string>
                            {
                                { "TempTable", t.QuotedSchemaAndName() },
                                { "DatafactoryId", datafactory.Id.ToString()}
                            };

                            string MergeSQL = GenerateSQLStatementTemplates.GetSQL(Shared.GlobalConfigs.GetStringConfig("SQLTemplateLocation"), "MergeIntoADFActivityErrors", SqlParams);
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


        public static dynamic GetADFActivityRuns(Logging logging)
        {

            
            using var client = new HttpClient();
            string token = Shared.Azure.AzureSDK.GetAzureRestApiToken("https://api.loganalytics.io");
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            using SqlConnection _conRead = TMD.GetSqlConnection();

            //Get Last Request Date
            var MaxTimesGen = _conRead.QueryWithRetry(@"
                                    Select a.*,  MaxActivityTimeGenerated from 
                                        DataFactory a left join 
                                        ( Select b.DataFactoryId, MaxActivityTimeGenerated = Max(MaxActivityTimeGenerated) 
                                        from ADFActivityRun b
                                        group by b.DatafactoryId) b on a.Id = b.DatafactoryId

                             ");

            DateTimeOffset MaxActivityTimeGenerated = DateTimeOffset.UtcNow.AddDays(-30);


            foreach (var datafactory in MaxTimesGen)
            {
                if (datafactory.MaxActivityTimeGenerated != null)
                {
                    MaxActivityTimeGenerated = ((DateTimeOffset)datafactory.MaxActivityTimeGenerated).AddMinutes(-5);
                }

                string workspaceId = datafactory.LogAnalyticsWorkspaceId.ToString();

                

                Dictionary<string, object> KqlParams = new Dictionary<string, object>
                {
                    {"MaxActivityTimeGenerated", MaxActivityTimeGenerated.ToString("yyyy-MM-dd HH:mm:ss.ff K") },
                    {"SubscriptionId", ((string)datafactory.SubscriptionUid.ToString()).ToUpper()},
                    {"ResourceGroupName", ((string)datafactory.ResourceGroup.ToString()).ToUpper() },
                    {"DataFactoryName", ((string)datafactory.Name.ToString()).ToUpper() },
                    {"DatafactoryId", datafactory.Id.ToString()  }
                };

                //Add in the rates from ADFServiceRates.json
                string ADFRatesStr = System.IO.File.ReadAllText(Path.Combine(Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.KQLTemplateLocation), "ADFServiceRates.json"));
                JObject ADFRates = JObject.Parse(ADFRatesStr);
                foreach (JProperty p in ADFRates.Properties())
                {
                    KqlParams.Add(p.Name, p.Value.ToString());
                }

                string KQL = System.IO.File.ReadAllText(Path.Combine(Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.KQLTemplateLocation),  "GetADFActivityRuns.kql"));
                KQL = KQL.FormatWith(KqlParams, FormatWith.MissingKeyBehaviour.ThrowException, null, '{', '}');


                JObject JsonContent = new JObject();
                JsonContent["query"] = KQL;

                var postContent = new StringContent(JsonContent.ToString(), System.Text.Encoding.UTF8, "application/json");

                var response = client.PostAsync($"https://api.loganalytics.io/v1/workspaces/{workspaceId}/query", postContent).Result;
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
                                dr[i] = ((Newtonsoft.Json.Linq.JValue)r[i]).Value;
                            }
                            dt.Rows.Add(dr);
                        }


                        Table t = new Table();
                        t.Schema = "dbo";
                        string TableGuid = Guid.NewGuid().ToString();
                        t.Name = $"#ADFActivityRun{TableGuid}";

                        using (SqlConnection _conWrite = TMD.GetSqlConnection())
                        {
                            TMD.BulkInsert(dt, t, true, _conWrite);
                            Dictionary<string, string> SqlParams = new Dictionary<string, string>
                            {
                                { "TempTable", t.QuotedSchemaAndName() },
                                { "DatafactoryId", datafactory.Id.ToString()}
                            };

                            string MergeSQL = GenerateSQLStatementTemplates.GetSQL(Shared.GlobalConfigs.GetStringConfig("SQLTemplateLocation"), "MergeIntoADFActivityRun", SqlParams);
                            _conWrite.ExecuteWithRetry(MergeSQL, 120);
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







