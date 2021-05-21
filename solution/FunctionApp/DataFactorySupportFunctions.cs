/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using AdsGoFast.SqlServer;
using AdsGoFast.TaskMetaData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Dapper;

namespace AdsGoFast
{
    /// <summary>
    /// Persist Metadata JSON in Azure Blob or ADLS
    /// </summary>
    public static class TaskExecutionSchemaFile
    {
        [FunctionName("TaskExecutionSchemaFile")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId; ;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = TaskExecutionSchemaFileCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "TaskExecutionSchemaFile", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }

        }

        public static JObject TaskExecutionSchemaFileCore(HttpRequest req, Logging logging)
        {
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string _storageAccountName = data["StorageAccountName"].ToString();
            string _storageAccountContainer = data["StorageAccountContainer"].ToString();
            string _relativePath = data["RelativePath"].ToString();

            string _metadataType = data["MetadataType"].ToString();
            string _sourceType = data["SourceType"].ToString();
            string _targetType = data["TargetType"].ToString();

            string _schemaFileName = data["SchemaFileName"].ToString();
            string _schemaStructure;

            if (_metadataType == "Parquet")
            {
                _schemaStructure = data["Data"]["structure"].ToString();
            }
            else
            {
                _schemaStructure = data["Data"]["value"].ToString();
            }

            _storageAccountName = _storageAccountName.Replace(".dfs.core.windows.net", "").Replace("https://", "").Replace(".blob.core.windows.net", "");
            TokenCredential StorageToken = new TokenCredential(Shared._AzureAuthenticationCredentialProvider.GetAzureRestApiToken(string.Format("https://{0}.blob.core.windows.net", _storageAccountName), Shared._ApplicationOptions.UseMSI));

            if (!(_sourceType == "Azure Blob" && _sourceType == "ADLS") && (_metadataType == "Parquet"))
            {
                _schemaFileName = _schemaFileName.Replace(".json", ".parquetschema.json");
            }

            Shared.Azure.Storage.UploadContentToBlob(_schemaStructure, _storageAccountName, _storageAccountContainer, _relativePath, _schemaFileName, StorageToken);

            JArray arr = (JArray)JsonConvert.DeserializeObject(_schemaStructure);

            JObject Root = GetSourceTargetMapping.Mapping(arr, _sourceType, _targetType, _metadataType);

            return Root;
        }

    }

    /// <summary>
    /// Get Source and Target Mapping (SQL to Parquet and Parquet to SQL)
    /// </summary>
    public static class GetSourceTargetMapping
    {
        [FunctionName("GetSourceTargetMapping")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = GetSourceTargetMappingCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "GetSourceTargetMapping", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }


        }

        public static JObject Mapping(JArray arr, string sourceType, string targetType, string metadataType)
        {
            JObject Header = new JObject
            {
                ["type"] = "TabularTranslator",
                ["typeConversion"] = true
            };

            JObject typeConversionSettings = new JObject
            {
                ["allowDataTruncation"] = true,
                ["treatBooleanAsNumber"] = false
            };

            Header["typeConversionSettings"] = typeConversionSettings;

            List<JObject> obj = new List<JObject>();

            foreach (JObject r in arr)
            {
                JObject mappings = new JObject();
                JObject source = new JObject();
                JObject sink = new JObject();

                if (metadataType == "SQL" && (sourceType == "Azure SQL" || sourceType == "SQL Server") && (targetType == "Azure Blob" || targetType == "ADLS"))
                {
                    source["name"] = r["COLUMN_NAME"].ToString();
                    source["type"] = TaskInstancesStatic.TransformSQLTypesToDotNetFramework(r["DATA_TYPE"].ToString());
                    source["physicalType"] = r["DATA_TYPE"].ToString();

                    sink["name"] = TaskInstancesStatic.TransformParquetFileColName(r["COLUMN_NAME"].ToString());
                    sink["type"] = TaskInstancesStatic.TransformSQLTypesToParquet(r["DATA_TYPE"].ToString());
                    sink["physicalType"] = r["DATA_TYPE"].ToString();
                }
                else if (metadataType == "Parquet" && (sourceType == "Azure Blob" || sourceType == "ADLS") && (targetType == "SQL Server" || targetType == "Azure SQL" || targetType == "Table"))
                {
                    source["name"] = TaskInstancesStatic.TransformParquetFileColName(r["COLUMN_NAME"].ToString());
                    source["type"] = TaskInstancesStatic.TransformSQLTypesToParquet(r["DATA_TYPE"].ToString());
                    source["physicalType"] = r["DATA_TYPE"].ToString();

                    sink["name"] = r["COLUMN_NAME"].ToString();
                    sink["type"] = TaskInstancesStatic.TransformSQLTypesToDotNetFramework(r["DATA_TYPE"].ToString());
                    sink["physicalType"] = r["DATA_TYPE"].ToString();
                }

                mappings["source"] = source;
                mappings["sink"] = sink;
                obj.Add(mappings);
            }
            Header["mappings"] = JToken.FromObject(obj);

            JObject Root = new JObject
            {
                ["value"] = Header
            };

            return Root;
        }

        public static JObject GetSourceTargetMappingCore(HttpRequest req,
            Logging logging)
        {
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string _storageAccountName = data["StorageAccountName"].ToString();
            string _storageAccountContainer = data["StorageAccountContainer"].ToString();
            string _relativePath = data["RelativePath"].ToString();
            string _metadataType = data["MetadataType"].ToString();
            string _sourceType = data["SourceType"].ToString();
            string _targetType = data["TargetType"].ToString();
            string _schemaFileName = data["SchemaFileName"].ToString();

            _storageAccountName = _storageAccountName.Replace(".dfs.core.windows.net", "").Replace("https://", "").Replace(".blob.core.windows.net", "");
            TokenCredential StorageToken = new TokenCredential(Shared._AzureAuthenticationCredentialProvider.GetAzureRestApiToken(string.Format("https://{0}.blob.core.windows.net", _storageAccountName), Shared._ApplicationOptions.UseMSI));

            string _schemaStructure = Shared.Azure.Storage.ReadFile(_storageAccountName, _storageAccountContainer, _relativePath, _schemaFileName, StorageToken);

            JArray arr = (JArray)JsonConvert.DeserializeObject(_schemaStructure);

            JObject Root = Mapping(arr, _sourceType, _targetType, _metadataType);

            logging.LogInformation("GetSourceTargetMapping Function complete.");
            return Root;
        }
    }

    public static class GetSQLCreateStatementFromSchema
    {
        [FunctionName("GetSQLCreateStatementFromSchema")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;

            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = GetSQLCreateStatementFromSchemaCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "GetSQLCreateStatementFromSchema", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }

        }

        public static JObject GetSQLCreateStatementFromSchemaCore(HttpRequest req,
            Logging logging)
        {
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            JObject data = JsonConvert.DeserializeObject<JObject>(requestBody);
            string _CreateStatement;
            JArray arr;

            if (data["Data"] != null)
            {
                //Need to swap logic for parquet vs sql etc
                arr = (JArray)data["Data"]["value"];
            }
            else if (data["SchemaFileName"] != null)
            {
                string _storageAccountName = data["StorageAccountName"].ToString();
                string _storageAccountContainer = data["StorageAccountContainer"].ToString();
                string _relativePath = data["RelativePath"].ToString();
                string _schemaFileName = data["SchemaFileName"].ToString();

                _storageAccountName = _storageAccountName.Replace(".dfs.core.windows.net", "").Replace("https://", "").Replace(".blob.core.windows.net", "");

                TokenCredential StorageToken = new TokenCredential(Shared._AzureAuthenticationCredentialProvider.GetAzureRestApiToken("https://" + _storageAccountName + ".blob.core.windows.net", Shared._ApplicationOptions.UseMSI));

                arr = (JArray)JsonConvert.DeserializeObject(Shared.Azure.Storage.ReadFile(_storageAccountName, _storageAccountContainer, _relativePath, _schemaFileName, StorageToken));
            }
            else
            {
                throw new System.ArgumentException("Not Valid parameters to GetSQLCreateStatementFromSchema Function!");
            }

            bool _DropIfExist = data["DropIfExist"] == null ? false : (bool)data["DropIfExist"];

            _CreateStatement = GenerateSQLStatementTemplates.GetCreateTable(arr, data["TableSchema"].ToString(), data["TableName"].ToString(), _DropIfExist);
            _CreateStatement += Environment.NewLine + "Select 1";

            JObject Root = new JObject
            {
                ["CreateStatement"] = _CreateStatement
            };

            return Root;
        }
    }

    /// <summary>
    /// Checks configuration database to see if there are any new storage accounts that need to be suscribed to. If there are then the this code will create the required subscriptions
    /// </summary>
    public static class CreateAzureStorageEventSubscriptions
    {
        [FunctionName("CreateAzureStorageEventSubscriptions")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = CreateAzureStorageEventSubscriptionsCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "CreateAzureStorageEventSubscriptions", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }

        }

        public static string CreateAzureStorageEventSubscriptionsCore(HttpRequest req,
            Logging logging)
        {
            //ToDo: WIP
            return "";
        }
    }




    /// <summary>
    /// Generate Merge Statement
    /// </summary>
    public static class GetSQLMergeStatement
    {
        [FunctionName("GetSQLMergeStatement")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = GetSQLMergeStatementCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "GetSQLMergeStatement", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }
        }
        public static JObject GetSQLMergeStatementCore(HttpRequest req,
            Logging logging)
        {
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(requestBody);


            JObject Root = new JObject();
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            using (SqlConnection _con = TMD.GetSqlConnection())
            {
                string _token = Shared._AzureAuthenticationCredentialProvider.GetAzureRestApiToken("https://database.windows.net/");
                String g = Guid.NewGuid().ToString().Replace("-", "");

                _con.AccessToken = _token;
                JArray arrStage = (JArray)data["Stage"];
                string _StagingTableSchema = data["StagingTableSchema"].ToString();
                string _StagingTableName = "#Temp_" + data["StagingTableName"].ToString() + g.ToString();
                string _CreateStatementStage = GenerateSQLStatementTemplates.GetCreateTable(arrStage, _StagingTableSchema, _StagingTableName, false);
                TMD.ExecuteSql(_CreateStatementStage, _con);

                JArray arrTarget = (JArray)data["Target"];
                string _TargetTableSchema = data["TargetTableSchema"].ToString();
                string _TargetTableName = "#Temp_" + data["TargetTableName"].ToString() + g.ToString();
                string _CreateStatementTarget = GenerateSQLStatementTemplates.GetCreateTable(arrTarget, _TargetTableSchema, _TargetTableName, false);

                TMD.ExecuteSql(_CreateStatementTarget, _con);

                string _MergeStatement = TMD.GenerateMergeSQL(_StagingTableSchema, _StagingTableName, _TargetTableSchema, _TargetTableName, _con, true, logging);
                string fullStagingTableName = string.Format("[{0}].[{1}]", _StagingTableSchema, _StagingTableName.Replace("#Temp_", "").Replace(g.ToString(), ""));
                string fullTargetTableName = string.Format("[{0}].[{1}]", _TargetTableSchema, _TargetTableName.Replace("#Temp_", "").Replace(g.ToString(), ""));
                _MergeStatement = _MergeStatement.Replace(_TargetTableName, fullTargetTableName);
                _MergeStatement = _MergeStatement.Replace(_StagingTableName, fullStagingTableName);
                //Add Select for ADF Lookup Activity 
                _MergeStatement += Environment.NewLine + "Select 1 ";

                Root["MergeStatement"] = _MergeStatement;

                logging.LogInformation("GetSQLMergeStatement Function complete.");
            }
            return Root;


        }
    }

    public static class GetInformationSchemaSQL
    {
        [FunctionName("GetInformationSchemaSQL")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = GetInformationSchemaSQLCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "GetInformationSchemaSQL", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }
        }

        public static JObject GetInformationSchemaSQLCore(HttpRequest req,
            Logging logging)
        {
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string _TableSchema = JObject.Parse(data.ToString())["TableSchema"];
            string _TableName = JObject.Parse(data.ToString())["TableName"];

            string _InformationSchemaSQL = string.Format(@"
                           SELECT DISTINCT
	                            c.ORDINAL_POSITION,
	                            c.COLUMN_NAME, 
	                            c.DATA_TYPE,
	                            IS_NULLABLE = cast(case when c.IS_NULLABLE = 'Yes' then 1 else 0 END as bit), 
	                            c.NUMERIC_PRECISION, 
	                            c.CHARACTER_MAXIMUM_LENGTH, 
	                            c.NUMERIC_SCALE,  
	                            ac.is_identity IS_IDENTITY,
	                            ac.is_computed IS_COMPUTED,
	                            KEY_COLUMN = cast(CASE WHEN kcu.TABLE_NAME IS NULL THEN 0 ELSE 1 END as bit),
	                            PKEY_COLUMN = cast(CASE WHEN tc.TABLE_NAME IS NULL THEN 0 ELSE 1 END as bit)
                            FROM INFORMATION_SCHEMA.COLUMNS c with (NOLOCK)
                            INNER JOIN sys.all_columns ac with (NOLOCK) ON object_id(QUOTENAME(c.TABLE_SCHEMA)+'.'+QUOTENAME(c.TABLE_NAME)) = ac.object_id and ac.name = c.COLUMN_NAME
                            LEFT OUTER join INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu with (NOLOCK) ON c.TABLE_CATALOG = kcu.TABLE_CATALOG and c.TABLE_SCHEMA = kcu.TABLE_SCHEMA AND c.TABLE_NAME = kcu.TABLE_NAME and c.COLUMN_NAME = kcu.COLUMN_NAME 
                            LEFT OUTER join 
	                            (SELECT Col.TABLE_CATALOG, Col.TABLE_SCHEMA, Col.TABLE_NAME, Col.COLUMN_NAME
	                            from 
		                            INFORMATION_SCHEMA.TABLE_CONSTRAINTS Tab with (NOLOCK), 
		                            INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE Col with (NOLOCK) 
	                            WHERE 
		                            Col.Constraint_Name = Tab.Constraint_Name
		                            AND Col.Table_Name = Tab.Table_Name
		                            AND Tab.Constraint_Type = 'PRIMARY KEY') tc
	                            ON c.TABLE_CATALOG = tc.TABLE_CATALOG and c.TABLE_SCHEMA = tc.TABLE_SCHEMA and c.TABLE_NAME = tc.TABLE_NAME and c.COLUMN_NAME = tc.COLUMN_NAME
                            WHERE c.TABLE_NAME = '{0}' AND c.TABLE_SCHEMA = '{1}'                                    
                        ", _TableName, _TableSchema);

            JObject Root = new JObject
            {
                ["InformationSchemaSQL"] = _InformationSchemaSQL
            };

            return Root;
        }

    }

    /// <summary>
    /// TBA
    /// </summary>
    public static class ADFLogging
    {
        [FunctionName("Log")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = LogCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "Log", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }

        }
        public static JObject LogCore(HttpRequest req,
            Logging LogHelper)
        {
            short _FrameworkNumberOfRetries = Shared.GlobalConfigs.GetInt16Config("FrameworkNumberOfRetries");

            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            dynamic _TaskInstanceId = JObject.Parse(data.ToString())["TaskInstanceId"];
            dynamic _NumberOfRetries = JObject.Parse(data.ToString())["NumberOfRetries"];
            dynamic _PostObjectExecutionUid = JObject.Parse(data.ToString())["ExecutionUid"];
            dynamic _AdfRunUid = JObject.Parse(data.ToString())["RunId"];
            dynamic _LogTypeId = JObject.Parse(data.ToString())["LogTypeId"];//1 Error, 2 Warning, 3 Info, 4 Performance, 5 Debug
            dynamic _LogSource = JObject.Parse(data.ToString())["LogSource"];//ADF, AF
            dynamic _ActivityType = JObject.Parse(data.ToString())["ActivityType"];
            dynamic _StartDateTimeOffSet = JObject.Parse(data.ToString())["StartDateTimeOffSet"];
            dynamic _Status = JObject.Parse(data.ToString())["Status"]; //Started Failed Completed
            dynamic _Comment = JObject.Parse(data.ToString())["Comment"];
            _Comment = _Comment == null ? null : _Comment.ToString().Replace("'", "");
            dynamic _EndDateTimeOffSet = JObject.Parse(data.ToString())["EndDateTimeOffSet"];
            dynamic _RowsInserted = JObject.Parse(data.ToString())["RowsInserted"];

            if (_TaskInstanceId != null) { LogHelper.DefaultActivityLogItem.TaskInstanceId = (long?)_TaskInstanceId; }
            if (_LogSource != null) { LogHelper.DefaultActivityLogItem.LogSource = (string)_LogSource; }
            if (_LogTypeId != null) { LogHelper.DefaultActivityLogItem.LogTypeId = (short?)_LogTypeId; }
            if (_StartDateTimeOffSet != null) { LogHelper.DefaultActivityLogItem.StartDateTimeOffset = (DateTimeOffset)_StartDateTimeOffSet; }
            if (_Status!= null) { LogHelper.DefaultActivityLogItem.Status = (string)_Status; }
            if (_EndDateTimeOffSet != null) { LogHelper.DefaultActivityLogItem.EndDateTimeOffset = (DateTimeOffset)_EndDateTimeOffSet; }
            if (_PostObjectExecutionUid != null) { LogHelper.DefaultActivityLogItem.ExecutionUid = (Guid?)_PostObjectExecutionUid; }

            LogHelper.LogInformation(_Comment);

            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            BaseTasks.TaskStatus TaskStatus = new BaseTasks.TaskStatus();
                        
                
            if (_ActivityType == "Data-Movement-Master")
            {
                if (_Status == "Failed")
                {
                    //Todo Put Max Number of retries in DB at TaskMasterLevel -- This has now been done. Have left logic in function as stored procedure handles with all failed statuses. 
                    _NumberOfRetries = (_NumberOfRetries == null) ? 0 : (int)_NumberOfRetries + 1;
                    TaskStatus = ((_NumberOfRetries < _FrameworkNumberOfRetries) ? BaseTasks.TaskStatus.FailedRetry : BaseTasks.TaskStatus.FailedNoRetry);

                }
                else
                {
                    if (Enum.TryParse<BaseTasks.TaskStatus>(_Status.ToString(), out TaskStatus) == false)
                    {
                        string InvalidStatus = "TaskStatus Enum does not exist for: " + _Status.ToString();
                        LogHelper.LogErrors(new Exception("TaskStatus Enum does not exist for: " + _Status.ToString()));
                        _Comment = _Comment.ToString() + "." + InvalidStatus;
                        TaskStatus = BaseTasks.TaskStatus.FailedNoRetry;
                    }
                }
                TMD.LogTaskInstanceCompletion((Int64)_TaskInstanceId, (System.Guid)_PostObjectExecutionUid, TaskStatus, (System.Guid)_AdfRunUid, (String)_Comment);
            }


            JObject Root = new JObject
            {
                ["Result"] = "Complete"
            };

            return Root;
        }
    }

    /// <summary>
    /// TBA
    /// </summary>
    public static class WaterMark
    {
        [FunctionName("WaterMark")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = UpdateWaterMark;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "UpdateWaterMark", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }

        }
        public static JObject UpdateWaterMark(HttpRequest req,
            Logging LogHelper)
        {
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(requestBody);

            dynamic _TaskMasterId = JObject.Parse(data.ToString())["TaskMasterId"];
            dynamic _TaskMasterWaterMarkColumnType = JObject.Parse(data.ToString())["TaskMasterWaterMarkColumnType"];
            dynamic _WaterMarkValue = JObject.Parse(data.ToString())["WaterMarkValue"];

            if (!string.IsNullOrEmpty(_WaterMarkValue.ToString()))
            {
                DataTable dt = new DataTable();
                dt.Columns.Add(new DataColumn("TaskMasterId", typeof(long)));
                dt.Columns.Add(new DataColumn("TaskMasterWaterMarkColumnType", typeof(string)));
                dt.Columns.Add(new DataColumn("TaskMasterWaterMark_DateTime", typeof(DateTime)));
                dt.Columns.Add(new DataColumn("TaskMasterWaterMark_BigInt", typeof(long)));
                dt.Columns.Add(new DataColumn("ActiveYN", typeof(bool)));
                dt.Columns.Add(new DataColumn("UpdatedOn", typeof(DateTime)));

                DataRow dr = dt.NewRow();
                dr["TaskMasterId"] = _TaskMasterId;
                dr["TaskMasterWaterMarkColumnType"] = _TaskMasterWaterMarkColumnType;
                if (_TaskMasterWaterMarkColumnType == "DateTime")
                {
                    dr["TaskMasterWaterMark_DateTime"] = _WaterMarkValue;
                    dr["TaskMasterWaterMark_BigInt"] = DBNull.Value;
                }
                else if (_TaskMasterWaterMarkColumnType == "BigInt")
                {
                    dr["TaskMasterWaterMark_DateTime"] = DBNull.Value;
                    dr["TaskMasterWaterMark_BigInt"] = _WaterMarkValue;
                }
                else
                {
                    throw new System.ArgumentException(string.Format("Invalid WaterMark ColumnType = '{0}'", _TaskMasterWaterMarkColumnType));
                }

                dr["ActiveYN"] = 1;
                dr["UpdatedOn"] = DateTime.UtcNow;
                dt.Rows.Add(dr);

                string TempTableName = "#Temp_TaskMasterWaterMark_" + Guid.NewGuid().ToString();
                TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
                TMD.AutoBulkInsertAndMerge(dt, TempTableName, "TaskMasterWaterMark");
            }

            JObject Root = new JObject
            {
                ["Result"] = "Complete"
            };

            return Root;
        }
    }

}

