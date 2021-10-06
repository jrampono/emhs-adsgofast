/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Dapper;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace AdsGoFast.GetTaskInstanceJSON
{
    /// <summary>
    /// POCO For GetTaskInstanceJSON stored procedure
    /// </summary>
    public class BaseTask
    {
        [JsonProperty(Required = Required.Always)]
        public Int64 TaskInstanceId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Int64 TaskMasterId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TaskStatus { get; set; }

        [JsonProperty(Required = Required.Always)]
        public Int64 TaskTypeId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TaskType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int NumberOfRetries { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int DegreeOfCopyParallelism { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TaskGroupConcurrency { get; set; }
        [JsonProperty(Required = Required.Always)]
        public int TaskGroupPriority { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TaskExecutionType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Int64 DataFactoryId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataFactoryName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string DataFactoryResourceGroup { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Guid DataFactorySubscriptionId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TaskMasterJson { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ScheduleMasterId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string ADFPipeline { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TaskDatafactoryIR { get; set; }


        

        //Task Instance
        public string TaskInstanceJson { get; set; }

        //Source
        [JsonProperty(Required = Required.Always)]
        public string SourceSystemType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Int64 SourceSystemId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string SourceSystemServer { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string SourceKeyVaultBaseUrl { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string SourceSystemAuthType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string SourceSystemSecretName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string SourceSystemUserName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string SourceSystemJSON { get; set; }



        //Target            
        [JsonProperty(Required = Required.Always)]
        public string TargetSystemType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public Int64 TargetSystemId { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TargetSystemServer { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TargetKeyVaultBaseUrl { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TargetSystemAuthType { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TargetSystemSecretName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TargetSystemUserName { get; set; }
        [JsonProperty(Required = Required.Always)]
        public string TargetSystemJSON { get; set; }
        
    } // End of class

   
}
