/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Data;
using Microsoft.Azure.Management.DataFactory;
using Microsoft.Azure.Management.DataFactory.Models;
using Newtonsoft.Json.Linq;
using static AdsGoFast.Shared.Azure.DataFactory;
using System.Runtime.CompilerServices;
using Microsoft.Rest.Azure;
using System.Net.Http;

namespace AdsGoFast
{
    public static class ExecutePipeline
    {
        public static JObject ExecutePipelineMethod(string subscriptionId, string resourceGroup, string factoryName, string pipelineName, Dictionary<string, object> parameters,  Logging logging)
        {


            #region CreatePipelineRun
            //Create a data factory management client
            logging.LogInformation("Creating ADF connectivity client.");
            string outputString = string.Empty;

            using (var client = DataFactoryClient.CreateDataFactoryClient(subscriptionId))
            {
                //Run pipeline
                CreateRunResponse runResponse;
                PipelineRun pipelineRun;

                if (parameters.Count == 0)
                {
                    logging.LogInformation("Called pipeline without parameters.");

                    runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(
                        resourceGroup, factoryName, pipelineName).Result.Body;
                }
                else
                {
                    logging.LogInformation("Called pipeline with parameters.");


                    logging.LogInformation("Number of parameters provided: " + parameters.Count);

                    System.Threading.Thread.Sleep(1000);

                    runResponse = client.Pipelines.CreateRunWithHttpMessagesAsync(
                       resourceGroup, factoryName, pipelineName, parameters: parameters).Result.Body;
                    runResponse = new CreateRunResponse();
                    runResponse.RunId = Guid.NewGuid().ToString();

                }

                logging.LogInformation("Pipeline run ID: " + runResponse.RunId);

                //Wait and check for pipeline to start...
                logging.LogInformation("Checking ADF pipeline status.");
                //while (true)
                //{
                 //   pipelineRun = client.PipelineRuns.Get(
                 //       resourceGroup, factoryName, runResponse.RunId);

                 //   logging.LogInformation("ADF pipeline status: " + pipelineRun.Status);

                    //if (pipelineRun.Status == "Queued")
                    //    System.Threading.Thread.Sleep(15000);
                    //else
                    //    break;
                //}

                //Final return detail
                outputString = "{ \"PipelineName\": \"" + pipelineName +
                                        "\", \"RunId\": \"" + runResponse.RunId +
                                        "\", \"Status\": \"" + "InProgress" + //pipelineRun.Status +
                                        "\" }";


            }

            #endregion

            JObject outputJson = JObject.Parse(outputString);
            return outputJson;
        }
    }

