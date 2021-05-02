
[Environment]::SetEnvironmentVariable("ENVIRONMENT_NAME", "development")

. .\Steps\PushEnvFileIntoVariables.ps1
ParseEnvFile("$env:ENVIRONMENT_NAME")

Invoke-Expression -Command  ".\Steps\CI_BuildFunctionApp.ps1"