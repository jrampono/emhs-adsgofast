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
TASKGROUP
*/

TRUNCATE TABLE [dbo].[TaskGroup]
Go

INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Load Excel',0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Load AwSample',0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Load OnPrem Adventureworks',0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Persist AwSample in Azure SQL',0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Generate Task Masters',0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Persist Adventureworks in Azure SQL',0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Move between Storage',0,10,null,1)
GO
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Build Dimensions',0,10,null,1)
GO
/*SasURI Sample Group*/
INSERT INTO [dbo].[TaskGroup] ([TaskGroupName],[TaskGroupPriority],[TaskGroupConcurrency],[TaskGroupJSON],[ActiveYN])
Values ('Generate SAS URI',0,10,null,1)
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

INSERT INTO [dbo].[FrameworkTaskRunner] values (1,'Runner 1',1,'Idle',null)
INSERT INTO [dbo].[FrameworkTaskRunner] values (2,'Runner 2',1,'Idle',null)
INSERT INTO [dbo].[FrameworkTaskRunner] values (3,'Runner 3',1,'Idle',null)
INSERT INTO [dbo].[FrameworkTaskRunner] values (4,'Runner 4',1,'Idle',null)
GO
/*
DATAFACTORY
*/
TRUNCATE TABLE [dbo].[DataFactory]
GO

INSERT INTO [dbo].[DataFactory] ([Name],[ResourceGroup],[SubscriptionUid],[DefaultKeyVaultURL]) 
VALUES ('','','','')

INSERT INTO [dbo].[DataFactory] ([Name],[ResourceGroup],[SubscriptionUid],[DefaultKeyVaultURL]) 
VALUES ('adsgofastdatakakeacceladf','AdsGoFastDataLakeAccel','xxxxxxxxxxxxxxxxxxx','https://xxxxxxxxxxxxxxxxxxxx.vault.azure.net/')
GO




