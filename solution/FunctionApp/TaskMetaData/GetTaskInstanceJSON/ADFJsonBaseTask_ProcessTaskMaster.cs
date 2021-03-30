/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using AdsGoFast.GetTaskInstanceJSON.TaskMasterJson;
using System.Reflection;

namespace AdsGoFast.GetTaskInstanceJSON
{
    partial class ADFJsonBaseTask : BaseTask
    {
        public void ProcessTaskMaster(TaskTypeMappings ttm)
        {

            //Validate TaskmasterJson based on JSON Schema
            var mapping = ttm.GetMapping(SourceSystemType, TargetSystemType, _TaskMasterJsonSource["Type"].ToString(), _TaskMasterJsonTarget["Type"].ToString(), TaskDatafactoryIR, TaskExecutionType, TaskTypeId);            
            string mapping_schema = mapping.TaskMasterJsonSchema;
            _TaskIsValid = Shared.JsonHelpers.ValidateJsonUsingSchema(logging, mapping_schema, this.TaskMasterJson, "Failed to validate TaskMaster JSON for TaskTypeMapping: " + mapping.MappingName + ". ");
            
            if (_TaskIsValid)
            {
                if (TaskType == "SQL Database to Azure Storage")
                {
                    ProcessTaskMaster_Mapping_XX_SQL_AZ_Storage_Parquet();
                    goto ProcessTaskMasterEnd;
                }

                if (TaskType == "Execute SQL Statement")
                {
                    ProcessTaskMaster_Mapping_AZ_SQL_StoredProcedure();
                    goto ProcessTaskMasterEnd;
                }

                if (TaskType == "Azure Storage to SQL Database")
                {
                    ProcessTaskMaster_Default();
                    goto ProcessTaskMasterEnd;
                }
                
                //Default Processing Branch              
                {
                    ProcessTaskMaster_Default();
                    goto ProcessTaskMasterEnd;
                }


            ProcessTaskMasterEnd:
                logging.LogInformation("ProcessTaskMasterJson Finished");

            }


        }

        public void ProcessTaskMaster_Mapping_XX_SQL_AZ_Storage_Parquet()
        {
            JObject Source = (JObject)_JsonObjectForADF["Source"];
            JObject Target = (JObject)_JsonObjectForADF["Target"];
            JObject Extraction = new JObject();

            Extraction.Merge(_TaskMasterJson["Source"], new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });

            Target.Merge(_TaskMasterJson["Target"], new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });
            Source["Extraction"] = Extraction;

            _JsonObjectForADF["Source"] = Source;
            _JsonObjectForADF["Target"] = Target;


        }

        
        public void ProcessTaskMaster_Mapping_AZ_SQL_StoredProcedure()
        {
            AdsGoFast.GetTaskInstanceJSON.TaskMasterJson.AZ_SQL_StoredProcedure.TaskMasterJson
                TaskMasterJsonObject = JsonConvert.DeserializeObject<AdsGoFast.GetTaskInstanceJSON.TaskMasterJson.AZ_SQL_StoredProcedure.TaskMasterJson>(JsonConvert.SerializeObject(_TaskMasterJsonSource));

            JObject Source = (JObject)_JsonObjectForADF["Source"];
            JObject Execute = new JObject();
            Execute["StoredProcedure"] = string.Format("Exec {0} {1} {2} {3}", TaskMasterJsonObject.Source.StoredProcedure, TaskMasterJsonObject.Source.Parameters, Environment.NewLine, " Select 1");

            Source["Execute"] = Execute;
            _JsonObjectForADF["Source"] = Source;

       }


        /// <summary>
        /// Default Method which merges Source & Target attributes on TaskMasterJson with existing Source and Target Attributes on TaskObject payload.
        /// </summary>

        public void ProcessTaskMaster_Default()
        {            

            JObject Source = (JObject)_TaskMasterJson["Source"];
            JObject Target = (JObject)_TaskMasterJson["Target"];

            Source.Merge(_JsonObjectForADF["Source"], new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });

            Target.Merge(_JsonObjectForADF["Target"], new JsonMergeSettings
            {
                // union array values together to avoid duplicates
                MergeArrayHandling = MergeArrayHandling.Union
            });

            _JsonObjectForADF["Source"] = Source;
            _JsonObjectForADF["Target"] = Target;
        }


        public void ProcessTaskMaster_SourceSystem_TypeIsStorage_TaskSourceTypeIsCSV()
        {
            AdsGoFast.GetTaskInstanceJSON.TaskMasterJson.SourceSystemTypeIsStorageAndTaskSourceTypeIsCSV TaskMasterJsonObject = JsonConvert.DeserializeObject<AdsGoFast.GetTaskInstanceJSON.TaskMasterJson.SourceSystemTypeIsStorageAndTaskSourceTypeIsCSV>(JsonConvert.SerializeObject(_TaskMasterJsonSource));

            JObject Source = (JObject)_JsonObjectForADF["Source"];

            Source["Type"] = TaskMasterJsonObject.Type;
            Source["DataFileName"] = TaskMasterJsonObject.DataFileName;
            Source["SchemaFileName"] = TaskMasterJsonObject.SchemaFileName;
            Source["FirstRowAsHeader"] = TaskMasterJsonObject.FirstRowAsHeader;
            Source["SheetName"] = TaskMasterJsonObject.SheetName;
            Source["SkipLineCount"] = TaskMasterJsonObject.SkipLineCount;

            _JsonObjectForADF["Source"] = Source;
        }

        public void ProcessTaskMaster_Target_TypeIsSQL()
        {
            AdsGoFast.GetTaskInstanceJSON.TaskMasterJson.TargetSystemTypeIsSQL TaskMasterJsonObject = JsonConvert.DeserializeObject<AdsGoFast.GetTaskInstanceJSON.TaskMasterJson.TargetSystemTypeIsSQL>(JsonConvert.SerializeObject(_TaskMasterJsonTarget));

            JObject Target = (JObject)_JsonObjectForADF["Target"];

            Target["Type"] = TaskMasterJsonObject.Type;
            Target["TableSchema"] = TaskMasterJsonObject.Type;
            Target["TableName"] = TaskMasterJsonObject.TableSchema;
            Target["StagingTableSchema"] = TaskMasterJsonObject.StagingTableSchema;
            Target["StagingTableName"] = TaskMasterJsonObject.StagingTableName;
            Target["AutoCreateTable"] = TaskMasterJsonObject.AutoGenerateMerge;
            Target["PreCopySQL"] = TaskMasterJsonObject.PreCopySQL;
            Target["PostCopySQL"] = TaskMasterJsonObject.PostCopySQL;
            Target["MergeSQL"] = TaskMasterJsonObject.MergeSQL;
            Target["AutoGenerateMerge"] = TaskMasterJsonObject.AutoGenerateMerge;
            Target["DynamicMapping"] = TaskMasterJsonObject.DynamicMapping;

            _JsonObjectForADF["Target"] = Target;
        }


    }
}
