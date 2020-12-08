FOR %A IN ("DataFactory" "FrameworkTaskRunner" "ScheduleInstance" "ScheduleMaster" "SourceAndTargetSystems" "SourceAndTargetSystemsJsonSchema" "TaskGroup" "TaskGroupDependency" "TaskInstance" "TaskMaster" "TaskMasterWaterMark" "TaskType" "TaskTypeMapping")	DO 	dotnet aspnet-codegenerator controller -m %A -dc AdsGoFastContext -l "~/Views/Shared/_Layout.cshtml" -name %AController -outDir Controllers -f


REM dotnet ef dbcontext scaffold "Server=adsgofastdatakakeaccelsqlsvr.database.windows.net;Database=AdsGoFast;Integrated Security=False;User=#####;password=#######" Microsoft.EntityFrameworkCore.SqlServer  --context test --context-dir Models2 --output-dir Models2 --table test --no-build