    public static class PipelineRunStats
    {
        public static void QueryPipelineRuns(string subscriptionId, string resourceGroup, string factoryName, string _rungroupid, DateTime startDT, DateTime endDT, Logging logging)
        {
            #region QueryPipelineRuns
           
            logging.LogInformation("Query ADF Pipeline Runs.");
            string outputString = string.Empty;

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("ExecutionUid", typeof(Guid)));
            dt.Columns.Add(new DataColumn("TaskInstanceId", typeof(Int64)));
            dt.Columns.Add(new DataColumn("TaskMasterId", typeof(Int64)));
            //dt.Columns.Add(new DataColumn("AdditionalProperties", typeof(String)));
            dt.Columns.Add(new DataColumn("DurationInMs", typeof(Int64)));
            //dt.Columns.Add(new DataColumn("InvokedBy", typeof(String)));
            dt.Columns.Add(new DataColumn("IsLastest", typeof(Boolean)));
            dt.Columns.Add(new DataColumn("LastUpdated", typeof(DateTime)));
            //dt.Columns.Add(new DataColumn("Message", typeof(String)));
            //dt.Columns.Add(new DataColumn("Parameters", typeof(String)));
            dt.Columns.Add(new DataColumn("RunId", typeof(Guid)));
            //dt.Columns.Add(new DataColumn("RunGroupId", typeof(Guid)));
            dt.Columns.Add(new DataColumn("PipelineName", typeof(String)));
            dt.Columns.Add(new DataColumn("RunStart", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("RunEnd", typeof(DateTime)));
            dt.Columns.Add(new DataColumn("RunDimensions", typeof(String)));
            dt.Columns.Add(new DataColumn("Status", typeof(String)));


            DataTable ActivityDt = new DataTable();
            ActivityDt.Columns.Add(new DataColumn("ActivityName", typeof(String)));
            ActivityDt.Columns.Add(new DataColumn("RunId", typeof(Guid)));
            ActivityDt.Columns.Add(new DataColumn("ActivityRunStart", typeof(DateTime)));
            ActivityDt.Columns.Add(new DataColumn("ActivityRunEnd", typeof(DateTime)));
            ActivityDt.Columns.Add(new DataColumn("ActivityRunId", typeof(Guid)));
            ActivityDt.Columns.Add(new DataColumn("ActivityType", typeof(String)));
            //dt.Columns.Add(new DataColumn("AdditionalProperties", typeof(String)));
            ActivityDt.Columns.Add(new DataColumn("DurationInMs", typeof(Int64)));
            //dt.Columns.Add(new DataColumn("Error", typeof(String)));
            //dt.Columns.Add(new DataColumn("Input", typeof(String)));
            //dt.Columns.Add(new DataColumn("LinkedServiceName", typeof(String)));
            ActivityDt.Columns.Add(new DataColumn("OutPut", typeof(String)));
            ActivityDt.Columns.Add(new DataColumn("PipelineName", typeof(String)));
            ActivityDt.Columns.Add(new DataColumn("PipelineRunId", typeof(String)));
            ActivityDt.Columns.Add(new DataColumn("Status", typeof(String)));

            using (var client = DataFactoryClient.CreateDataFactoryClient(subscriptionId))
            {
                //Get pipeline status with provided run id
                PipelineRunsQueryResponse pipelineRunsQueryResponse;

               

                RunFilterParameters filterParameterActivityRuns = new RunFilterParameters();
                filterParameterActivityRuns.LastUpdatedAfter = startDT;
                filterParameterActivityRuns.LastUpdatedBefore = endDT.AddHours(+2);

                RunFilterParameters filterParameter = new RunFilterParameters();
                filterParameter.LastUpdatedAfter = startDT;
                filterParameter.LastUpdatedBefore = endDT;

                IList<string> rungroupid = new List<string> { _rungroupid };
                IList<RunQueryFilter> filter = new List<RunQueryFilter>();
                filter.Add(new RunQueryFilter
                {
                    Operand = RunQueryFilterOperand.RunGroupId,
                    OperatorProperty = RunQueryFilterOperator.Equals,
                    Values = rungroupid
                });

                filterParameter.Filters = filter;

                logging.LogInformation(String.Format("API PipelineRuns.QueryByFactory Start"));
                pipelineRunsQueryResponse = client.PipelineRuns.QueryByFactory(resourceGroup, factoryName, filterParameter);
                logging.LogInformation(String.Format("API PipelineRuns.QueryByFactory End"));
                var enumerator = pipelineRunsQueryResponse.Value.GetEnumerator();
                PipelineRun pipelineRuns;
                string runId = String.Empty;
                int item = 0;

                while (true)
                {
                    for (bool hasMoreRuns = enumerator.MoveNext(); hasMoreRuns;)
                    {
                        pipelineRuns = enumerator.Current;
                        hasMoreRuns = enumerator.MoveNext();
                        runId = pipelineRuns.RunId;
                        item += 1;

                        logging.LogInformation(String.Format("PipelineRuns.QueryByFactory RunId {0} Current Item {1} of {2}",runId,item,pipelineRunsQueryResponse.Value.Count));

                        DataRow dr = dt.NewRow();
                        string _param = string.Empty;
                        foreach (var element in pipelineRuns.Parameters)
                        {
                            _param = element.Value;
                            break;
                        }
                        dr["ExecutionUid"] = Shared.JsonHelpers.GetStringValueFromJSON(logging, "ExecutionUid", JObject.Parse(_param), null, true);
                        dr["TaskInstanceId"] = Shared.JsonHelpers.GetStringValueFromJSON(logging, "TaskInstanceId", JObject.Parse(_param), null, true);
                        dr["TaskMasterId"] = Shared.JsonHelpers.GetStringValueFromJSON(logging, "TaskMasterId", JObject.Parse(_param), null, true);
                        //dr["AdditionalProperties"] = pipelineRuns.AdditionalProperties ?? (object)DBNull.Value;
                        dr["DurationInMs"] = pipelineRuns.DurationInMs ?? (object)DBNull.Value;
                        //dr["InvokedBy"] = pipelineRuns.InvokedBy ?? (object)DBNull.Value;
                        dr["IsLastest"] = pipelineRuns.IsLatest ?? (object)DBNull.Value;
                        dr["LastUpdated"] = pipelineRuns.LastUpdated ?? (object)DBNull.Value;
                        //dr["Message"] = pipelineRuns.Message ?? (object)DBNull.Value;
                        //dr["Parameters"] = _param;
                        dr["RunId"] = pipelineRuns.RunId;
                        //dr["RunGroupId"] = pipelineRuns.RunGroupId ?? (object)DBNull.Value;
                        dr["PipelineName"] = pipelineRuns.PipelineName ?? (object)DBNull.Value;
                        dr["RunStart"] = pipelineRuns.RunStart ?? (object)DBNull.Value;
                        dr["RunEnd"] = pipelineRuns.RunEnd ?? (object)DBNull.Value;
                        dr["RunDimensions"] = pipelineRuns.PipelineName ?? (object)DBNull.Value;
                        dr["Status"] = pipelineRuns.Status ?? (object)DBNull.Value;
                        dt.Rows.Add(dr);

                        QueryActivityRuns(subscriptionId, resourceGroup, factoryName, runId, runId, filterParameterActivityRuns, logging, ref ActivityDt);
                    }
                   

                    if (pipelineRunsQueryResponse.ContinuationToken == null)
                        break;

                    filterParameter.ContinuationToken = pipelineRunsQueryResponse.ContinuationToken;
                    pipelineRunsQueryResponse = client.PipelineRuns.QueryByFactory(resourceGroup, factoryName, filterParameter);
                    enumerator = pipelineRunsQueryResponse.Value.GetEnumerator();
                    item = 0;
                }

            }

            if (ActivityDt.Rows.Count > 0)
            {                
                string TempTableName = "#Temp_ADFActivities_" + Guid.NewGuid().ToString();
                TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
                TMD.AutoBulkInsertAndMerge(ActivityDt, TempTableName, "ADFActivity");                
            }


            if (dt.Rows.Count > 0)
            {
                string TempTableName = "#Temp_ADFPipelineRun_" + Guid.NewGuid().ToString();
                TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
                TMD.AutoBulkInsertAndMerge(dt, TempTableName, "ADFPipelineRun");
            }

            #endregion

        }

