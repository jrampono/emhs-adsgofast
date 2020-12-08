
INSERT INTO [dbo].[ActivityAudit]
           ([ExecutionUid]
           ,[TaskInstanceId]
           ,[AdfRunUid]
           ,[LogTypeId]
           ,[LogSource]
           ,[LogDateUTC]
           ,[LogDateTimeOffSet]
           ,[Status]
           ,[Comment])
SELECT 
	[ExecutionUid]
	,[TaskInstanceId]
	,[AdfRunUid]
	,[LogTypeId]
	,[LogSource]
	,[LogDateUTC]
	,[LogDateTimeOffSet]
	,[Status]
	,[Comment]
FROM {tmpActivityAudit}