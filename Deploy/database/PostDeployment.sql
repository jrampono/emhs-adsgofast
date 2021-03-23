/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
/*
Post-Deployment Script Template							
--------------------------------------------------------------------------------------
 This file contains SQL statements that will be appended to the build script.		
 Use SQLCMD syntax to include a file in the post-deployment script.			
 Example:      :r .\myfile.sql								
 Use SQLCMD syntax to reference a variable in the post-deployment script.		
 Example:      :setvar TableName MyTable							
               SELECT * FROM [$(TableName)]					
--------------------------------------------------------------------------------------
*/

/*
SCHEDULEMASTER
*/


TRUNCATE TABLE [dbo].[ScheduleMaster]
GO

INSERT INTO [dbo].[ScheduleMaster] ([ScheduleCronExpression],[ScheduleDesciption],[ActiveYN])
VALUES ('* * * * * *','Every Second',1)
GO

INSERT INTO [dbo].[ScheduleMaster] ([ScheduleCronExpression],[ScheduleDesciption],[ActiveYN])
VALUES ('0 0 * * * *','Every Hour',1)
GO

INSERT INTO [dbo].[ScheduleMaster] ([ScheduleCronExpression],[ScheduleDesciption],[ActiveYN])
VALUES ('0 * * * * *','Every Minute',1)
GO

INSERT INTO [dbo].[ScheduleMaster] ([ScheduleCronExpression],[ScheduleDesciption],[ActiveYN])
VALUES ('N/A','Run Once Only',1)
GO

INSERT INTO [dbo].[ScheduleMaster] ([ScheduleCronExpression],[ScheduleDesciption],[ActiveYN])
VALUES ('0 0 1 */3 * *','At 00:00 on day-of-month 1 in every 3rd month',1)
GO


/*
Subject Area
*/


--SET IDENTITY_INSERT [dbo].[SubjectArea] ON
--INSERT INTO [dbo].[SubjectArea] ([SubjectAreaId], [SubjectAreaName], [ActiveYN], [SubjectAreaFormId], [DefaultTargetSchema], [UpdatedBy]) VALUES (1, N'Default - Admin', 1, NULL, N'**ALL**', N'user@adsgofast.com')
--INSERT INTO [dbo].[SubjectArea] ([SubjectAreaId], [SubjectAreaName], [ActiveYN], [SubjectAreaFormId], [DefaultTargetSchema], [UpdatedBy]) VALUES (2, N'Secured - Test SubjectArea', 1, NULL, N'TestSubjectArea', N'user@adsgofast.com')
--SET IDENTITY_INSERT [dbo].[SubjectArea] OFF

/*
TASKGROUP
*/

TRUNCATE TABLE [dbo].[TaskGroup]
Go

INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[SubjectAreaId], [TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Ingest Excel to Azure SQL',1, 0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[SubjectAreaId], [TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Ingest Adventureworks to Data Lake (Azure SQL)', 1,0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[SubjectAreaId], [TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Ingest Adventureworks (SQL 2019) to Data Lake', 1,0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[SubjectAreaId], [TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Ingest Adventureworks (Data Lake) to Staging (Azure SQL)', 1,0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[SubjectAreaId], [TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Generate Task Masters',1, 0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[SubjectAreaId], [TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Ingest Adventureworks (SQL 2019/Data Lake) in Azure SQL',1, 0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[SubjectAreaId], [TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Move between Storage',1, 0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[SubjectAreaId], [TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Build Dimensions',1, 0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[SubjectAreaId], [TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Generate SAS URI',1, 0,10,null,1)
GO

/*
TASKGROUPDEPENDENCY
*/

TRUNCATE TABLE [dbo].[TaskGroupDependency]
GO

INSERT INTO [dbo].[TaskGroupDependency] ([AncestorTaskGroupId],[DescendantTaskGroupId],[DependencyType])
VALUES (2, 4 ,'TasksMatchedByTagAndSchedule')
GO

INSERT INTO [dbo].[TaskGroupDependency] ([AncestorTaskGroupId],[DescendantTaskGroupId],[DependencyType])
VALUES (3, 6 ,'TasksMatchedByTagAndSchedule')
GO

/*
[dbo].[FrameworkTaskRunner]
*/
TRUNCATE TABLE [dbo].[FrameworkTaskRunner]
GO

INSERT INTO [dbo].[FrameworkTaskRunner] values (1,'Runner 1',1,'Idle',30,null,null)
INSERT INTO [dbo].[FrameworkTaskRunner] values (2,'Runner 2',1,'Idle',30,null,null)
INSERT INTO [dbo].[FrameworkTaskRunner] values (3,'Runner 3',1,'Idle',30,null,null)
INSERT INTO [dbo].[FrameworkTaskRunner] values (4,'Runner 4',1,'Idle',30,null,null)
GO
/*
DATAFACTORY
*/
TRUNCATE TABLE [dbo].[DataFactory]
GO

INSERT INTO [dbo].[DataFactory] ([Name],[ResourceGroup],[SubscriptionUid],[DefaultKeyVaultURL],[LogAnalyticsWorkspaceId]) 
VALUES ($(ADFName),$(ResourceGroup),$(SubscriptionUid),$(DefaultKeyVaultURL),$(LogAnalyticsWorkspaceId))
GO

/*
SourceAndTargetSystems
*/
TRUNCATE TABLE [dbo].[SourceAndTargetSystems]
GO

SET IDENTITY_INSERT [dbo].[SourceAndTargetSystems] ON 
GO

INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (1, $(sampleDB)+'-'+$(SQLServer), N'Azure SQL', $(sampleDB), $(SQLServer)+'.database.windows.net', N'MSI', NULL, NULL, $(DefaultKeyVaultURL), N'
    {
        "Database" : "'+$(sampleDB)+'"
    }
', 1)
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (2, $(stagingDB)+'-'+$(SQLServer), N'Azure SQL', $(stagingDB), $(SQLServer)+'.database.windows.net', N'MSI', NULL, NULL, $(DefaultKeyVaultURL), N'
    {
        "Database" : "'+$(stagingDB)+'"
    }
', 1)
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (4, N'datalakeraw'+$(ADLSStorageAccount), N'ADLS', N'datalakeraw Dev ADLS Gen2', N'https://'+$(ADLSStorageAccount)+'.dfs.core.windows.net', N'MSI', NULL, NULL, $(DefaultKeyVaultURL), N'
    {
        "Container" : "datalakeraw"
    }
', 1)
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (5, $(VMOnPIR)+'-dataingestion', N'File', N'DataIngestion File', N'\\\\'+$(VMOnPIR)+'\\D$\\dataingestion\\', N'WindowsAuth', N'AzureUser', N'adsgofast-onpre-file-password', $(DefaultKeyVaultURL), NULL, 1)
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (6, N'AdventureWorks2017-'+$(VMOnPIR), N'SQL Server', N'AdventureWorks2017 Dev', $(VMOnPIR), N'SQLAuth', N'sqladfir', N'adsgofast-onpre-sqladfir-password', $(DefaultKeyVaultURL), N'
    {
        "Database" : "AdventureWorks2017"
    }
', 1)
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (8, N'datalakelanding'+$(ADLSStorageAccount), N'ADLS', N'datalakelanding Dev ADLS Gen2', N'https://'+$(ADLSStorageAccount)+'.dfs.core.windows.net', N'MSI', NULL, NULL, $(DefaultKeyVaultURL), N'
    {
        "Container" : "datalakelanding"
    }
', 1)
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (9, N'adsgofasttransientstg'+$(ADLSStorageAccount), N'Azure Blob', N'adsgofasttransientstg Transient In Dev', N'https://'+$(ADLSStorageAccount)+'.blob.core.windows.net', N'MSI', NULL, NULL, $(DefaultKeyVaultURL), N'
    {
        "Container" : "transientin"
    }
', 1)
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (10, N'SendGrid Email For PipQi Upload', N'SendGrid', N'SendGrid Dev', N'ADSGoFastSendGrid', N'Key', NULL, NULL, $(DefaultKeyVaultURL), N'    {
        "SenderEmail" : "noreply@cleverchiro.com",
        "SenderDescription" : "ADS Go Fast (No Reply)",
        "Subject" : "ADS GO Fast SAS Uri for File Upload",
        "PlainTextContent" : "Hello, Email!",
        "HtmlContent" : "<strong>Hi <NAME>, </strong> </br></br> Use the link below to upload files: </br> <SASTOKEN> </br></br>"
    }', 1)
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (11, N'Task Master Meta Data Database', N'Azure SQL', N'Task Master Meta Data Database', $(SQLServer)+'.database.windows.net', N'MSI', NULL, NULL, $(DefaultKeyVaultURL), N'
    {
        "Database" : "'+$(adsGoFastDB)+'"
    }
', 1)

SET IDENTITY_INSERT [dbo].[SourceAndTargetSystems] OFF

/*
SourceAndTargetSystems_JsonSchema
*/
TRUNCATE TABLE [dbo].[SourceAndTargetSystems_JsonSchema]
GO

INSERT [dbo].[SourceAndTargetSystems_JsonSchema] ([SystemType], [JsonSchema]) VALUES (N'ADLS', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Container": {
      "type": "string"
    }
  },
  "required": [
    "Container"
  ]}')
INSERT [dbo].[SourceAndTargetSystems_JsonSchema] ([SystemType], [JsonSchema]) VALUES (N'Azure Blob', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Container": {
      "type": "string"
    }
  },
  "required": [
    "Container"
  ]}')
INSERT [dbo].[SourceAndTargetSystems_JsonSchema] ([SystemType], [JsonSchema]) VALUES (N'Azure SQL', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Database": {
      "type": "string"
    }
  },
  "required": [
    "Database"
  ]
}')
INSERT [dbo].[SourceAndTargetSystems_JsonSchema] ([SystemType], [JsonSchema]) VALUES (N'SendGrid', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "SenderEmail": {
      "type": "string"
    },
    "SenderDescription": {
      "type": "string"
    }
  },
  "required": [
    "SenderEmail",
    "SenderDescription"
  ]
}')
INSERT [dbo].[SourceAndTargetSystems_JsonSchema] ([SystemType], [JsonSchema]) VALUES (N'SQL Server', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Database": {
      "type": "string"
    }
  },
  "required": [
    "Database"
  ]
}')

/*
TaskType
*/
TRUNCATE TABLE [dbo].[TaskType]
GO

SET IDENTITY_INSERT [dbo].[TaskType] ON 

INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (1, N'Azure Storage to SQL Database', N'ADF', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (2, N'Azure Storage to Azure Storage', N'ADF', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (3, N'SQL Database to Azure Storage', N'ADF', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (4, N'Execute ADF Pipeline', N'ADF', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (5, N'Execute SSIS Pipeline', N'ADF', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (6, N'Execute Databricks Notebook', N'ADB', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (7, N'Execute SQL Statement', N'ADF', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (8, N'Generate Task Masters', N'ADF', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (9, N'Generate and Send SAS URI File Upload Link', N'AF', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (10, N'Cache Azure Storage File List', N'AF', NULL, 1)
INSERT [dbo].[TaskType] ([TaskTypeId], [TaskTypeName], [TaskExecutionType], [TaskTypeJson], [ActiveYN]) VALUES (11, N'Send Email Alert Triggered by File in FileList Cache', N'AF', NULL, 1)
SET IDENTITY_INSERT [dbo].[TaskType] OFF

/*
TaskTypeMapping
*/
TRUNCATE TABLE [dbo].[TaskTypeMapping]
GO

SET IDENTITY_INSERT [dbo].[TaskTypeMapping] ON 

INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (1, 1, N'ADF', N'AZ-Storage-Excel-AZ-SQL-SH-IR', N'Azure Blob', N'Excel', N'Azure SQL', N'Table', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        },
        "FirstRowAsHeader": {
          "type": "string"
        },
        "SheetName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName",
        "FirstRowAsHeader",
        "SheetName"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "StagingTableSchema": {
          "type": "string"
        },
        "StagingTableName": {
          "type": "string"
        },
        "AutoCreateTable": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "PreCopySQL": {
          "type": "string"
        },
        "PostCopySQL": {
          "type": "string"
        },
        "MergeSQL": {
          "type": "string"
        },
        "AutoGenerateMerge": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "StagingTableSchema",
        "StagingTableName",
        "AutoCreateTable",
        "TableSchema",
        "TableName",
        "PreCopySQL",
        "PostCopySQL",
        "MergeSQL",
        "AutoGenerateMerge"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (2, 1, N'ADF', N'AZ-Storage-CSV-AZ-SQL-SH-IR', N'ADLS', N'csv', N'Azure SQL', N'Table', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        },
        "FirstRowAsHeader": {
          "type": "string"
        },
        "SkipLineCount": {
          "type": "string"
        },
        "MaxConcorrentConnections": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName",
        "FirstRowAsHeader",
        "SkipLineCount",
        "MaxConcorrentConnections"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "StagingTableSchema": {
          "type": "string"
        },
        "StagingTableName": {
          "type": "string"
        },
        "AutoCreateTable": {
          "type": "string"
        },
        "PreCopySQL": {
          "type": "string"
        },
        "PostCopySQL": {
          "type": "string"
        },
        "MergeSQL": {
          "type": "string"
        },
        "AutoGenerateMerge": {
          "type": "string"
        },
        "DynamicMapping": {
          "type": "object"
        }
      },
      "required": [
        "Type",
        "TableSchema",
        "TableName",
        "StagingTableSchema",
        "StagingTableName",
        "AutoCreateTable",
        "PreCopySQL",
        "PostCopySQL",
        "MergeSQL",
        "AutoGenerateMerge",
        "DynamicMapping"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (3, 1, N'ADF', N'AZ-Storage-JSON-AZ-SQL-SH-IR', N'ADLS', N'JSON', N'Azure SQL', N'Table', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        },
        "MaxConcorrentConnections": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName",
        "MaxConcorrentConnections"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "StagingTableSchema": {
          "type": "string"
        },
        "StagingTableName": {
          "type": "string"
        },
        "AutoCreateTable": {
          "type": "string"
        },
        "PreCopySQL": {
          "type": "string"
        },
        "PostCopySQL": {
          "type": "string"
        },
        "MergeSQL": {
          "type": "string"
        },
        "AutoGenerateMerge": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "TableSchema",
        "TableName",
        "StagingTableSchema",
        "StagingTableName",
        "AutoCreateTable",
        "PreCopySQL",
        "PostCopySQL",
        "MergeSQL",
        "AutoGenerateMerge"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (4, 2, N'ADF', N'AZ-Storage-Excel-AZ-Storage-CSV-SH-IR', N'Azure Blob', N'Excel', N'Azure Blob', N'csv', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        },
        "FirstRowAsHeader": {
          "type": "string"
        },
        "SheetName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName",
        "FirstRowAsHeader",
        "SheetName"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        },
        "FirstRowAsHeader": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName",
        "FirstRowAsHeader"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (5, 2, N'ADF', N'AZ-Storage-Excel-AZ-Storage-CSV-SH-IR', N'ADLS', N'Excel', N'ADLS', N'csv', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        },
        "FirstRowAsHeader": {
          "type": "string"
        },
        "SheetName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName",
        "FirstRowAsHeader",
        "SheetName"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        },
        "FirstRowAsHeader": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName",
        "FirstRowAsHeader"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (6, 3, N'ADF', N'AZ-SQL-AZ-Storage-Parquet-SH-IR', N'Azure SQL', N'Table', N'Azure Blob', N'Parquet', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "IncrementalType": {
          "type": "string"
        },
        "ExtractionSQL": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "ChunkField": {
          "type": ["string","null"]
        },
        "ChunkSize": {
          "type": ["number","null"]
        }
      },
      "required": [
        "Type",
        "IncrementalType",
        "ExtractionSQL",
        "TableSchema",
        "TableName"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "TargetRelativePath": {
      "type": "string"
    },
    "IncrementalField": {
      "type": "string"
    },
    "IncrementalColumnType": {
      "type": "string"
    },
    "IncrementalValue": {
      "type": "string"
    }
  },
  "required": [
    "TargetRelativePath",
    "IncrementalField",
    "IncrementalColumnType",
    "IncrementalValue"
  ]
}')
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (7, 3, N'ADF', N'AZ-SQL-AZ-Storage-Parquet-SH-IR', N'Azure SQL', N'Table', N'ADLS', N'Parquet', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "IncrementalType": {
          "type": "string"
        },
        "ExtractionSQL": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "ChunkField": {
          "type": ["string","null"]
        },
        "ChunkSize": {
          "type": ["number","null"]
        }
      },
      "required": [
        "Type",
        "IncrementalType",
        "ExtractionSQL",
        "TableSchema",
        "TableName"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "TargetRelativePath": {
      "type": "string"
    },
    "IncrementalField": {
      "type": "string"
    },
    "IncrementalColumnType": {
      "type": "string"
    },
    "IncrementalValue": {
      "type": "string"
    }
  },
  "required": [
    "TargetRelativePath",
    "IncrementalField",
    "IncrementalColumnType",
    "IncrementalValue"
  ]
}')
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (8, 1, N'ADF', N'AZ-Storage-JSON-AZ-SQL-SH-IR', N'Azure Blob', N'JSON', N'Azure SQL', N'Table', N'SH IR', NULL, 1, NULL, NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (9, 1, N'ADF', N'AZ-Storage-Parquet-AZ-SQL-SH-IR', N'Azure Blob', N'Parquet', N'Azure SQL', N'Table', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "StagingTableSchema": {
          "type": "string"
        },
        "StagingTableName": {
          "type": "string"
        },
        "AutoCreateTable": {
          "type": "string"
        },
        "PreCopySQL": {
          "type": "string"
        },
        "PostCopySQL": {
          "type": "string"
        },
        "MergeSQL": {
          "type": "string"
        },
        "AutoGenerateMerge": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "TableSchema",
        "TableName",
        "StagingTableSchema",
        "StagingTableName",
        "AutoCreateTable",
        "PreCopySQL",
        "PostCopySQL",
        "MergeSQL",
        "AutoGenerateMerge"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (10, 1, N'ADF', N'AZ-Storage-Parquet-AZ-SQL-SH-IR', N'ADLS', N'Parquet', N'Azure SQL', N'Table', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "StagingTableSchema": {
          "type": "string"
        },
        "StagingTableName": {
          "type": "string"
        },
        "AutoCreateTable": {
          "type": "string"
        },
        "PreCopySQL": {
          "type": "string"
        },
        "PostCopySQL": {
          "type": "string"
        },
        "MergeSQL": {
          "type": "string"
        },
        "AutoGenerateMerge": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "TableSchema",
        "TableName",
        "StagingTableSchema",
        "StagingTableName",
        "AutoCreateTable",
        "PreCopySQL",
        "PostCopySQL",
        "MergeSQL",
        "AutoGenerateMerge"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (11, 2, N'ADF', N'GEN-File-Binary-AZ-Storage-Binary-SH-IR', N'File', N'Binary', N'Azure Blob', N'Binary', N'SH IR', NULL, 1, NULL, NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (12, 2, N'ADF', N'GEN-File-Binary-AZ-Storage-Binary-OnP-SH-IR', N'File', N'Binary', N'Azure Blob', N'Binary', N'OnP SH IR', NULL, 1, NULL, NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (13, 3, N'ADF', N'OnP-SQL-AZ-Storage-Parquet-OnP-SH-IR', N'SQL Server', N'Table', N'Azure Blob', N'Parquet', N'OnP SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "IncrementalType": {
          "type": "string"
        },
        "ExtractionSQL": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "ChunkField": {
          "type": ["string","null"]
        },
        "ChunkSize": {
          "type": ["number","null"]
        }
      },
      "required": [
        "Type",
        "IncrementalType",
        "ExtractionSQL",
        "TableSchema",
        "TableName"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (14, 3, N'ADF', N'OnP-SQL-GEN-File-Parquet-OnP-SH-IR', N'SQL Server', N'Table', N'File', N'Parquet', N'OnP SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "IncrementalType": {
          "type": "string"
        },
        "ExtractionSQL": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "ChunkField": {
          "type": "string"
        },
        "ChunkSize": {
          "type": "number"
        }
      },
      "required": [
        "Type",
        "IncrementalType",
        "ExtractionSQL",
        "TableSchema",
        "TableName"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "TargetRelativePath": {
      "type": "string"
    },
    "IncrementalField": {
      "type": "string"
    },
    "IncrementalColumnType": {
      "type": "string"
    },
    "IncrementalValue": {
      "type": "string"
    }
  },
  "required": [
    "TargetRelativePath",
    "IncrementalField",
    "IncrementalColumnType",
    "IncrementalValue"
  ]
}')
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (15, 8, N'ADF', N'Create-Task-Master-AZ-SQL-SH-IR', N'Azure SQL', N'Table', N'Azure Blob', N'Parquet', N'SH IR', NULL, 1, NULL, NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (16, 8, N'ADF', N'Create-Task-Master-AZ-SQL-SH-IR', N'Azure SQL', N'Parquet', N'Azure Blob', N'Table', N'SH IR', NULL, 1, NULL, NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (17, 8, N'ADF', N'Create-Task-Master-AZ-SQL-SH-IR', N'Azure SQL', N'Table', N'ADLS', N'Parquet', N'SH IR', NULL, 1, NULL, NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (18, 8, N'ADF', N'Create-Task-Master-AZ-SQL-SH-IR', N'Azure SQL', N'Parquet', N'ADLS', N'Table', N'SH IR', NULL, 1, NULL, NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (19, 8, N'ADF', N'Create-Task-Master-AZ-SQL-OnP-SH-IR', N'SQL Server', N'Table', N'Azure Blob', N'Parquet', N'OnP SH IR', NULL, 1, NULL, NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (20, 8, N'ADF', N'Create-Task-Master-AZ-SQL-OnP-SH-IR', N'SQL Server', N'Parquet', N'Azure Blob', N'Table', N'OnP SH IR', NULL, 1, NULL, NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (21, 2, N'ADF', N'AZ-Storage-Binary-AZ-Storage-Binary-SH-IR', N'Azure Blob', N'Parquet', N'Azure Blob', N'Parquet', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "Recursively": {
          "type": "string"
        },
        "DeleteAfterCompletion": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "Recursively",
        "DeleteAfterCompletion"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (22, 2, N'ADF', N'AZ-Storage-Binary-AZ-Storage-Binary-SH-IR', N'Azure Blob', N'Parquet', N'ADLS', N'Parquet', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "Recursively": {
          "type": "string"
        },
        "DeleteAfterCompletion": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "Recursively",
        "DeleteAfterCompletion"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (23, 2, N'ADF', N'AZ-Storage-Binary-AZ-Storage-Binary-SH-IR', N'ADLS', N'Parquet', N'Azure Blob', N'Parquet', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "Recursively": {
          "type": "string"
        },
        "DeleteAfterCompletion": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "Recursively",
        "DeleteAfterCompletion"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (24, 2, N'ADF', N'AZ-Storage-Binary-AZ-Storage-Binary-SH-IR', N'ADLS', N'Parquet', N'ADLS', N'Parquet', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "Recursively": {
          "type": "string"
        },
        "DeleteAfterCompletion": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "Recursively",
        "DeleteAfterCompletion"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (25, 7, N'ADF', N'AZ-SQL-StoredProcedure-SH-IR', N'Azure SQL', N'StoredProcedure', N'Azure SQL', N'StoredProcedure', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "StoredProcedure": {
          "type": "string"
        },
        "Parameters": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "TableSchema",
        "TableName",
        "StoredProcedure",
        "Parameters"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        }
      },
      "required": [
        "Type"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (26, 9, N'AF', N'AZ-Storage-SAS-Uri-SMTP-Email', N'Azure Blob', N'SASUri', N'SendGrid', N'Email', N'N/A', NULL, 1, NULL, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "SourceRelativePath": {
      "type": "string"
    }
  },
  "required": [
    "SourceRelativePath"
  ]
}')
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (27, 10, N'AF', N'AZ-Storage-Cache-File-List', N'Azure Blob', N'Filelist', N'Azure SQL', N'Table', N'N/A', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "StorageAccountToken": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "StorageAccountToken"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        }
      },
      "required": [
        "Type"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (29, 2, N'ADF', N'AZ-Storage-Binary-AZ-Storage-Binary-SH-IR', N'Azure Blob', N'*', N'Azure Blob', N'*', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "Recursively": {
          "type": "string"
        },
        "TriggerUsingAzureStorageCache": {
          "type": "boolean"
        },
        "DeleteAfterCompletion": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "Recursively",
        "TriggerUsingAzureStorageCache",
        "DeleteAfterCompletion"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "SourceRelativePath": {
      "type": "string"
    },
    "TargetRelativePath": {
      "type": "string"
    }
  },
  "required": [
    "SourceRelativePath",
    "TargetRelativePath"
  ]
}')
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (30, 11, N'AF', N'Cache-File-List-To-Email-Alert', N'Azure Blob', N'*', N'SendGrid', N'*', N'*', NULL, 1, N'{
    "$schema": "http://json-schema.org/draft-07/schema",
    "type": "object",
    "required": [
        "Source",
        "Target"
    ],
    "properties": {
        "Source": {
            "type": "object",
            "required": [
                "Type",
                "RelativePath",
                "DataFileName",
                "Recursively",
                "TriggerUsingAzureStorageCache",
                "DeleteAfterCompletion"
            ],
            "properties": {
                "Type": {
                    "type": "string"
                },
                "RelativePath": {
                    "type": "string"
                },
                "DataFileName": {
                    "type": "string"
                },
                "Recursively": {
                    "type": "string"
                },
                "TriggerUsingAzureStorageCache": {
                    "type": "boolean"
                },
                "DeleteAfterCompletion": {
                    "type": "string"
                }
            },
            "additionalProperties": true
        },
        "Target": {
            "type": "object",
            "required": [
                "Type",
                "Alerts"
            ],
            "properties": {
                "Type": {
                    "type": "string"
                },
                "Alerts": {
                    "type": "array",
                    "additionalItems": true,
                    "items": {
                        "anyOf": [
                            {
                                "type": "object",
                                "required": [
                                    "AlertCategory",
                                    "EmailTemplateFileName",
                                    "EmailRecepient",
                                    "EmailSubject",
                                    "EmailRecepientName"
                                ],
                                "properties": {
                                    "AlertCategory": {
                                        "type": "string"
                                    },
                                    "EmailTemplateFileName": {
                                        "type": "string"
                                    },
                                    "EmailRecepient": {
                                        "type": "string"
                                    },
                                    "EmailSubject": {
                                        "type": "string"
                                    },
                                    "EmailRecepientName": {
                                        "type": "string"
                                    }
                                },
                                "additionalProperties": true
                            }
                        ]
                    }
                }
            },
            "additionalProperties": true
        }
    },
    "additionalProperties": true
}', N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "SourceRelativePath": {
      "type": "string"
    }
  },
  "required": [
    "SourceRelativePath"
  ]
}')

INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (31, 14, N'AF', N'StartAndStopVMs', N'AzureVM', N'*', N'AzureVM', N'*', N'*', N'{}', 1
, N'{  "definitions": {},  "$schema": "http://json-schema.org/draft-07/schema#",   "$id": "https://example.com/object1604368781.json",   "title": "TaskMasterJson",   "type": "object",  "required": [   "Source",   "Target"  ],  "properties": {   "Source": {    "$id": "#root/Source",     "title": "Source",     "type": "object",    "required": [     "Type"    ],    "properties": {     "Type": {      "$id": "#root/Source/Type",       "title": "Type",                      "type": "string",                     "enum": [                         "*"                     ],      "default": "*",      "examples": [       "*"      ],      "pattern": "^.*$"     }    }   } ,   "Target": {    "$id": "#root/Target",     "title": "Target",     "type": "object",    "required": [     "Type",     "Action"    ],    "properties": {     "Type": {      "$id": "#root/Target/Type",       "title": "Type",                      "type": "string",                     "enum": [                         "*"                     ],      "default": "*",      "examples": [       "*"      ],      "pattern": "^.*$"     },     "Action": {      "$id": "#root/Target/Action",       "title": "Action",                      "type": "string",                     "enum": [                         "Start","Stop"                     ],      "default": "Start",      "examples": [       "Start"      ],      "pattern": "^.*$"     }    }   }   } }'
, N'{}')
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (32, 3, N'ADF', N'SH-SQL-AZ-Storage-Parquet-SH-IR', N'SQL Server', N'Table', N'Azure Blob', N'Parquet', N'SH IR', NULL, 1, null, null)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (33, 1, N'ADF', N'AZ-Storage-Excel-AZ-SQL-SH-IR', N'ADLS', N'Excel', N'Azure SQL', N'Table', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        },
        "FirstRowAsHeader": {
          "type": "string"
        },
        "SheetName": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName",
        "FirstRowAsHeader",
        "SheetName"
      ]
    },
	"Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "StagingTableSchema": {
          "type": "string"
        },
        "StagingTableName": {
          "type": "string"
        },
        "AutoCreateTable": {
          "type": "string"
        },
        "PreCopySQL": {
          "type": "string"
        },
        "PostCopySQL": {
          "type": "string"
        },
        "MergeSQL": {
          "type": "string"
        },
        "AutoGenerateMerge": {
          "type": "string"
        },
        "DynamicMapping": {
          "type": "object"
        }
      },
      "required": [
        "Type",
        "TableSchema",
        "TableName",
        "StagingTableSchema",
        "StagingTableName",
        "AutoCreateTable",
        "PreCopySQL",
        "PostCopySQL",
        "MergeSQL",
        "AutoGenerateMerge",
        "DynamicMapping"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)
INSERT [dbo].[TaskTypeMapping] ([TaskTypeMappingId], [TaskTypeId], [MappingType], [MappingName], [SourceSystemType], [SourceType], [TargetSystemType], [TargetType], [TaskDatafactoryIR], [TaskTypeJson], [ActiveYN], [TaskMasterJsonSchema], [TaskInstanceJsonSchema]) VALUES (34, 1, N'ADF', N'AZ-Storage-CSV-AZ-SQL-SH-IR', N'Azure Blob', N'csv', N'Azure SQL', N'Table', N'SH IR', NULL, 1, N'{
  "$schema": "http://json-schema.org/draft-04/schema#",
  "type": "object",
  "properties": {
    "Source": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "RelativePath": {
          "type": "string"
        },
        "DataFileName": {
          "type": "string"
        },
        "SchemaFileName": {
          "type": "string"
        },
        "FirstRowAsHeader": {
          "type": "string"
        },
        "SkipLineCount": {
          "type": "string"
        },
        "MaxConcorrentConnections": {
          "type": "string"
        }
      },
      "required": [
        "Type",
        "RelativePath",
        "DataFileName",
        "SchemaFileName",
        "FirstRowAsHeader",
        "SkipLineCount",
        "MaxConcorrentConnections"
      ]
    },
    "Target": {
      "type": "object",
      "properties": {
        "Type": {
          "type": "string"
        },
        "TableSchema": {
          "type": "string"
        },
        "TableName": {
          "type": "string"
        },
        "StagingTableSchema": {
          "type": "string"
        },
        "StagingTableName": {
          "type": "string"
        },
        "AutoCreateTable": {
          "type": "string"
        },
        "PreCopySQL": {
          "type": "string"
        },
        "PostCopySQL": {
          "type": "string"
        },
        "MergeSQL": {
          "type": "string"
        },
        "AutoGenerateMerge": {
          "type": "string"
        },
        "DynamicMapping": {
          "type": "object"
        }
      },
      "required": [
        "Type",
        "TableSchema",
        "TableName",
        "StagingTableSchema",
        "StagingTableName",
        "AutoCreateTable",
        "PreCopySQL",
        "PostCopySQL",
        "MergeSQL",
        "AutoGenerateMerge",
        "DynamicMapping"
      ]
    }
  },
  "required": [
    "Source",
    "Target"
  ]
}', NULL)


SET IDENTITY_INSERT [dbo].[TaskTypeMapping] OFF

/*
[dbo].[TaskMaster]
*/
TRUNCATE TABLE [dbo].[TaskMaster]
GO

SET IDENTITY_INSERT [dbo].[TaskMaster] ON 

INSERT [dbo].[TaskMaster] ([TaskMasterId], [TaskMasterName], [TaskTypeId], [TaskGroupId], [ScheduleMasterId], [SourceSystemId], [TargetSystemId], [DegreeOfCopyParallelism], [AllowMultipleActiveInstances], [TaskDatafactoryIR], [TaskMasterJSON], [ActiveYN], [DependencyChainTag], [DataFactoryId]) VALUES (1, N'afs_lic_202006.xlsx - AFS_LIC_202006', 1, 1, 2, 4, 2, 1, 0, N'SH IR', N'
{
    "Source": {
        "Type": "Excel",               
        "RelativePath": "/Gov/AUFinancialLicenses/",
        "DataFileName": "afs_lic_202006.xlsx",
        "SchemaFileName": "",
        "FirstRowAsHeader": "True",
        "SheetName": "AFS_LIC_202006"
    },
    "Target": {
        "Type": "Table",
        "StagingTableSchema": "dbo",
        "StagingTableName": "stg_AUFinancialLicenses",
        "AutoCreateTable": "False",
        "TableSchema": "dbo",
        "TableName": "AUFinancialLicenses",
        "PreCopySQL": "IF OBJECT_ID(''dbo.stg_AUFinancialLicenses'') IS NOT NULL 
            Truncate Table dbo.stg_AUFinancialLicenses",
        "PostCopySQL": "",
        "MergeSQL": "",
        "AutoGenerateMerge": "False"
    }
}
', 0, NULL, 1)
INSERT [dbo].[TaskMaster] ([TaskMasterId], [TaskMasterName], [TaskTypeId], [TaskGroupId], [ScheduleMasterId], [SourceSystemId], [TargetSystemId], [DegreeOfCopyParallelism], [AllowMultipleActiveInstances], [TaskDatafactoryIR], [TaskMasterJSON], [ActiveYN], [DependencyChainTag], [DataFactoryId]) VALUES (2, N'yellow_tripdata_2017-03.csv - NYTaxi', 1, 1, 2, 4, 2, 1, 0, N'SH IR', N'
{
    "Source": {
        "Type": "csv",        
        "RelativePath": "/NYTaxi/",
        "DataFileName": "yellow_tripdata_2017-03.csv",
        "SchemaFileName": "",
        "FirstRowAsHeader": "True",
        "SkipLineCount": "0",
        "MaxConcorrentConnections" : "10"
    },
    "Target": {
        "Type": "Table",
        "TableSchema": "dbo",
        "TableName": "NYTaxiYellowTripData",
        "StagingTableSchema": "dbo",
        "StagingTableName": "stg_NYTaxiYellowTripData",
        "AutoCreateTable": "False",
        "PreCopySQL": "IF OBJECT_ID(''dbo.stg_NYTaxiYellowTripData'') IS NOT NULL 
            Truncate Table dbo.stg_NYTaxiYellowTripData",
        "PostCopySQL": "",
        "MergeSQL": "",
        "AutoGenerateMerge": "True",
        "DynamicMapping": {
        }
    }
}
', 0, NULL, 1)
INSERT [dbo].[TaskMaster] ([TaskMasterId], [TaskMasterName], [TaskTypeId], [TaskGroupId], [ScheduleMasterId], [SourceSystemId], [TargetSystemId], [DegreeOfCopyParallelism], [AllowMultipleActiveInstances], [TaskDatafactoryIR], [TaskMasterJSON], [ActiveYN], [DependencyChainTag], [DataFactoryId]) VALUES (3, N'BrisbaneHospital JSON', 1, 1, 2, 4, 2, 1, 0, N'SH IR', N'
{
    "Source": {
        "Type": "json",        
        "RelativePath": "/Gov/BrisbaneHospital/",
        "DataFileName": "brisbane-hospital-registers-deaths-1933-1963.json",
        "SchemaFileName": "",
        "MaxConcorrentConnections" : "10"
    },
    "Target": {
        "Type": "Table",
        "TableSchema": "dbo",
        "TableName": "BrisbHospRegDeaths",
        "StagingTableSchema": "dbo",
        "StagingTableName": "stg_BrisbHospRegDeaths",
        "AutoCreateTable": "False",
        "PreCopySQL": "IF OBJECT_ID(''dbo.stg_BrisbHospRegDeaths'') IS NOT NULL 
            Truncate Table dbo.stg_BrisbHospRegDeaths",
        "PostCopySQL": "",
        "MergeSQL": "",
        "AutoGenerateMerge": "True"
    }
}
', 0, NULL, 1)
INSERT [dbo].[TaskMaster] ([TaskMasterId], [TaskMasterName], [TaskTypeId], [TaskGroupId], [ScheduleMasterId], [SourceSystemId], [TargetSystemId], [DegreeOfCopyParallelism], [AllowMultipleActiveInstances], [TaskDatafactoryIR], [TaskMasterJSON], [ActiveYN], [DependencyChainTag], [DataFactoryId]) VALUES (6, N'Generate Tasks for AdventureWorksLT Extract to Data Lake (ADLS)', 8, 5, 4, 1, 4, 1, 0, N'SH IR', N'  {
  "TaskMasterName": "AdventureWorksLT {@TableSchema@}.{@TableName@} Extract to Data Lake (ADLS)",
  "TaskTypeId": 3,
  "TaskGroupId": 2,
  "ScheduleMasterId": 2,
  "SourceSystemId": 1,
  "TargetSystemId": 4,
  "DegreeOfCopyParallelism": 1,
  "AllowMultipleActiveInstances": 0,
  "TaskDatafactoryIR": "SH IR",
  "Source": {
      "Type": "Table",
      "IncrementalType": "Full",
      "ExtractionSQL": "",
      "TableSchema": "{@TableSchema@}",
      "TableName": "{@TableName@}"
  },
  "Target": {
      "Type": "Parquet",
      "RelativePath": "AdventureWorksLT/{@TableSchema@}/{@TableName@}/{yyyy}/{MM}/{dd}/{hh}/{mm}/",
      "DataFileName": "{@TableSchema@}.{@TableName@}.parquet",
      "SchemaFileName": "{@TableSchema@}.{@TableName@}.json"
  },
  "ActiveYN": 1,
  "DependencyChainTag": "{@TableSchema@}.adls_{@TableName@}",
  "DataFactoryId":1
} ', 0, NULL, 1)
INSERT [dbo].[TaskMaster] ([TaskMasterId], [TaskMasterName], [TaskTypeId], [TaskGroupId], [ScheduleMasterId], [SourceSystemId], [TargetSystemId], [DegreeOfCopyParallelism], [AllowMultipleActiveInstances], [TaskDatafactoryIR], [TaskMasterJSON], [ActiveYN], [DependencyChainTag], [DataFactoryId]) VALUES (7, N'Generate Tasks for AdventureWorksLT Data Lake (ADLS) to SQL', 8, 5, 4, 1, 4, 1, 0, N'SH IR', N'  {
  "TaskMasterName": "AdventureWorksLT {@TableSchema@}.{@TableName@} Data Lake (ADLS) to SQL",
  "TaskTypeId": 1,
  "TaskGroupId": 4,
  "ScheduleMasterId": 2,
  "SourceSystemId": 4,
  "TargetSystemId": 2,
  "DegreeOfCopyParallelism": 1,
  "AllowMultipleActiveInstances": 0,
  "TaskDatafactoryIR": "SH IR",
  "Source": {
     "Type": "Parquet",
       "RelativePath": "AdventureWorksLT/{@TableSchema@}/{@TableName@}/{yyyy}/{MM}/{dd}/{hh}/{mm}/",
       "DataFileName": "{@TableSchema@}.{@TableName@}.parquet",
       "SchemaFileName": "{@TableSchema@}.{@TableName@}.json"
   },
   "Target": {
     "Type": "Table",
     "TableSchema": "{@TableSchema@}",
     "TableName": "{@TableName@}",
     "StagingTableSchema": "{@TableSchema@}",
     "StagingTableName": "stg_{@TableName@}",
     "AutoCreateTable": "True",
     "PreCopySQL": "IF OBJECT_ID(''{@TableSchema@}.stg_{@TableName@}'') IS NOT NULL \r\n Truncate Table {@TableSchema@}.stg_{@TableName@}",
     "PostCopySQL": "",
     "MergeSQL": "",
     "AutoGenerateMerge": "True"    
   },
  "ActiveYN": 1,
  "DependencyChainTag": "{@TableSchema@}.adls_{@TableName@}",
  "DataFactoryId":1
} ', 0, NULL, 1)
INSERT [dbo].[TaskMaster] ([TaskMasterId], [TaskMasterName], [TaskTypeId], [TaskGroupId], [ScheduleMasterId], [SourceSystemId], [TargetSystemId], [DegreeOfCopyParallelism], [AllowMultipleActiveInstances], [TaskDatafactoryIR], [TaskMasterJSON], [ActiveYN], [DependencyChainTag], [DataFactoryId]) VALUES (8, N'Generate Tasks for AdventureWorks2017 OnPrem Extract to Data Lake (ADLS)', 8, 5, 4, 6, 4, 1, 0, N'OnP SH IR', N'  {
  "TaskMasterName": "AdventureWorks2017 {@TableSchema@}.{@TableName@} Extract to Data Lake (ADLS)",
  "TaskTypeId": 3,
  "TaskGroupId": 3,
  "ScheduleMasterId": 2,
  "SourceSystemId": 6,
  "TargetSystemId": 4,
  "DegreeOfCopyParallelism": 1,
  "AllowMultipleActiveInstances": 0,
  "TaskDatafactoryIR": "OnP SH IR",
  "Source": {
      "Type": "Table",
      "IncrementalType": "Full",
      "ExtractionSQL": "",
      "TableSchema": "{@TableSchema@}",
      "TableName": "{@TableName@}"
  },
  "Target": {
      "Type": "Parquet",
      "RelativePath": "AdventureWorks2017/{@TableSchema@}/{@TableName@}/{yyyy}/{MM}/{dd}/{hh}/{mm}/",
      "DataFileName": "{@TableSchema@}.{@TableName@}.parquet",
      "SchemaFileName": "{@TableSchema@}.{@TableName@}.json"
  },
  "ActiveYN": 1,
  "DependencyChainTag": "{@TableSchema@}.{@TableName@}",
  "DataFactoryId":1
} ', 0, NULL, 0)
INSERT [dbo].[TaskMaster] ([TaskMasterId], [TaskMasterName], [TaskTypeId], [TaskGroupId], [ScheduleMasterId], [SourceSystemId], [TargetSystemId], [DegreeOfCopyParallelism], [AllowMultipleActiveInstances], [TaskDatafactoryIR], [TaskMasterJSON], [ActiveYN], [DependencyChainTag], [DataFactoryId]) VALUES (9, N'Generate Tasks for AdventureWorks2017 Data Lake (ADLS) to SQL', 8, 5, 4, 6, 4, 1, 0, N'OnP SH IR', N'  {
  "TaskMasterName": "AdventureWorks2017 {@TableSchema@}.{@TableName@} Data Lake (ADLS) to SQL",
  "TaskTypeId": 1,
  "TaskGroupId": 6,
  "ScheduleMasterId": 2,
  "SourceSystemId": 4,
  "TargetSystemId": 2,
  "DegreeOfCopyParallelism": 1,
  "AllowMultipleActiveInstances": 0,
  "TaskDatafactoryIR": "SH IR",
  "Source": {
     "Type": "Parquet",
       "RelativePath": "AdventureWorks2017/{@TableSchema@}/{@TableName@}/{yyyy}/{MM}/{dd}/{hh}/{mm}/",
       "DataFileName": "{@TableSchema@}.{@TableName@}.parquet",
       "SchemaFileName": "{@TableSchema@}.{@TableName@}.json"
   },
   "Target": {
     "Type": "Table",
     "TableSchema": "{@TableSchema@}",
     "TableName": "{@TableName@}",
     "StagingTableSchema": "{@TableSchema@}",
     "StagingTableName": "stg_{@TableName@}",
     "AutoCreateTable": "True",
     "PreCopySQL": "IF OBJECT_ID(''{@TableSchema@}.stg_{@TableName@}'') IS NOT NULL \r\n Truncate Table {@TableSchema@}.stg_{@TableName@}",
     "PostCopySQL": "",
     "MergeSQL": "",
     "AutoGenerateMerge": "True"    
   },
  "ActiveYN": 1,
  "DependencyChainTag": "{@TableSchema@}.{@TableName@}",
  "DataFactoryId":1
} ', 0, NULL, 0)
SET IDENTITY_INSERT [dbo].[TaskMaster] OFF