        public static void QueryActivityRuns(string subscriptionId, string resourceGroup, string factoryName, string runId, string parentRunId, RunFilterParameters filterParameterActivityRuns, Logging logging, ref DataTable dt)
        {
            #region QueryActivityRuns

            logging.LogInformation(String.Format("QueryActivityRuns - RunId {0}",runId));
            string outputString = string.Empty;



            using (var client = DataFactoryClient.CreateDataFactoryClient(subscriptionId))
            {
                //Get pipeline status with provided run id

                

                logging.LogInformation(String.Format("QueryActivityRuns - RunId {0} - API QueryByPipelineRun Start", runId));
                ActivityRunsQueryResponse activityRunsQueryResponse = client.ActivityRuns.QueryByPipelineRun(resourceGroup, factoryName, runId, filterParameterActivityRuns);
                logging.LogInformation(String.Format("QueryActivityRuns - RunId {0} - API QueryByPipelineRun End", runId));
                var enumeratorActivity = activityRunsQueryResponse.Value.GetEnumerator();
                ActivityRun activityRun;

                for (bool hasMoreActivityRuns = enumeratorActivity.MoveNext(); hasMoreActivityRuns;)
                {
                    activityRun = enumeratorActivity.Current;
                    hasMoreActivityRuns = enumeratorActivity.MoveNext();

                    DataRow dr = dt.NewRow();
                    dr["ActivityName"] = activityRun.ActivityName ?? (object)DBNull.Value;
                    dr["RunId"] = parentRunId;
                    dr["ActivityRunStart"] = activityRun.ActivityRunStart ?? (object)DBNull.Value;
                    dr["ActivityRunEnd"] = activityRun.ActivityRunEnd ?? (object)DBNull.Value;
                    dr["ActivityRunId"] = activityRun.ActivityRunId ?? (object)DBNull.Value;
                    dr["ActivityType"] = activityRun.ActivityType ?? (object)DBNull.Value;
                    //dr["AdditionalProperties"] = activityRun.AdditionalProperties ?? (object)DBNull.Value;
                    dr["DurationInMs"] = activityRun.DurationInMs ?? (object)DBNull.Value;
                    //dr["Error"] = activityRun.Error ?? (object)DBNull.Value;
                    //dr["Input"] = activityRun.Input ?? (object)DBNull.Value;
                    //dr["LinkedServiceName"] = activityRun.LinkedServiceName ?? (object)DBNull.Value;
                    dr["OutPut"] = activityRun.Output ?? (object)DBNull.Value;
                    dr["PipelineName"] = activityRun.PipelineName ?? (object)DBNull.Value;
                    dr["PipelineRunId"] = activityRun.PipelineRunId ?? (object)DBNull.Value;
                    dr["Status"] = activityRun.Status ?? (object)DBNull.Value;
                    dt.Rows.Add(dr);

                    if(activityRun.ActivityType == "ExecutePipeline")
                    {
                        string _runId = Shared.JsonHelpers.GetStringValueFromJSON(logging, "pipelineRunId", (JObject)activityRun.Output, null, true);
                        if (!String.IsNullOrEmpty(_runId))
                            QueryActivityRuns(subscriptionId, resourceGroup, factoryName, _runId, parentRunId, filterParameterActivityRuns, logging, ref dt);
                        else
                            logging.LogInformation(String.Format("QueryActivityRuns - RunId Is null  for {0} ", (JObject)activityRun.Output));
                    }
                }
                
            }

            #endregion
        }

    }

