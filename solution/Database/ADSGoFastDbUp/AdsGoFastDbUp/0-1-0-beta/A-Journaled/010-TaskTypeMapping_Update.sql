
Update  
[dbo].[TaskTypeMapping]
Set MappingName =  replace(replace(replace(MappingName, '-', '_'), 'OnP_SH_IR', 'IRB'), 'SH_IR', 'IRA') 


Update  
[dbo].[TaskTypeMapping]
Set TaskDataFactoryIR =  replace(replace(replace(TaskDataFactoryIR, '-', '_'), 'OnP SH IR', 'IRB'), 'SH IR', 'IRA') 

Update  
[dbo].[TaskMaster]
Set TaskDataFactoryIR =  replace(replace(replace(TaskDataFactoryIR, '-', '_'), 'OnP SH IR', 'IRB'), 'SH IR', 'IRA') 


Update [dbo].[TaskTypeMapping]
set SourceType = replace(replace(replace(SourceType,'csv', 'Csv'), 'json','Json'), 'JSON','Json')

Update [dbo].[TaskTypeMapping]
set TargetType = replace(replace(replace(TargetType,'csv', 'Csv'), 'json','Json'), 'JSON','Json')