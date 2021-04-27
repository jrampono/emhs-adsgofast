/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using AdsGoFast.Models.Options;
using AdsGoFast.SqlServer;
using AdsGoFast.TaskMetaData;
using Cronos;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace AdsGoFast
{

    public class PrepareFrameworkTasksTimerTrigger
    {
        private readonly IOptions<ApplicationOptions> _appOptions;
        public PrepareFrameworkTasksTimerTrigger(IOptions<ApplicationOptions> appOptions)
        {
            _appOptions = appOptions;
        }

        [FunctionName("PrepareFrameworkTasksTimerTrigger")]
        public async Task Run([TimerTrigger("0 */2 * * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            Guid ExecutionId = context.InvocationId;
            if (_appOptions.Value.TimerTriggers.EnablePrepareFrameworkTasks)
            {
                using (FrameworkRunner FR = new FrameworkRunner(log, ExecutionId))
                {
                    FrameworkRunner.FrameworkRunnerWorker worker = PrepareFrameworkTasks.PrepareFrameworkTasksCore;
                    FrameworkRunner.FrameworkRunnerResult result = FR.Invoke("PrepareFrameworkTasksHttpTrigger", worker);
                }
            }
        }
    }

    public static class PrepareFrameworkTasks
    {
        public static dynamic PrepareFrameworkTasksCore(Logging logging)
        {
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            TMD.ExecuteSql(string.Format("Insert into Execution values ('{0}', '{1}', '{2}')", logging.DefaultActivityLogItem.ExecutionUid, DateTimeOffset.Now.ToString("u"), DateTimeOffset.Now.AddYears(999).ToString("u")));

            //Check status of running pipelines and calculate available "slots" based on max concurrency settings
            short _FrameworkWideMaxConcurrency = Shared._ApplicationOptions.FrameworkWideMaxConcurrency;

            //ToDo: Write Pipelines that need to be checked to Queue for now I have just reduced to only those tasks that have been running for longer than x minutes.
            //CheckLongRunningPipelines(logging);

            //Get Count of All runnning pipelines directly from the database
            short _RunnningPipelines = CountRunnningPipelines(logging);

            short _AvailableConcurrencySlots = (short)(_FrameworkWideMaxConcurrency - _RunnningPipelines);

            //Generate new task instances based on task master and schedules
            CreateTaskInstance(logging);

            //Is there is Available Slots Proceed
            if (_AvailableConcurrencySlots > 0)
            {
                List<AdsGoFast.TaskMetaData.TaskGroup> _TaskGroups = TaskGroupsStatic.GetActive();

                if (_TaskGroups.Count > 0)
                {
                    short _ConcurrencySlotsAllocated = 0;
                    short _DefaultTasksPerGroup = 0;
                    short _DistributionLoopCounter = 1;

                    //Distribute Concurrency Slots 
                    while (_AvailableConcurrencySlots > 0)
                    {
                        DistributeConcurrencySlots(ref _TaskGroups, ref _DefaultTasksPerGroup, ref _ConcurrencySlotsAllocated, ref _AvailableConcurrencySlots, _DistributionLoopCounter);
                        _DistributionLoopCounter += 1;
                    }

                    Table TempTarget = new Table
                    {
                        Schema = "dbo",
                        Name = "#TempGroups" + logging.DefaultActivityLogItem.ExecutionUid.ToString()
                    };
                    SqlConnection _con = TMD.GetSqlConnection();
                    TMD.BulkInsert(_TaskGroups.ToDataTable(), TempTarget, true, _con);
                    Dictionary<string, string> _params = new Dictionary<string, string>
                    {
                        { "TempTable", TempTarget.QuotedSchemaAndName() }
                    };
                    string _sql = GenerateSQLStatementTemplates.GetSQL(System.IO.Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.SQLTemplateLocation), "UpdateTaskInstancesWithTaskRunner", _params);
                    TMD.ExecuteSql(_sql, _con);
                }
            }

            return new { };
        }

        public static void DistributeConcurrencySlots(ref List<AdsGoFast.TaskMetaData.TaskGroup> _TaskGroups, ref short _DefaultTasksPerGroup, ref short _ConcurrencySlotsAllocated, ref short _AvailableConcurrencySlots, short DistributionLoopCount)
        {
            short TaskGroupLoopCount = 1;
            short MaxTaskGroupIndex = 32767;

            //Ensure that we have at least one concurrency slot per group
            if (_TaskGroups.Count > _AvailableConcurrencySlots)
            {
                MaxTaskGroupIndex = _AvailableConcurrencySlots;
                _DefaultTasksPerGroup = 1;
                //List<AdsGoFast.TaskMetaData.TaskGroup> _NewTaskGroups = _TaskGroups.Take<AdsGoFast.TaskMetaData.TaskGroup>(_AvailableConcurrencySlots).ToList();
                //_TaskGroups = _NewTaskGroups;
            }
            else
            {
                _DefaultTasksPerGroup = (short)Math.Floor((decimal)(_AvailableConcurrencySlots / _TaskGroups.Count));
            }

            foreach (TaskGroup _TaskGroup in _TaskGroups)
            {
                if (DistributionLoopCount == 1)
                { _TaskGroup.TasksUnAllocated = _TaskGroup.TaskCount; }

                if (_TaskGroup.TasksUnAllocated < _DefaultTasksPerGroup)
                {
                    _ConcurrencySlotsAllocated += _TaskGroup.TaskCount;
                    _AvailableConcurrencySlots -= _TaskGroup.TaskCount;
                    _TaskGroup.ConcurrencySlotsAllocated += _TaskGroup.TaskCount;
                    _TaskGroup.TasksUnAllocated -= _TaskGroup.TaskCount;
                }
                else
                {
                    _ConcurrencySlotsAllocated += _DefaultTasksPerGroup;
                    _AvailableConcurrencySlots -= _DefaultTasksPerGroup;
                    _TaskGroup.ConcurrencySlotsAllocated += _DefaultTasksPerGroup;
                    _TaskGroup.TasksUnAllocated -= _DefaultTasksPerGroup;
                }

                TaskGroupLoopCount += 1;
                //Break the foreach if we have hit the max
                if (TaskGroupLoopCount >= MaxTaskGroupIndex) { break; }
            }


        }
        public static void CreateTaskInstance(Logging logging)
        {
            logging.LogInformation("Create ScheduleInstance called.");
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            DateTimeOffset _date = DateTimeOffset.Now;

            DataTable dtScheduleInstance = new DataTable();
            dtScheduleInstance.Columns.Add(new DataColumn("ScheduleMasterId", typeof(long)));
            dtScheduleInstance.Columns.Add(new DataColumn("ScheduledDateUtc", typeof(DateTime)));
            dtScheduleInstance.Columns.Add(new DataColumn("ScheduledDateTimeOffset", typeof(DateTimeOffset)));
            dtScheduleInstance.Columns.Add(new DataColumn("ActiveYN", typeof(bool)));

            dynamic resScheduleInstance = TMD.GetSqlConnection().QueryWithRetry(@"
                Select 
	                SM.ScheduleMasterId, 
	                SM.ScheduleCronExpression, 
	                Coalesce(SI.MaxScheduledDateTimeOffset,cast('1900-01-01' as datetimeoffset)) as MaxScheduledDateTimeOffset
                from
                    ScheduleMaster SM 
	                join ( 
	                Select distinct ScheduleMasterId from TaskMaster TM where TM.ActiveYN = 1) TM on TM.ScheduleMasterId = SM.ScheduleMasterId
	                left outer join
                    (
		                Select ScheduleMasterId, Max(ScheduledDateTimeOffset) MaxScheduledDateTimeOffset
		                From ScheduleInstance
		                Where ActiveYN = 1
		                Group By ScheduleMasterId
                    ) SI on SM.ScheduleMasterId = SI.ScheduleMasterId
                Where SM.ActiveYN = 1");

            foreach (dynamic _row in resScheduleInstance)
            {
                DateTimeOffset? nextUtc;
                if (_row.ScheduleCronExpression.ToString() == "N/A")
                {
                    nextUtc = DateTime.UtcNow.AddMinutes(-1);
                }
                else
                {
                    CronExpression _cronExpression = CronExpression.Parse(_row.ScheduleCronExpression.ToString(), CronFormat.IncludeSeconds);

                    nextUtc = _cronExpression.GetNextOccurrence(_row.MaxScheduledDateTimeOffset, TimeZoneInfo.Utc);
                }

                if (nextUtc?.DateTime <= DateTime.UtcNow)
                {
                    DataRow dr = dtScheduleInstance.NewRow();

                    dr["ScheduleMasterId"] = _row.ScheduleMasterId;
                    dr["ScheduledDateUtc"] = _date.Date;
                    dr["ScheduledDateTimeOffset"] = _date;
                    dr["ActiveYN"] = true;

                    dtScheduleInstance.Rows.Add(dr);
                }
            }

            //Persist TEMP ScheduleInstance
            SqlConnection _con = TMD.GetSqlConnection();
            Table tmpScheduleInstanceTargetTable = new Table
            {
                Name = "#Temp" + Guid.NewGuid().ToString()
            };
            TMD.BulkInsert(dtScheduleInstance, tmpScheduleInstanceTargetTable, true, _con);

            //Create TaskInstance 
            logging.LogInformation("Create TaskInstance called.");

            DataTable dtTaskInstance = new DataTable();
            dtTaskInstance.Columns.Add(new DataColumn("ExecutionUid", typeof(Guid)));
            dtTaskInstance.Columns.Add(new DataColumn("TaskMasterId", typeof(long)));
            dtTaskInstance.Columns.Add(new DataColumn("ScheduleInstanceId", typeof(long)));
            dtTaskInstance.Columns.Add(new DataColumn("ADFPipeline", typeof(string)));
            dtTaskInstance.Columns.Add(new DataColumn("TaskInstanceJson", typeof(string)));
            dtTaskInstance.Columns.Add(new DataColumn("LastExecutionStatus", typeof(string)));
            dtTaskInstance.Columns.Add(new DataColumn("ActiveYN", typeof(bool)));

            dynamic resTaskInstance = TMD.GetSqlConnection().QueryWithRetry(@"Exec dbo.GetTaskMaster");
            DataTable dtTaskTypeMapping = GetTaskTypeMapping(logging);

            foreach (dynamic _row in resTaskInstance)
            {
                DataRow drTaskInstance = dtTaskInstance.NewRow();
                logging.DefaultActivityLogItem.TaskInstanceId = _row.TaskInstanceId;
                logging.DefaultActivityLogItem.TaskMasterId = _row.TaskMasterId;
                try
                {
                    dynamic sourceSystemJson = JsonConvert.DeserializeObject(_row.SourceSystemJSON);
                    dynamic taskMasterJson = JsonConvert.DeserializeObject(_row.TaskMasterJSON);
                    dynamic targetSystemJson = JsonConvert.DeserializeObject(_row.TargetSystemJSON);

                    string _ADFPipeline = GetTaskTypeMappingName(logging, _row.TaskExecutionType.ToString(), dtTaskTypeMapping, _row.TaskTypeId, _row.SourceSystemType.ToString(), taskMasterJson?.Source.Type.ToString(), _row.TargetSystemType.ToString(), taskMasterJson?.Target.Type.ToString(), _row.TaskDatafactoryIR);

                    drTaskInstance["TaskMasterId"] = _row.TaskMasterId ?? DBNull.Value;
                    drTaskInstance["ScheduleInstanceId"] = 0;//_row.ScheduleInstanceId == null ? DBNull.Value : _row.ScheduleInstanceId;
                    drTaskInstance["ExecutionUid"] = logging.DefaultActivityLogItem.ExecutionUid;
                    drTaskInstance["ADFPipeline"] = _ADFPipeline;
                    drTaskInstance["LastExecutionStatus"] = "Untried";
                    drTaskInstance["ActiveYN"] = true;

                    JObject Root = new JObject();

                    if (_row.SourceSystemType == "ADLS" || _row.SourceSystemType == "Azure Blob")
                    {
                        if (taskMasterJson?.Source.Type.ToString() != "Filelist")
                        {
                            Root["SourceRelativePath"] = TaskInstancesStatic.TransformRelativePath(JObject.Parse(_row.TaskMasterJSON)["Source"]["RelativePath"].ToString(), _date.DateTime);
                        }
                    }

                    if (_row.TargetSystemType == "ADLS" || _row.TargetSystemType == "Azure Blob")
                    {
                        if (JObject.Parse(_row.TaskMasterJSON)["Target"]["RelativePath"] != null)
                        {
                            Root["TargetRelativePath"] = TaskInstancesStatic.TransformRelativePath(JObject.Parse(_row.TaskMasterJSON)["Target"]["RelativePath"].ToString(), _date.DateTime);
                        }
                    }

                    if (JObject.Parse(_row.TaskMasterJSON)["Source"]["IncrementalType"] == "Watermark")
                    {
                        Root["IncrementalField"] = _row.TaskMasterWaterMarkColumn;
                        Root["IncrementalColumnType"] = _row.TaskMasterWaterMarkColumnType;
                        if (_row.TaskMasterWaterMarkColumnType == "DateTime")
                        {
                            Root["IncrementalValue"] = _row.TaskMasterWaterMark_DateTime ?? "1900-01-01";
                        }
                        else if (_row.TaskMasterWaterMarkColumnType == "BigInt")
                        {
                            Root["IncrementalValue"] = _row.TaskMasterWaterMark_BigInt ?? -1;
                        }
                    }

                    if (Root == null)
                    {
                        drTaskInstance["TaskInstanceJson"] = DBNull.Value;
                    }
                    else
                    {
                        drTaskInstance["TaskInstanceJson"] = Root;
                    }

                    dtTaskInstance.Rows.Add(drTaskInstance);
                }
                catch (Exception e)
                {
                    logging.LogErrors(new Exception(string.Format("Failed to create new task instances for TaskMasterId '{0}'.", logging.DefaultActivityLogItem.TaskInstanceId)));
                    logging.LogErrors(e);
                }
            }

            //Persist TMP TaskInstance
            Table tmpTaskInstanceTargetTable = new Table
            {
                Name = "#Temp" + Guid.NewGuid().ToString()
            };
            TMD.BulkInsert(dtTaskInstance, tmpTaskInstanceTargetTable, true, _con);

            Dictionary<string, string> SqlParams = new Dictionary<string, string>
            {
                { "tmpScheduleInstance", tmpScheduleInstanceTargetTable.QuotedSchemaAndName() },
                { "tmpTaskInstance", tmpTaskInstanceTargetTable.QuotedSchemaAndName() }
            };

            string InsertSQL = GenerateSQLStatementTemplates.GetSQL(System.IO.Path.Combine(Shared._ApplicationBasePath, Shared._ApplicationOptions.LocalPaths.SQLTemplateLocation), "InsertScheduleInstance_TaskInstance", SqlParams);

            _con.ExecuteWithRetry(InsertSQL);
            _con.Close();
        }

        public static short CountRunnningPipelines(Logging logging)
        {
            short _ActivePipelines = ActivePipelines.CountActivePipelines(logging);
            return _ActivePipelines;
        }


        /// <summary>
        /// Checks for long running pipelines and updates their status in the database
        /// </summary>
        /// <param name="logging"></param>
        /// <returns></returns>
        public static short CheckLongRunningPipelines(Logging logging)
        {
            dynamic _ActivePipelines = ActivePipelines.GetLongRunningPipelines(logging);

            short RunningPipelines = 0;
            short FinishedPipelines = 0;
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn("TaskInstanceId", typeof(string)));
            dt.Columns.Add(new DataColumn("ExecutionUid", typeof(Guid)));
            dt.Columns.Add(new DataColumn("PipelineName", typeof(string)));
            dt.Columns.Add(new DataColumn("DatafactorySubscriptionUid", typeof(Guid)));
            dt.Columns.Add(new DataColumn("DatafactoryResourceGroup", typeof(string)));
            dt.Columns.Add(new DataColumn("DatafactoryName", typeof(string)));
            dt.Columns.Add(new DataColumn("RunUid", typeof(Guid)));
            dt.Columns.Add(new DataColumn("Status", typeof(string)));
            dt.Columns.Add(new DataColumn("SimpleStatus", typeof(string)));

            //Check Each Running Pipeline
            foreach (dynamic _Pipeline in _ActivePipelines)
            {
                dynamic _PipelineStatus = CheckPipelineStatus.CheckPipelineStatusMethod(_Pipeline.DatafactorySubscriptionUid.ToString(), _Pipeline.DatafactoryResourceGroup.ToString(), _Pipeline.DatafactoryName.ToString(), _Pipeline.PipelineName.ToString(), _Pipeline.AdfRunUid.ToString(), logging);

                if (_PipelineStatus["SimpleStatus"].ToString() == "Runnning")
                {
                    RunningPipelines += 1;
                }

                if (_PipelineStatus["SimpleStatus"].ToString() == "Done")
                {
                    FinishedPipelines += 1;
                }

                DataRow dr = dt.NewRow();

                dr["TaskInstanceId"] = _Pipeline.TaskInstanceId;
                dr["ExecutionUid"] = _Pipeline.ExecutionUid;
                dr["DatafactorySubscriptionUid"] = _Pipeline.DatafactorySubscriptionUid;
                dr["DatafactoryResourceGroup"] = _Pipeline.DatafactoryResourceGroup;
                dr["DatafactoryName"] = _Pipeline.DatafactoryName;

                dr["Status"] = _PipelineStatus["Status"];
                dr["SimpleStatus"] = _PipelineStatus["SimpleStatus"];
                dr["RunUid"] = (Guid)_PipelineStatus["RunId"];
                dr["PipelineName"] = _PipelineStatus["PipelineName"];
                dt.Rows.Add(dr);

            }

            string TempTableName = "#Temp" + Guid.NewGuid().ToString();
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            //Todo: Update both the TaskInstanceExecution and the TaskInstance;
            TMD.AutoBulkInsertAndMerge(dt, TempTableName, "TaskInstanceExecution");

            return RunningPipelines;


        }

        public static DataTable GetTaskTypeMapping(Logging logging)
        {
            logging.LogDebug("Load TaskTypeMapping called.");
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            dynamic resTaskTypeMapping = TMD.GetSqlConnection().QueryWithRetry(@"select * from [dbo].[TaskTypeMapping] Where ActiveYN = 1");

            DataTable dtTaskTypeMapping = new DataTable();
            dtTaskTypeMapping.Columns.Add(new DataColumn("TaskTypeId", typeof(int)));
            dtTaskTypeMapping.Columns.Add(new DataColumn("MappingType", typeof(string)));
            dtTaskTypeMapping.Columns.Add(new DataColumn("MappingName", typeof(string)));
            dtTaskTypeMapping.Columns.Add(new DataColumn("SourceSystemType", typeof(string)));
            dtTaskTypeMapping.Columns.Add(new DataColumn("SourceType", typeof(string)));
            dtTaskTypeMapping.Columns.Add(new DataColumn("TargetSystemType", typeof(string)));
            dtTaskTypeMapping.Columns.Add(new DataColumn("TargetType", typeof(string)));
            dtTaskTypeMapping.Columns.Add(new DataColumn("TaskDatafactoryIR", typeof(string)));

            foreach (dynamic _row in resTaskTypeMapping)
            {
                DataRow dr = dtTaskTypeMapping.NewRow();

                dr["TaskTypeId"] = _row.TaskTypeId;
                dr["MappingType"] = _row.MappingType;
                dr["MappingName"] = _row.MappingName;
                dr["SourceSystemType"] = _row.SourceSystemType;
                dr["SourceType"] = _row.SourceType;
                dr["TargetSystemType"] = _row.TargetSystemType;
                dr["TargetType"] = _row.TargetType;
                dr["TaskDatafactoryIR"] = _row.TaskDatafactoryIR;

                dtTaskTypeMapping.Rows.Add(dr);
            }

            logging.LogDebug("Load TaskTypeMapping complete.");

            return dtTaskTypeMapping;
        }
        public static string GetTaskTypeMappingName(Logging logging, String MappingType, DataTable dt, int TaskTypeId, string SourceSystemType, string SourceType, string TargetSystemType, string TargetType, string TaskDatafactoryIR)
        {
            logging.LogDebug("Get TaskTypeMappingName called.");

            string _ex = string.Format("MappingType = '{6}' and SourceSystemType = '{0}' and SourceType = '{1}' and TargetSystemType = '{2}' and TargetType = '{3}' and TaskDatafactoryIR = '{4}' and TaskTypeId = {5}", SourceSystemType, SourceType, TargetSystemType, TargetType, TaskDatafactoryIR, TaskTypeId, MappingType);

            DataRow[] foundRows = dt.Select(_ex);

            if (foundRows.Count() > 1 || foundRows.Count() == 0)
            {
                throw new System.ArgumentException(string.Format("Invalid TypeMapping for SourceSystemType = '{0}' and SourceType = '{1}' and TargetSystemType = '{2}' and TargetType = '{3}' and TaskDatafactoryIR = '{4}'. Mapping returned: {5}", SourceSystemType, SourceType, TargetSystemType, TargetType, TaskDatafactoryIR, foundRows.Count()));
            }

            string TypeName = foundRows[0]["MappingName"].ToString();

            logging.LogDebug("Get TaskTypeMappingName complete.");

            return TypeName;
        }

    }

}