    public static class CheckPipelineStatus
    {        

        public static JObject CheckPipelineStatusMethod(string subscriptionId, string resourceGroup, string factoryName, string pipelineName, string runId, Logging logging)
        {


            #region CreatePipelineRun
            //Create a data factory management client
            logging.LogInformation("Creating ADF connectivity client.");
            string outputString = string.Empty;

            using (var client = DataFactoryClient.CreateDataFactoryClient(subscriptionId))
            {
                //Get pipeline status with provided run id
                PipelineRun pipelineRun;
                pipelineRun = client.PipelineRuns.Get(resourceGroup, factoryName, runId);
                logging.LogInformation("Checking ADF pipeline status.");

                //Create simple status for Data Factory Until comparison checks
                string simpleStatus;

                if (pipelineRun.Status == "InProgress")
                {
                    simpleStatus = "Running";
                }
                else
                {
                    simpleStatus = "Done";
                }

                logging.LogInformation("ADF pipeline status: " + pipelineRun.Status);

                //Final return detail
                outputString = "{ \"PipelineName\": \"" + pipelineName +
                                        "\", \"RunId\": \"" + pipelineRun.RunId +
                                        "\", \"SimpleStatus\": \"" + simpleStatus +
                                        "\", \"Status\": \"" + pipelineRun.Status +
                                        "\" }";

            }

            #endregion

            JObject outputJson = JObject.Parse(outputString);
            return outputJson;
        }

        /// <summary>
        /// ToDo: Function Not yet finished. 
        /// </summary>
        /// <param name="subscriptionId"></param>
        /// <param name="resourceGroup"></param>
        /// <param name="factoryName"></param>
        public static void GetPipelinesByFactory(string subscriptionId, string resourceGroup, string factoryName)
        {
                   
            var _Token = Shared._AzureAuthenticationCredentialProvider.GetAzureRestApiToken("https://management.azure.com");
            using (var client = new System.Net.Http.HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _Token);
                string queryString = string.Format("https://management.azure.com/subscriptions/{0}/resourceGroups/{1}/providers/Microsoft.DataFactory/factories/{2}/queryPipelineRuns?api-version=2018-06-01", subscriptionId, resourceGroup, factoryName);

                JObject PostBody = new JObject();

                PostBody["lastUpdatedAfter"] = "2018-06-16T00: 36:44.3345758Z";
                PostBody["lastUpdatedBefore"] = "2022-06-16T00:49:48.3686473Z";

                JArray Filters = new JArray();
                JObject Filter = new JObject();
                Filter["operand"] = "Status";
                Filter["operator"] = "Equals";
                JArray FilterValues = new JArray();
                FilterValues.Add("InProgress");
                Filter["values"] = FilterValues;
                Filters.Add(Filter);
                PostBody["filters"] = Filters;

                var httpContent = new StringContent(PostBody.ToString(), System.Text.Encoding.UTF8, "application/json");
        

                var result = client.PostAsync(queryString, httpContent).Result;

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var content = result.Content.ReadAsStringAsync().Result;
                    var definition = new { value = "" };
                    var jobject = JsonConvert.DeserializeAnonymousType(content, definition);                    
                }
                else
                {
                    var error = "";
                    try
                    {
                        var content = result.Content.ReadAsStringAsync().Result;
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
}
