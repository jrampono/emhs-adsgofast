/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdsGoFast.GetTaskInstanceJSON
{
    partial class ADFJsonBaseTask : BaseTask
    {

        public void ProcessTaskInstance(TaskTypeMappings ttm)
        {
            //Validate TaskInstance based on JSON Schema
            var mapping = ttm.GetMapping(SourceSystemType, TargetSystemType, _TaskMasterJsonSource["Type"].ToString(), _TaskMasterJsonTarget["Type"].ToString(), TaskDatafactoryIR, TaskExecutionType, TaskTypeId);
            string mapping_schema = mapping.TaskInstanceJsonSchema;
            if (mapping_schema != null)
            {
                _TaskIsValid = Shared.JsonHelpers.ValidateJsonUsingSchema(logging, mapping_schema, this.TaskInstanceJson, "Failed to validate TaskInstance JSON for TaskTypeMapping: " + mapping.MappingName + ". ");
            }

            if (_TaskIsValid)
            {
                ProcessTaskInstance_Default();
            }
        }

        /// <summary>
        /// Default Method which merges Source & Target attributes on TaskInstanceJson with existing Source and Target Attributes on TaskObject payload.
        /// </summary>

        public void ProcessTaskInstance_Default()
        {

            JObject Source = (JObject)_JsonObjectForADF["Source"];
            JObject Target = (JObject)_JsonObjectForADF["Target"];

            Shared.JsonHelpers.JsonObjectPropertyList SourcePL = new Shared.JsonHelpers.JsonObjectPropertyList() {
                                        { "SourceRelativePath", "RelativePath",false,false},
                                        { "IncrementalField", "IncrementalField",false,false},
                                        { "IncrementalColumnType", "IncrementalColumnType",false,false},
                                        { "IncrementalValue", "IncrementalValue",false,false}
            };            
            Shared.JsonHelpers.BuildJsonObjectWithValidation(logging, ref SourcePL,_TaskInstanceJson.ToString(), ref Source);

            if (TaskType == "SQL Database to Azure Storage")
            {
                if (Shared.JsonHelpers.CheckForJSONProperty(logging, "Extraction", (JObject)Source))
                {
                    JObject Extraction = (JObject)Source["Extraction"];

                    Shared.JsonHelpers.JsonObjectPropertyList ExtractionPL = new Shared.JsonHelpers.JsonObjectPropertyList() {                                        
                                        { "IncrementalField", "IncrementalField",false,false},
                                        { "IncrementalColumnType", "IncrementalColumnType",false,false},
                                        { "IncrementalValue", "IncrementalValue",false,false}
                    };                    

                    Shared.JsonHelpers.BuildJsonObjectWithValidation(logging, ref ExtractionPL, _TaskInstanceJson.ToString(), ref Extraction);

                    if (_TaskInstanceJson["IncrementalColumnType"] != null)
                    {
                        if (_TaskInstanceJson["IncrementalColumnType"].ToString() == "DateTime")
                        {
                            DateTime _IncrementalValue = (DateTime)_TaskInstanceJson["IncrementalValue"];
                            Extraction["IncrementalValue"] = _IncrementalValue.ToString("yyyy-MM-dd HH:mm:ss.fff");//JObject.Parse(T.TaskInstanceJson)["IncrementalValue"].ToString();
                        }
                        else if (_TaskInstanceJson["IncrementalColumnType"].ToString() == "BigInt")
                        {
                            int _IncrementalValue = (int)_TaskInstanceJson["IncrementalValue"];
                            Extraction["IncrementalValue"] = _IncrementalValue.ToString();//JObject.Parse(T.TaskInstanceJson)["IncrementalValue"].ToString();
                        }
                    }

                    Extraction["IncrementalType"] = TaskMetaData.TaskInstancesStatic.IncrementalType(_TaskMasterJson);
                    Extraction["SQLStatement"] = TaskMetaData.TaskInstancesStatic.CreateSQLStatement(_TaskMasterJson, _TaskInstanceJson, logging);

                    Source["Extraction"] = Extraction;
                }

            }

            Shared.JsonHelpers.JsonObjectPropertyList TargetPL = new Shared.JsonHelpers.JsonObjectPropertyList() {
                                        { "TargetRelativePath", "RelativePath",false,false} };
            Shared.JsonHelpers.BuildJsonObjectWithValidation(logging, ref TargetPL, _TaskInstanceJson.ToString(), ref Target);

            _JsonObjectForADF["Source"] = Source;
            _JsonObjectForADF["Target"] = Target;        

        
        }
    }
}
