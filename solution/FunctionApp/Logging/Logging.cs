/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using AdsGoFast.SqlServer;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.InteropServices.ComTypes;
using Microsoft.Extensions.ObjectPool;

namespace AdsGoFast
{
    public class Logging
    {

        public enum LogType : short
        {
            Error = 1,
            Warning = 2,
            Performance = 3,
            Information = 4,
            Debug = 5
        }

        private int MaxLogLevelPersistedToDatabase { get; set; }

        public class ActivityLogItem
        {
            public DateTimeOffset LogDateTimeOffset { get; set; }
            public DateTimeOffset LogDateUTC { get; set; }
            public DateTimeOffset StartDateTimeOffset { get; set; }
            public DateTimeOffset EndDateTimeOffset { get; set; }

            public short? LogTypeId { get; set; }
            public Guid? ExecutionUid { get; set; }
            public string LogSource { get; set; }
            public long? TaskMasterId { get; set; }
            public long? TaskGroupId { get; set; }
            public long? TaskInstanceId { get; set; }
            public Guid? AdfRunUid { get; set; }
            public long? ScheduleInstanceId { get; set; }

            public string ActivityType { get; set; }

            public string Comment { get; set; }

            public string Status { get; set; }

            /// <summary>
            /// Constructor of setting the DefaultActivityItem
            /// </summary>
            public ActivityLogItem()
            {
                LogSource = "AF";
                LogDateTimeOffset = DateTimeOffset.UtcNow;
                LogDateUTC = LogDateTimeOffset.Date;
            }

            /// <summary>
            /// Constructor for all non DefaultActivityItems
            /// </summary>
            /// <param name="_DefaultActivityLogItem"></param>
            public ActivityLogItem(ActivityLogItem _DefaultActivityLogItem)
            {
                LogSource = "AF";
                LogDateTimeOffset = DateTimeOffset.UtcNow;
                LogDateUTC = LogDateTimeOffset.Date;

                //If the default has the values below then use these 
                if (_DefaultActivityLogItem != null)
                {
                    if (_DefaultActivityLogItem.ExecutionUid != null) { ExecutionUid = _DefaultActivityLogItem.ExecutionUid; }
                    if (_DefaultActivityLogItem.TaskInstanceId != null) { TaskInstanceId = _DefaultActivityLogItem.TaskInstanceId; }
                    if (_DefaultActivityLogItem.AdfRunUid != null) { AdfRunUid = _DefaultActivityLogItem.AdfRunUid; }
                    if (_DefaultActivityLogItem.TaskMasterId != null) { TaskMasterId = _DefaultActivityLogItem.TaskMasterId; }
                    if (_DefaultActivityLogItem.ActivityType != null) { ActivityType = _DefaultActivityLogItem.ActivityType; }

                }


            }

            public LogParamsAndTemplate WriteToStandardLog()
            {
                LogParamsAndTemplate ret = new LogParamsAndTemplate();
                ret._params = new List<object>();
                ret.Template = @"";

                ret.Template += "LogSource={LogSource}"; ret._params.Add(this.LogSource);
                ret.Template += ",LogDateTimeOffset={LogDateTimeOffset}"; ret._params.Add(this.LogDateTimeOffset);
                ret.Template += ",LogDateUTC={LogDateUTC}"; ret._params.Add(this.LogDateUTC);

                if (this.AdfRunUid != null) { ret.Template += ",AdfRunUid={AdfRunUid}"; ret._params.Add(this.AdfRunUid); }
                if (this.ActivityType != null) { ret.Template += ",ActivityType={ActivityType}"; ret._params.Add(this.ActivityType); }
                if (this.Comment != null) { ret.Template += ",Comment={Comment}"; ret._params.Add(this.Comment); }
                if (this.ExecutionUid != null) { ret.Template += ",ExecutionUid={ExecutionUid}"; ret._params.Add(this.ExecutionUid); }
                if (this.ScheduleInstanceId != null) { ret.Template += ",ScheduleInstanceId={ScheduleInstanceId}"; ret._params.Add(this.ScheduleInstanceId); }
                if (this.Status != null) { ret.Template += ",Status={Status}"; ret._params.Add(this.Status); }
                if (this.TaskInstanceId != null) { ret.Template += ",TaskInstanceId={TaskInstanceId}"; ret._params.Add(this.TaskInstanceId); }
                if (this.TaskMasterId != null) { ret.Template += ",TaskMasterId={TaskMasterId}"; ret._params.Add(this.TaskMasterId); }
              

                return ret;
            }

            public class LogParamsAndTemplate
            {
                public List<object> _params { get; set; }
                public string Template { get; set; }
            }
        }

