/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Text;

namespace AdsGoFast.GetTaskInstanceJSON.PayloadObjectPocos
{
    class TaskObject
    {
        /// <summary>
        /// Root Object - Mandatory - Al fields required
        /// </summary>
        public class Task
        {
            public int TaskInstanceId { get; set; }
            public int TaskMasterId { get; set; }
            public string TaskStatus { get; set; }
            public string TaskType { get; set; }
            public int Enabled { get; set; }
            public string ExecutionUid { get; set; }
            public int NumberOfRetries { get; set; }
            public int DegreeOfCopyParallelism { get; set; }
            public string KeyVaultBaseUrl { get; set; }
            public int ScheduleMasterId { get; set; }
            public int TaskGroupConcurrency { get; set; }
            public int TaskGroupPriority { get; set; }
            public Source Source { get; set; }
            public Target Target { get; set; }
            public DataFactory DataFactory { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class Source
        {
            ///Mandatory
            public string Type { get; set; }
            
            //SourceSystemType is AZ Storage
            public string StorageAccountName { get; set; }
            public string StorageAccountContainer { get; set; }
            public string StorageAccountAccessMethod { get; set; }
            public string RelativePath { get; set; }
            public string DataFileName { get; set; }


            public string SchemaFileName { get; set; }
            
            //TaskMasterJson Source System Type Is File && SourceSystemType is AzStorage
            public object FirstRowAsHeader { get; set; }
            public object SheetName { get; set; }
            public object SkipLineCount { get; set; }
            public object MaxConcorrentConnections { get; set; }
            
            
            //TaskMasterJson Source System 
            public object Recursively { get; set; }
            public object DeleteAfterCompletion { get; set; }
            public object UserId { get; set; }
            public object Host { get; set; }
            public object PasswordKeyVaultSecretName { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class Target
        {
            public string Type { get; set; }
            public Database Database { get; set; }
            public string TableSchema { get; set; }
            public string TableName { get; set; }
            public string StagingTableSchema { get; set; }
            public string StagingTableName { get; set; }
            public string AutoCreateTable { get; set; }
            public string PreCopySQL { get; set; }
            public string PostCopySQL { get; set; }
            public string MergeSQL { get; set; }
            public string AutoGenerateMerge { get; set; }
            public object DynamicMapping { get; set; }
        }
        /// <summary>
        /// 
        /// </summary>
        public class Database
        {
            public string SystemName { get; set; }
            public string Name { get; set; }
            public string AuthenticationType { get; set; }
        }
        /// <summary>
        /// Mandatory DataFactory Object
        /// </summary>
        public class DataFactory
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string ResourceGroup { get; set; }
            public string SubscriptionId { get; set; }
            public string ADFPipeline { get; set; }
        }

    }
}
