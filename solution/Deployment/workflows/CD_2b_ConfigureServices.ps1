
[Environment]::SetEnvironmentVariable("ENVIRONMENT_NAME", "development")
. .\Steps\PushEnvFileIntoVariables.ps1
ParseEnvFile("$env:ENVIRONMENT_NAME")

Invoke-Expression -Command  ".\Steps\CD_SetResourceGroupHash.ps1"

#az login --service-principal --username $env:secrets_AZURE_CREDENTIALS_clientId --password $env:secrets_AZURE_CREDENTIALS_clientSecret --tenant $env:secrets_AZURE_CREDENTIALS_tenantId


######################################################
### Continuous Deployment - Configure             ####
######################################################Write-Host ([Environment]::GetEnvironmentVariable("AdsOpts_CI_Enable"))
if (([Environment]::GetEnvironmentVariable("AdsOpts_CD_Enable")) -eq "True")
{
    Write-Host "Starting CD.."

    Invoke-Expression -Command  ".\Steps\CD_ConfigureAzureSQLServer.ps1"

    Invoke-Expression -Command  ".\Steps\CD_ConfigureWebApp.ps1"

    Invoke-Expression -Command  ".\Steps\CD_ConfigureFunctionApp.ps1"  

    Write-Host "Finishing CD.."
}

#Invoke-Expression -Command  ".\Cleanup_RemoveAll.ps1"