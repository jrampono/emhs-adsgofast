/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using AdsGoFast.SqlServer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdsGoFast
{
    class TaskTypeMappings:IEnumerable<TaskTypeMapping>, IDisposable
    {
        public TaskTypeMappings()
        {
            TaskMetaDataDatabase TMD = new AdsGoFast.TaskMetaDataDatabase();
            _internallist = TMD.GetSqlConnection().QueryWithRetry<TaskTypeMapping>("select * from [dbo].[TaskTypeMapping] Where ActiveYN = 1").ToList();
        }
        private List<TaskTypeMapping> _internallist { get; set; }

        public void Dispose()
        {
            _internallist.Clear();
            _internallist = null;
        }

        public IEnumerator<TaskTypeMapping> GetEnumerator()
        {
            return ((IEnumerable<TaskTypeMapping>)_internallist).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)_internallist).GetEnumerator();
        }

        public TaskTypeMapping GetMapping(string SourceSystemType, string TargetSystemType, string SourceType, string TargetType, string TaskDatafactoryIR, string TaskExecutionType, Int64 TaskTypeId )
        {
            TaskTypeMapping ret;
            if (this._internallist.Where(x => (x.SourceSystemType == "*" || x.SourceSystemType == SourceSystemType) && x.SourceType == SourceType && (x.TargetSystemType == "*" || x.TargetSystemType == TargetSystemType) && x.TargetType == TargetType && x.TaskDatafactoryIR == TaskDatafactoryIR && x.MappingType == TaskExecutionType && x.TaskTypeId == TaskTypeId).Any())
            {
                ret = this._internallist.Where(x => (x.SourceSystemType == "*" || x.SourceSystemType == SourceSystemType) && x.SourceType == SourceType && (x.TargetSystemType == "*" || x.TargetSystemType == TargetSystemType) && x.TargetType == TargetType && x.TaskDatafactoryIR == TaskDatafactoryIR && x.MappingType == TaskExecutionType && x.TaskTypeId == TaskTypeId).First();
            }
            else
            {
                throw (new Exception(string.Format("Failed to find TaskTypeMapping record for SourceSystemType: {0}, TargetSystemType {1},  SourceType: {2}, TargetType: {3},TaskDatafactoryIR: {4}, TaskExecutionType: {5}, TaskTypeId: ",SourceSystemType, TargetSystemType, SourceType, TargetType, TaskDatafactoryIR, TaskExecutionType, TaskTypeId.ToString()) ));
            }

            return ret;
        }

    }

    class TaskTypeMapping
    {
            public Int64 TaskTypeId          {get; set;}
            public string MappingType         {get; set;}
            public string MappingName         {get; set;}
            public string SourceSystemType    {get; set;}
            public string SourceType          {get; set;}
            public string TargetSystemType    {get; set;}
            public string TargetType          {get; set;}
            public string TaskDatafactoryIR { get; set; }
            public string TaskMasterJsonSchema { get; set; }
            public string TaskInstanceJsonSchema { get; set; }
        
    }
}
