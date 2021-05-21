
#[Environment]::SetEnvironmentVariable("ENVIRONMENT_NAME", "development")
#. .\Steps\PushEnvFileIntoVariables.ps1
#ParseEnvFile("$env:ENVIRONMENT_NAME")

######################################################
### Continuous Integration                         ####
######################################################
if (([Environment]::GetEnvironmentVariable("AdsOpts_CI_Enable")) -eq "True")
{
    Write-Host "Starting CI.."
    
    Invoke-Expression -Command  ".\Steps\CI_BuildFunctionApp.ps1"

    Invoke-Expression -Command  ".\Steps\CI_BuildWebApp.ps1"

    Invoke-Expression -Command  ".\Steps\CI_BuildAdsGoFastDatabase.ps1"

    Invoke-Expression -Command  ".\Steps\CI_BuildDataFactory.ps1"
    
    Write-Host "Finishing CI.."
}
