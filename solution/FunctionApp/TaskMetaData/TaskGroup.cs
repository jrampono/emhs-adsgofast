/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using AdsGoFast.SqlServer;
using System.Collections.Generic;
using System.Linq;

namespace AdsGoFast.TaskMetaData
{

    public static class TaskGroupsStatic
    {
        /// <summary>
        /// Gets Active Task Groups with Active Tasks
        /// </summary>
        public static List<TaskGroup> GetActive()
        {
            //Logging.LogInformation("Get TaskGroup called.");
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            List<TaskGroup> res = TMD.GetSqlConnection().QueryWithRetry<TaskGroup>("Exec dbo.GetTaskGroups").ToList();
            return res;
        }
    }
    /// <summary>
    /// Gets Active Task Groups with Active Tasks
    /// </summary>
    public class TaskGroup
    {
        public long TaskGroupId { get; set; }
        public string TaskGroupName { get; set; }
        public int TaskGroupPriority { get; set; }
        public short TaskGroupConcurrency { get; set; }
        public string TaskGroupJSON { get; set; }
        public bool ActiveYN { get; set; }
        public short TaskCount { get; set; }
        public short ConcurrencySlotsAllocated { get; set; }
        public short TasksUnAllocated { get; set; }

    }





}
