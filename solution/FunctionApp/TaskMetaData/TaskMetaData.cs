/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using AdsGoFast.SqlServer;
using Microsoft.WindowsAzure.Storage.Auth;
using System.Collections.Generic;
using System.Linq;

namespace AdsGoFast
{



    public static class ActiveTasks
    {
        public static string GetActiveTasks(Logging logging, long TaskGroupId, short TopCount)
        {
            logging.LogDebug("Get Active Tasks called.");
            TokenCredential StorageToken = new TokenCredential(Shared._AzureAuthenticationCredentialProvider.GetAzureRestApiToken(string.Format("https://{0}.blob.core.windows.net/", Shared._ApplicationOptions.TestingOptions.TaskMetaDataStorageAccount), Shared._ApplicationOptions.UseMSI));
            string FileResult = Shared.Azure.Storage.ReadFile(Shared._ApplicationOptions.TestingOptions.TaskMetaDataStorageAccount, Shared._ApplicationOptions.TestingOptions.TaskMetaDataStorageContainer, Shared._ApplicationOptions.TestingOptions.TaskMetaDataStorageFolder, "ActiveTasks.json", StorageToken);
            return FileResult;
        }
    }

    public static class ActivePipelines
    {


        public static dynamic GetLongRunningPipelines(Logging logging)
        {
            logging.LogDebug("Get GetActivePipelines called.");
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            dynamic res = TMD.GetSqlConnection().QueryWithRetry(@"
            
            Select 
            * 
            from [dbo].[TaskInstanceExecution] 
            where Status in ('InProgress',  'Queued')
            and datediff(minute, StartDateTime, GetUtcDate()) > 30
            order by StartDateTime desc

            ");
            return res;
        }
        /// <summary>
        /// Returns a list of long running pipelines. These will be checked to ensure that they are still in-progress.
        /// </summary>
        /// <param name="logging"></param>
        /// <returns></returns>
        public static short CountActivePipelines(Logging logging)
        {
            logging.LogDebug("Get CountActivePipelines called.");
            TaskMetaDataDatabase TMD = new TaskMetaDataDatabase();
            IEnumerable<short> res = TMD.GetSqlConnection().QueryWithRetry<short>(@"
            
            Select 
            count(*) ActiveCount 
            from [dbo].[TaskInstance] 
            where LastExecutionStatus in ('InProgress',  'Queued') or (LastExecutionStatus in ('Untried',  'FailedRetry') and TaskRunnerId is not null)

            ");
            return res.First();
        }

    }


}
