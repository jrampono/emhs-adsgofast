/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;


namespace AdsGoFast.GetTaskInstanceJSON.TaskMasterJson
{
    public class SourceSystemTypeIsSQLTaskTypeIsSQLDatabasetoAzureStorage
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string IncrementalType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ChunkField { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ChunkSize { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ExtractionSQL { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TableSchema { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TableName { get; set; }
    }
    public class SourceSystemTypeIsStorageAndTaskSourceTypeIsCSV
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string DataFileName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string SchemaFileName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool FirstRowAsHeader { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string SheetName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int SkipLineCount { get; set; }


    }

    public class SourceSystemTypeIsStorageAndTaskTypeIsAzureStoragetoAzureStorage
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string RelativePath { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string DataFileName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool Recursively { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool DeleteAfterCompletion { get; set; }


    }

    public class SourceSystemTypeIsStorageAndTaskTypeIsCacheAzureStorageFileList
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string StorageAccountToken { get; set; }

    }

    public class SourceSystemTypeIsStorageAndTaskTypeIsSasUri
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Guid TargetSystemUidInPHI { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int SasURIDaysValid { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string FileUploaderWebAppURL { get; set; }

    }

    public class TargetSystemTypeIsSQL
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string TableSchema { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string TableName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string StagingTableSchema { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string StagingTableName { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string PreCopySQL { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string PostCopySQL { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string MergeSQL { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string AutoGenerateMerge { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DynamicMapping { get; set; }

    }

}


namespace AdsGoFast.GetTaskInstanceJSON.TaskMasterJson.AZ_SQL_StoredProcedure
{
    public class Source
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TableSchema { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TableName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string StoredProcedure { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Parameters { get; set; }
    }

    public class Target
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }
    }

    public class TaskMasterJson
    {
        [JsonProperty(Required = Required.Always)]
        public Source Source { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Target Target { get; set; }
    }

}

namespace AdsGoFast.GetTaskInstanceJSON.TaskMasterJson.XX_SQL_AZ_Storage_Parquet
{
    public class Source
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string IncrementalType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ExtractionSQL { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TableSchema { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TableName { get; set; }
        [JsonProperty(Required = Required.Default)]
        public string ChunkField { get; set; }
        [JsonProperty(Required = Required.Default)]
        public Int64 ChunkSize { get; set; }
    }

    public class Target
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string RelativePath { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataFileName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string SchemaFileName { get; set; }
    }

    public class TaskMasterJson
    {
        [JsonProperty(Required = Required.Always)]
        public Source Source { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Target Target { get; set; }
    }


}

namespace AdsGoFast.GetTaskInstanceJSON.TaskMasterJson.AZ_Storage_Binary_AZ_Storage_Binary_XX_XX
{
    public class Source
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string RelativePath { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataFileName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string Recursively { get; set; }
        [JsonProperty(Required = Required.Always)]
        public bool TriggerUsingAzureStorageCache { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DeleteAfterCompletion { get; set; }
    }

    public class Target
    {
        [JsonProperty(Required = Required.Always)]
        public string Type { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string RelativePath { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataFileName { get; set; }
    }

    public class TaskMasterJson
    {
        [JsonProperty(Required = Required.Always)]
        public Source Source { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Target Target { get; set; }
    }


}
