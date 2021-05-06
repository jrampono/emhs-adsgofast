
[Environment]::SetEnvironmentVariable("ENVIRONMENT_NAME", "development")
. .\Steps\PushEnvFileIntoVariables.ps1
ParseEnvFile("$env:ENVIRONMENT_NAME")

######################################################
### Continuous Deployment                         ####
######################################################Write-Host ([Environment]::GetEnvironmentVariable("AdsOpts_CI_Enable"))
if (([Environment]::GetEnvironmentVariable("AdsOpts_CD_Enable")) -eq "True")
{
    Write-Host "Starting CD.."

    Invoke-Expression -Command  ".\Steps\CD_DeployResourceGroup.ps1"

    Invoke-Expression -Command  ".\Steps\CD_SetResourceGroupHash.ps1"
    
    Invoke-Expression -Command  ".\Steps\CD_DeployStorageForLogging.ps1"
    
    Invoke-Expression -Command  ".\Steps\CD_DeployAppService.ps1"

    Write-Host "Finishing CD.."
}