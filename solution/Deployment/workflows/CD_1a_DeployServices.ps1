
######################################################
### Continuous Deployment                         ####
######################################################Write-Host ([Environment]::GetEnvironmentVariable("AdsOpts_CI_Enable"))
if (([Environment]::GetEnvironmentVariable("AdsOpts_CD_EnableDeploy")) -eq "True")
{
    Write-Debug "Starting CD.."

    Invoke-Expression -Command  ".\Steps\CD_DeployKeyVault.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployStorageForLogging.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployStorageADLS.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployStorageBlob.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployAppInsights.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployLogAnalytics.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployAppService.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployWebSite.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployFunctionApp.ps1"

    if($env:AdsOpts_CD_Services_Vnet_Enable -eq "True")
    {
        Invoke-Expression -Command  ".\Steps\CD_DeployVnet.ps1"           #vNet Integration
    }

    Invoke-Expression -Command  ".\Steps\CD_DeployAzureSqlServer.ps1"

    Invoke-Expression -Command  ".\Steps\CD_DeployADF.ps1"

    Write-Debug "Finishing CD.."
}
else 
{

    Write-Warning "CD_1a_DeployServices.ps1 skipped as flag in environment file is set to false" 
}

 #Invoke-Expression -Command  ".\Cleanup_RemoveAll.ps1"