/*
--Delete Unsupported datatype
DELETE from dbo.TaskMaster
where TaskMasterId in (
SELECT TaskMasterId FROM TaskInstance WHERE TaskInstanceId IN (SELECT distinct TaskInstanceId FROM ActivityAudit Where status = 'Failed' and Comment like '%Please add conversion logic to %')
)

DELETE TaskInstance WHERE TaskInstanceId IN (SELECT distinct TaskInstanceId FROM ActivityAudit Where status = 'Failed' and Comment like '%Please add conversion logic to %')

DELETE FROM TaskInstance Where TaskMasterId in (SELECT TaskMasterId FROM TaskMaster Where TaskMasterName like '%Person%Address%')
DELETE FROM TaskMaster Where TaskMasterName like '%Person%Address%'
DELETE FROM TaskInstance Where TaskMasterId in (SELECT TaskMasterId FROM TaskMaster Where TaskMasterName like '%Production%ProductDocument%')
DELETE FROM TaskMaster Where TaskMasterName like '%Production%ProductDocument%'
DELETE FROM TaskInstance Where TaskMasterId in (SELECT TaskMasterId FROM TaskMaster Where TaskMasterName like '%HumanResources%Shift%')
DELETE FROM TaskMaster Where TaskMasterName like '%HumanResources%Shift%'
DELETE FROM TaskInstance Where TaskMasterId in (SELECT TaskMasterId FROM TaskMaster Where TaskMasterName like '%HumanResources%JobCandidate%')
DELETE FROM TaskMaster Where TaskMasterName like '%HumanResources%JobCandidate%'
DELETE FROM TaskInstance Where TaskMasterId in (SELECT TaskMasterId FROM TaskMaster Where TaskMasterName like '%HumanResources%Employee%')
DELETE FROM TaskMaster Where TaskMasterName like '%HumanResources%Employee%'
DELETE FROM TaskInstance Where TaskMasterId in (SELECT TaskMasterId FROM TaskMaster Where TaskMasterName like  '%Production%Document%')
DELETE FROM TaskMaster Where TaskMasterName like '%Production%Document%'



--Set Table PrimaryKey
-- WaterMark with Chunk
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','CustomerID') Where TaskMasterName like 'AwSample SalesLT.Customer Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','ProductModelID') Where TaskMasterName like 'AwSample SalesLT.ProductModel Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','ProductDescriptionID') Where TaskMasterName like 'AwSample SalesLT.ProductDescription Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','ProductID') Where TaskMasterName like 'AwSample SalesLT.Product Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','ProductCategoryID') Where TaskMasterName like 'AwSample SalesLT.ProductCategory Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','SystemInformationID') Where TaskMasterName like 'AwSample dbo.BuildVersion Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','ErrorLogID') Where TaskMasterName like 'AwSample dbo.ErrorLog Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','AddressID') Where TaskMasterName like 'AwSample SalesLT.Address Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','SalesOrderDetailID') Where TaskMasterName like 'AwSample SalesLT.SalesOrderDetail Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'{@TablePrimaryKey}','SalesOrderID') Where TaskMasterName like 'AwSample SalesLT.SalesOrderHeader Extract to Data Lake'

--Reduce Chunk to 100 for testing
--UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'"5000"','"100"')  Where TaskMasterName like 'AwSample%Extract to Data Lake'

--Full Load with Chunk
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'"Watermark"','"Full"') Where TaskMasterName like 'AwSample SalesLT.ProductCategory Extract to Data Lake'

--FULL Load with no chunk
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'"Watermark"','"Full"') Where TaskMasterName like 'AwSample SalesLT.ProductModelProductDescription Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'"5000"','""') Where TaskMasterName like 'AwSample SalesLT.ProductModelProductDescription Extract to Data Lake'

UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'"Watermark"','"Full"') Where TaskMasterName like 'AwSample SalesLT.CustomerAddress Extract to Data Lake'
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'"500"','""') Where TaskMasterName like 'AwSample SalesLT.CustomerAddress Extract to Data Lake'

-- WaterMark with Chunk
UPDATE [dbo].[TaskMaster] SET TaskMasterJSON = Replace(TaskMasterJSON,'"5000"','""') Where TaskMasterName like 'AwSample SalesLT.SalesOrderHeader Extract to Data Lake'

UPDATE [dbo].[TaskMaster] SET ActiveYN = 1 Where TaskMasterName like 'AwSample%Extract to Data Lake'


-- Create WaterMark
TRUNCATE TABLE [dbo].[TaskMasterWaterMark]
GO 

INSERT INTO [dbo].[TaskMasterWaterMark]
([TaskMasterId],[TaskMasterWaterMarkColumn],[TaskMasterWaterMarkColumnType],[TaskMasterWaterMark_DateTime],[TaskMasterWaterMark_BigInt],[TaskWaterMarkJSON],[ActiveYN],[UpdatedOn])
SELECT TaskMasterId, CASE WHEN  TaskMasterName like 'AwSample%ErrorLog%Extract to Data Lake' THEN 'ErrorTime' ELSE 'ModifiedDate' END,'DateTime',null,null,null,1,GETDATE() FROM [dbo].[TaskMaster]  Where TaskMasterName like 'AwSample%Extract to Data Lake'
GO

INSERT INTO [dbo].[TaskMasterWaterMark]
([TaskMasterId],[TaskMasterWaterMarkColumn],[TaskMasterWaterMarkColumnType],[TaskMasterWaterMark_DateTime],[TaskMasterWaterMark_BigInt],[TaskWaterMarkJSON],[ActiveYN],[UpdatedOn])
SELECT TaskMasterId, 'ModifiedDate','DateTime',null,null,null,1,GETDATE() FROM [dbo].[TaskMaster]  Where TaskMasterName like 'AdventureWorks2017%' and TaskMasterJSON like '%Watermark%'
GO

-- OnPrem Full-Chunk
UPDATE TaskMaster Set TaskMasterJSON = REPLACE(TaskMasterJSON,'"IncrementalType": "Full"','"IncrementalType": "Full",      "ChunkField": "CustomerID",      "ChunkSize": "5000"') WHERE TaskMasterName like 'AdventureWorks2017%Sales%Customer%'
-- OnPrem Watermark
UPDATE TaskMaster Set TaskMasterJSON = REPLACE(TaskMasterJSON,'"IncrementalType": "Full"','"IncrementalType": "Watermark",      "ChunkField": "SalesOrderID",      "ChunkSize": ""') WHERE TaskMasterName like 'AdventureWorks2017%SalesOrderDetail%'
-- OnPrem Watermark-Chunk
UPDATE TaskMaster Set TaskMasterJSON = REPLACE(TaskMasterJSON,'"IncrementalType": "Full"','"IncrementalType": "Watermark",      "ChunkField": "SalesOrderID",      "ChunkSize": "5000"') WHERE TaskMasterName like 'AdventureWorks2017%SalesOrderHeader %'

INSERT INTO dbo.ADFPrice VALUES ('Orchestration','Runs',1.373/1000,'AUD','AzureIR','Activity, trigger and debug runs',getdate())
INSERT INTO dbo.ADFPrice VALUES ('Orchestration','Runs',1.373/1000,'AUD','Azure managed VNET integration runtime','Activity, trigger and debug runs',getdate())
INSERT INTO dbo.ADFPrice VALUES ('Orchestration','Runs',2.060/1000,'AUD','SelfhostedIR','Activity, trigger and debug runs',getdate())

INSERT INTO dbo.ADFPrice VALUES ('Execution-Data movement activities','DIU-hour',0.344,'AUD','AzureIR','Cost to execute an Azure Data Factory activity on the Azure integration runtime',getdate())
INSERT INTO dbo.ADFPrice VALUES ('Execution-Pipeline activities','Hour',0.007,'AUD','AzureIR','Cost to execute an Azure Data Factory activity on the Azure integration runtime',getdate())
INSERT INTO dbo.ADFPrice VALUES ('Execution-External pipeline activities','Hour',0.000344,'AUD','AzureIR','Cost to execute an Azure Data Factory activity on the Azure integration runtime',getdate())

INSERT INTO dbo.ADFPrice VALUES ('Execution-Data movement activities','DIU-hour',0.344,'AUD','Azure managed VNET integration runtime','Cost to execute an Azure Data Factory activity on the Azure managed VNET integration runtime',getdate())
INSERT INTO dbo.ADFPrice VALUES ('Execution-Pipeline activities','Hour',1.373,'AUD','Azure managed VNET integration runtime','Cost to execute an Azure Data Factory activity on the Azure managed VNET integration runtime',getdate())
INSERT INTO dbo.ADFPrice VALUES ('Execution-External pipeline activities','Hour',1.373,'AUD','Azure managed VNET integration runtime','Cost to execute an Azure Data Factory activity on the Azure managed VNET integration runtime',getdate())

INSERT INTO dbo.ADFPrice VALUES ('Execution-Data movement activities','DIU-hour',0.138,'AUD','SelfhostedIR','Cost to execute an Azure Data Factory activity on a self-hosted integration runtime',getdate())
INSERT INTO dbo.ADFPrice VALUES ('Execution-Pipeline activities','Hour',0.003,'AUD','SelfhostedIR','Cost to execute an Azure Data Factory activity on a self-hosted integration runtime',getdate())
INSERT INTO dbo.ADFPrice VALUES ('Execution-External pipeline activities','Hour',0.000138,'AUD','SelfhostedIR','Cost to execute an Azure Data Factory activity on a self-hosted integration runtime',getdate())


*/




