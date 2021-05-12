
[Environment]::SetEnvironmentVariable("ENVIRONMENT_NAME", "development")
. .\Steps\PushEnvFileIntoVariables.ps1
ParseEnvFile("$env:ENVIRONMENT_NAME")
Invoke-Expression -Command  ".\Steps\CD_SetResourceGroupHash.ps1"


######################################################
### Continuous Deployment                         ####
######################################################Write-Host ([Environment]::GetEnvironmentVariable("AdsOpts_CI_Enable"))
if (([Environment]::GetEnvironmentVariable("AdsOpts_CD_Enable")) -eq "True")
{
    Write-Host "Starting CD.."

    Invoke-Expression -Command  ".\Steps\CD_DeployStorageForLogging.ps1"
    
    Invoke-Expression -Command  ".\Steps\CD_DeployAppService.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployVnet.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployAzureSqlServer.ps1"

    Write-Host "Finishing CD.."
}

 #Invoke-Expression -Command  ".\Cleanup_RemoveAll.ps1"