use adsgofast
go

/*
#### Clean data from System tables to re-run all Tasks
*/
truncate table [dbo].[Execution]
truncate table [dbo].[TaskInstance]
truncate table [dbo].[TaskInstanceExecution]
truncate table [dbo].[ScheduleInstance]
truncate table [dbo].[TaskMasterWaterMark]

/*
#### Set TaskRunners to Idle
*/
update [dbo].[FrameworkTaskRunner] set Status = 'Idle'

/*
#### Remove auto generated Task Masters
*/
Delete from TaskMaster where TaskMasterId > 9

/*
#### Disable All Task Master
*/
Update TaskMaster Set ActiveYN = 0

