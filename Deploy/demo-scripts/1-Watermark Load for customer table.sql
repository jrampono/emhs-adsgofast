use adsgofast
go

update taskmaster set activeyn = 0
go

declare @TaskMasterId Int

select @TaskMasterId=TaskMasterId from taskmaster where taskmastername like '%SalesLT.Customer %'


INSERT [dbo].[TaskMasterWaterMark] ([TaskMasterId], [TaskMasterWaterMarkColumn], [TaskMasterWaterMarkColumnType], [TaskMasterWaterMark_DateTime], [TaskMasterWaterMark_BigInt], [TaskWaterMarkJSON], [ActiveYN], [UpdatedOn]) VALUES (@TaskMasterId, N'ModifiedDate', N'datetime', NULL, NULL, NULL, 1, CAST(N'0001-01-01T00:00:00.0000000+00:00' AS DateTimeOffset))
GO

--enable customer table ingestion for watermark test
update taskmaster set activeyn = 1 where taskmasterid in (select taskmasterid from taskmaster where taskmastername like '%SalesLT.Customer %' and [TaskMasterName] not like '%Generate Tasks%')

-- enable all taskmaster
update taskmaster set activeyn = 1 where taskmasterid in (select taskmasterid from taskmaster where [TaskMasterName] not like '%Generate Tasks%')
go