        public ActivityLogItem DefaultActivityLogItem { get; set; }
        private ILogger log { get; set; }
        private List<ActivityLogItem> ActivityLogItems { get; set; }
        public void InitializeLog(ILogger _log, ActivityLogItem _DefaultActivityLogItem)
        {
            log = _log;
            ActivityLogItems = new List<ActivityLogItem>();
            DefaultActivityLogItem = _DefaultActivityLogItem;
            MaxLogLevelPersistedToDatabase = 5;
        }


        public void LogErrors(System.Exception e)
        {
            LogErrors(e, new ActivityLogItem(DefaultActivityLogItem));
        }

        public void LogErrors(System.Exception e, ActivityLogItem activityLogItem)
        {
            activityLogItem.LogTypeId = (short)LogType.Error;
            activityLogItem.Status = "Failed";
            string msg = e.Message;

            if (e.StackTrace != null)
            {
                msg += "; Stack Trace: " + e.StackTrace;
            }

            log.LogError(msg);
            activityLogItem.Comment = msg;

            ActivityLogItem.LogParamsAndTemplate lpt = activityLogItem.WriteToStandardLog();
            log.LogError(lpt.Template, lpt._params.ToArray());

            if (activityLogItem.LogTypeId <= MaxLogLevelPersistedToDatabase)
            { ActivityLogItems.Add(activityLogItem); }
        }

        public void LogInformation(string Message)
        {
            LogInformation(Message, new ActivityLogItem(DefaultActivityLogItem));
        }

        public void LogInformation(string Message, ActivityLogItem activityLogItem)
        {
            activityLogItem.LogTypeId = (short)LogType.Information;          
            activityLogItem.Comment = Message;
            ActivityLogItem.LogParamsAndTemplate lpt = activityLogItem.WriteToStandardLog();
            log.LogInformation(lpt.Template, lpt._params.ToArray());
            if (activityLogItem.LogTypeId <= MaxLogLevelPersistedToDatabase)
            { ActivityLogItems.Add(activityLogItem); }
        }

        public void LogWarning(string Message)
        {
            LogWarning(Message, new ActivityLogItem(DefaultActivityLogItem));
        }
        public void LogWarning(string Message, ActivityLogItem activityLogItem)
        {
            activityLogItem.LogTypeId = (short)LogType.Warning;            
            activityLogItem.Comment = Message;
            ActivityLogItem.LogParamsAndTemplate lpt = activityLogItem.WriteToStandardLog();
            log.LogWarning(lpt.Template, lpt._params.ToArray());
            if (activityLogItem.LogTypeId <= MaxLogLevelPersistedToDatabase)
            { ActivityLogItems.Add(activityLogItem); }
        }

        public void LogDebug(string Message)
        {
            LogDebug(Message, new ActivityLogItem(DefaultActivityLogItem));
        }
        public void LogDebug(string Message, ActivityLogItem activityLogItem)
        {
            activityLogItem.LogTypeId = (short)LogType.Debug;
            activityLogItem.Comment = Message;
            ActivityLogItem.LogParamsAndTemplate lpt = activityLogItem.WriteToStandardLog();
            log.LogDebug(lpt.Template, lpt._params.ToArray());
            if (activityLogItem.LogTypeId <= MaxLogLevelPersistedToDatabase)
            { ActivityLogItems.Add(activityLogItem); }
        }

        public void LogPerformance(string Message)
        {
            LogPerformance(Message, new ActivityLogItem(DefaultActivityLogItem));
        }
        public void LogPerformance(string Message, ActivityLogItem activityLogItem)
        {
            activityLogItem.LogTypeId = (short)LogType.Performance;
            activityLogItem.Status = "Performance";
            activityLogItem.Comment = Message;
            ActivityLogItem.LogParamsAndTemplate lpt = activityLogItem.WriteToStandardLog();
            log.LogInformation(lpt.Template, lpt._params.ToArray());
            if (activityLogItem.LogTypeId <= MaxLogLevelPersistedToDatabase)
            { ActivityLogItems.Add(activityLogItem); }
        }


        public void PersistLogToDatabase()
        {
            //Note this has now been superseeded as low level logging now integrated into log analytics


            //DataTable dt =  ActivityLogItems.ToDataTable<ActivityLogItem>();
            //Table t = new Table();
            //t.Schema = "dbo";
            //t.Name = string.Format("#ActivityAudit{0}", Guid.NewGuid().ToString());
            //TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            //using (SqlConnection _con = TMD.GetSqlConnection())
            //{
            //    TMD.BulkInsert(dt, t, true, _con);
            //    string MergeSQL = TMD.GenerateInsertSQL(t.Schema, t.Name, "dbo", "ActivityAudit", _con);
            //    _con.ExecuteWithRetry(MergeSQL);
            //    _con.Close();
            //    _con.Dispose();
            //}
        }
    }

}
