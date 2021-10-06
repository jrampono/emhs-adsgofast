/*-----------------------------------------------------------------------

 Copyright (c) Microsoft Corporation.
 Licensed under the MIT license.

-----------------------------------------------------------------------*/
create view Pbi.ADFPipelineStats as
select a.ExecutionUid, a.TaskInstanceId, a.PipelineRunStatus,  b.* 
from ADFPipelineRun a 
left join ADFActivityRun b on a.DatafactoryId = b.DataFactoryId and a.PipelineRunUid = b.PipelineRunUid