namespace AdsGoFast.ADSSupport.SqlServer
{
    public static class MsSqlGetAllTablesFromDatabaseSQL
    {
        [FunctionName("MsSqlGetAllTablesFromDatabaseSQL")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = MsSqlGetAllTablesFromDatabaseSQLCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "MsSqlGetAllTablesFromDatabaseSQL", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }

        }

        public static JObject MsSqlGetAllTablesFromDatabaseSQLCore(HttpRequest req,
            Logging logging)
        {
            string _InformationSchemaSQL = string.Format(@"
                            Select * from INFORMATION_SCHEMA.TABLES
                            where [Table_schema] not in ('sys') and TABLE_TYPE = 'BASE TABLE'       
                        ");

            JObject Root = new JObject
            {
                ["InformationSchemaSQL"] = _InformationSchemaSQL
            };

            logging.LogInformation("MsSqlGetAllTablesFromDatabaseSQL Function complete.");
            return Root;
        }
    }

}


namespace AdsGoFast.ADSSupport.Oracle
{


}


namespace AdsGoFast.ADSSupport.ManipulateTaskMasters
{
    public static class GenerateTaskMasters
    {
        [FunctionName("GenerateTaskMasters")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
           ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);

            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = GenerateTaskMastersCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "GenerateTaskMasters", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }

        }

        private static dynamic GenerateTaskMastersCore(HttpRequest req,
           Logging logging)
        {
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            JObject data = JsonConvert.DeserializeObject<JObject>(requestBody);
            JArray tables = JArray.Parse(data["TableList"].ToString());
            JObject jsontemplate = JObject.Parse(data["JsonTemplate"].ToString());

            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("TaskMasterName", Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("TaskTypeId", Type.GetType("System.Int64")));
            dt.Columns.Add(new DataColumn("TaskGroupId", Type.GetType("System.Int64")));
            dt.Columns.Add(new DataColumn("ScheduleMasterId", Type.GetType("System.Int64")));
            dt.Columns.Add(new DataColumn("SourceSystemId", Type.GetType("System.Int64")));
            dt.Columns.Add(new DataColumn("TargetSystemId", Type.GetType("System.Int64")));
            dt.Columns.Add(new DataColumn("DegreeOfCopyParallelism", Type.GetType("System.Int16")));
            dt.Columns.Add(new DataColumn("AllowMultipleActiveInstances", Type.GetType("System.Boolean")));
            dt.Columns.Add(new DataColumn("TaskDatafactoryIR", Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("ActiveYN", Type.GetType("System.Boolean")));
            dt.Columns.Add(new DataColumn("DependencyChainTag", Type.GetType("System.String")));
            dt.Columns.Add(new DataColumn("DataFactoryId", Type.GetType("System.Int64")));
            dt.Columns.Add(new DataColumn("TaskMasterJSON", Type.GetType("System.String")));

            foreach (JToken t in tables)
            {
                DataRow dr = dt.NewRow();
                dr["TaskMasterName"] = jsontemplate["TaskMasterName"].ToString().Replace("{@TableSchema@}", t["TABLE_SCHEMA"].ToString()).Replace("{@TableName@}", t["TABLE_NAME"].ToString());
                dr["TaskTypeId"] = jsontemplate["TaskTypeId"];
                dr["TaskGroupId"] = jsontemplate["TaskGroupId"];
                dr["ScheduleMasterId"] = jsontemplate["ScheduleMasterId"];
                dr["SourceSystemId"] = jsontemplate["SourceSystemId"];
                dr["TargetSystemId"] = jsontemplate["TargetSystemId"];
                dr["DegreeOfCopyParallelism"] = jsontemplate["DegreeOfCopyParallelism"];
                dr["AllowMultipleActiveInstances"] = jsontemplate["AllowMultipleActiveInstances"];
                dr["TaskDatafactoryIR"] = jsontemplate["TaskDatafactoryIR"];
                dr["ActiveYN"] = jsontemplate["ActiveYN"];
                dr["DependencyChainTag"] = jsontemplate["DependencyChainTag"].ToString().Replace("{@TableSchema@}", t["TABLE_SCHEMA"].ToString()).Replace("{@TableName@}", t["TABLE_NAME"].ToString());
                dr["DataFactoryId"] = jsontemplate["DataFactoryId"];
                dr["TaskDatafactoryIR"] = jsontemplate["TaskDatafactoryIR"];

                JObject _TaskMasterJSON = new JObject
                {
                    ["Source"] = JObject.Parse(Shared.JsonHelpers.GetStringValueFromJSON(logging, "Source", jsontemplate, null, true).Replace("{@TableSchema@}", t["TABLE_SCHEMA"].ToString()).Replace("{@TableName@}", t["TABLE_NAME"].ToString())),
                    ["Target"] = JObject.Parse(Shared.JsonHelpers.GetStringValueFromJSON(logging, "Target", jsontemplate, null, true).Replace("{@TableSchema@}", t["TABLE_SCHEMA"].ToString()).Replace("{@TableName@}", t["TABLE_NAME"].ToString()))
                };

                dr["TaskMasterJSON"] = JObject.Parse(_TaskMasterJSON.ToString());

                dt.Rows.Add(dr);
            }

            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();

            string TempTableName = "#TempTaskMaster" + Guid.NewGuid().ToString();
            Table TempTable = new Table
            {
                Name = TempTableName
            };
            SqlConnection _con = TMD.GetSqlConnection();
            TMD.BulkInsert(dt, TempTable, true, _con);

            Dictionary<string, string> SqlParams = new Dictionary<string, string>
            {
                { "TempTableName", TempTableName }
            };

            string _sql = GenerateSQLStatementTemplates.GetSQL(System.IO.Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.SQLTemplateLocation), "GenerateTaskMasters", SqlParams);
            TMD.ExecuteSql(_sql, _con);

            return new { }; //Return Empty Object
        }
    }

    public static class DisableTaskMaster
    {
        [FunctionName("DisableTaskMaster")]
        public static async Task<IActionResult> Run(
           [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
           ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            using FrameworkRunner FRP = new FrameworkRunner(log, ExecutionId);
            FrameworkRunner.FrameworkRunnerWorkerWithHttpRequest worker = DisableTaskMasterCore;
            FrameworkRunner.FrameworkRunnerResult result = FRP.Invoke(req, "DisableTaskMaster", worker);
            if (result.Succeeded)
            {
                return new OkObjectResult(JObject.Parse(result.ReturnObject));
            }
            else
            {
                return new BadRequestObjectResult(new { Error = "Execution Failed...." });
            }
        }

        public static JObject DisableTaskMasterCore(HttpRequest req,
            Logging logging)
        {
            string requestBody = new StreamReader(req.Body).ReadToEndAsync().Result;
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            dynamic _TaskMasterId = JObject.Parse(data.ToString())["TaskMasterId"];
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            TMD.ExecuteSql(string.Format(@"Update [dbo].[TaskMaster] SET ActiveYN = '0' Where [TaskMasterId] = {0}", _TaskMasterId));

            JObject Root = new JObject
            {
                ["Result"] = "Complete"
            };

            return Root;
        }
    }
}
