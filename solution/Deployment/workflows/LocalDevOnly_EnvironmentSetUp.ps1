#az login 
#az account set -s "jorampon internal consumption"


[Environment]::SetEnvironmentVariable("ENVIRONMENT_NAME", "development")
if (Test-Path -PathType Container -Path "../bin/"){New-Item -ItemType Directory -Force -Path "../bin"}
New-Item -Path "../bin/" -Name "GitEnv.txt" -type "file" -value "" -force   
. .\Steps\PushEnvFileIntoVariables.ps1
ParseEnvFile("$env:ENVIRONMENT_NAME")
Invoke-Expression -Command  ".\Steps\CD_SetResourceGroupHash.ps1"
