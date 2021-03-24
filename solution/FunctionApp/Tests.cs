/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using AdsGoFast.SqlServer;
using AdsGoFast.TaskMetaData;
using Cronos;
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace AdsGoFast
{

    public static class Testing_MarkTasksCompleteTimerTrigger
    {
        //[FunctionName("Testing_MarkTasksCompleteTimerTrigger")]
        //public static async Task Run([TimerTrigger("0 0 */1 * * *")] TimerInfo myTimer, ILogger log, ExecutionContext context)
        //{
        //    Guid ExecutionId = context.InvocationId;

        //    using (FrameworkRunner FR = new FrameworkRunner(log, ExecutionId))
        //    {
        //        FrameworkRunner.FrameworkRunnerWorker worker = Testing_MarkTasksComplete.Testing_MarkTasksCompleteCore;
        //        FrameworkRunner.FrameworkRunnerResult result = FR.Invoke("Testing_MarkTasksCompleteTimerTrigger", worker);                
        //    }
        //}
    }

    public static class Testing_MarkTasksComplete
    {
        public static dynamic Testing_MarkTasksCompleteCore(Logging logging)
        {
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            TMD.ExecuteSql(string.Format("Insert into Execution values ('{0}', '{1}', '{2}')", logging.DefaultActivityLogItem.ExecutionUid, DateTimeOffset.Now.ToString("u"), DateTimeOffset.Now.AddYears(999).ToString("u")));

            int res = TMD.GetSqlConnection().Execute(@"

            Declare @RunningTasks int = (
            Select Count(*)
            from TaskInstance 
            where TaskRunnerId is not null aND LastExecutionStatus = 'InProgress')

            Declare @TasksToBeMarkedComplete int = (
            SELECT ROUND(1 + (RAND() * @RunningTasks),0) AS RAND_1_100)

            Update TaskInstance 
            Set LastExecutionStatus = 'Completed', TaskRunnerId = null
            from TaskInstance a
            inner join 
            (
            Select *, Rn = ROW_NUMBER() over (order by TaskInstanceId) 
            from TaskInstance 
            where TaskRunnerId is not null aND LastExecutionStatus = 'InProgress'
            ) b on a.TaskInstanceId = b.TaskInstanceId
            where a.TaskRunnerId is not null and a.LastExecutionStatus = 'InProgress'
            and b.Rn <= @TasksToBeMarkedComplete

            ");

            return new { };
        }

        

    }

}







