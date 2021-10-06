/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/

SET IDENTITY_INSERT [dbo].[SourceAndTargetSystems] ON 
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (1, N'AWSample-adsgofastdatakakeaccelsqlsvr', N'Azure SQL', N'AWSample Dev', N'adsgofastdatakakeaccelsqlsvr.database.windows.net', N'MSI', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'
    {
        "Database" : "AWSample"
    }
', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (2, N'Staging-adsgofastdatakakeaccelsqlsvr', N'Azure SQL', N'Staging Dev', N'adsgofastdatakakeaccelsqlsvr.database.windows.net', N'MSI', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'    {        "Database" : "Staging"    }', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (3, N'datalakeraw-adsgofastdatalakeaccelst', N'Azure Blob', N'datalakeraw Dev', N'https://adsgofastdatalakeaccelst.blob.core.windows.net', N'MSI', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'
    {
        "Container" : "datalakeraw"
    }
', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (4, N'datalakeraw-adsgofastdatalakeadls', N'ADLS', N'datalakeraw Dev ADLS Gen2', N'https://adsgofastdatalakeadls.dfs.core.windows.net', N'MSI', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'
    {
        "Container" : "datalakeraw"
    }
', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (5, N'adsgofastonpremadfir-dataingestion', N'File', N'DataIngestion File', N'\\\\adsgofastonpremadfir\\D$\\dataingestion\\', N'WindowsAuth', N'AzureUser', N'adsgofast-onpre-file-password', N'https://adsgofastkeyvault.vault.azure.net/', NULL, 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (6, N'AdventureWorks2017-adsgofast-onpre', N'SQL Server', N'AdventureWorks2017 Dev', N'adsgofast-onpre', N'SQLAuth', N'sqladfir', N'adsgofast-onpre-sqladfir-password', N'https://adsgofastkeyvault.vault.azure.net/', N'
    {
        "Database" : "AdventureWorks2017"
    }
', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (7, N'datalakelanding-adsgofastdatalakeaccelst', N'Azure Blob', N'datalakelanding Dev', N'https://adsgofastdatalakeaccelst.blob.core.windows.net', N'MSI', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'
    {
        "Container" : "datalakelanding"
    }
', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (8, N'datalakelanding-adsgofastdatalakeadls', N'ADLS', N'datalakelanding Dev ADLS Gen2', N'https://adsgofastdatalakeadls.dfs.core.windows.net', N'MSI', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'
    {
        "Container" : "datalakelanding"
    }
', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (9, N'adsgofasttransientstg', N'Azure Blob', N'adsgofasttransientstg Transient In Dev', N'https://adsgofasttransientstg.blob.core.windows.net', N'MSI', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'
    {
        "Container" : "transientin"
    }
', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (10, N'SendGrid Email For PipQi Upload', N'SendGrid', N'SendGrid Dev', N'ADSGoFastSendGrid', N'Key', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'    {
        "SenderEmail" : "noreply@cleverchiro.com",
        "SenderDescription" : "ADS Go Fast (No Reply)",
        "Subject" : "ADS GO Fast SAS Uri for File Upload",
        "PlainTextContent" : "Hello, Email!",
        "HtmlContent" : "<strong>Hi <NAME>, </strong> </br></br> Use the link below to upload files: </br> <SASTOKEN> </br></br>"
    }', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (11, N'Task Master Meta Data Database', N'Azure SQL', N'Task Master Meta Data Database', N'adsgofastdatakakeaccelsqlsvr.database.windows.net', N'MSI', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'
    {
        "Database" : "AdsGoFast"
    }
', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (15, N'Email For PipQi Upload Confirmation', N'SendGrid', N'SendGrid Dev', N'ADSGoFastSendGrid', N'Key', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'    {
        "SenderEmail" : "noreply@cleverchiro.com",
        "SenderDescription" : "ADS Go Fast (No Reply)",
        "Subject" : "ADS GO Fast SAS Uri for File Upload",
        "PlainTextContent" : "Hello, Email!",
        "HtmlContent" : "<strong>Hi <NAME>, </strong> </br></br> Use the link below to upload files: </br> <SASTOKEN> </br></br>"
    }', 1)
GO
INSERT [dbo].[SourceAndTargetSystems] ([SystemId], [SystemName], [SystemType], [SystemDescription], [SystemServer], [SystemAuthType], [SystemUserName], [SystemSecretName], [SystemKeyVaultBaseUrl], [SystemJSON], [ActiveYN]) VALUES (16, N'SH IR - Integration Runtime VM', N'AzureVM', N'SH IR - Integration Runtime VM', N'adsgofastadfir', N'MSI', NULL, NULL, N'https://adsgofastkeyvault.vault.azure.net/', N'    {        "SubscriptionUid" : "035a1364-f00d-48e2-b582-4fe125905ee3", "VMname" : "adsgofastadfir", "ResourceGroup":"ADSGOFASTDATALAKEACCEL" }', 1)
GO
SET IDENTITY_INSERT [dbo].[SourceAndTargetSystems] OFF
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
GO
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
GO
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
GO
INSERT [dbo].[SourceAndTargetSystems_JsonSchema] ([SystemType], [JsonSchema]) VALUES (N'AzureVM', N'{     "$schema": "http://json-schema.org/draft-04/schema#",     "type": "object",     "properties": {         "SubscriptionUid": {             "type": "string"         },         "VMname": {           "type": "string"       },       "ResourceGroup": {         "type": "string"     }     },     "required": [         "SubscriptionUid",         "VMname",         "ResourceGroup"      ] }')
GO
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
GO
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
GO