Write-Host "Configuring Function App"

$SourceFile = $env:AdsOpts_CD_FolderPaths_PublishZip + "/functionapp/Publish.zip"
if($env:AdsOpts_CD_Services_CoreFunctionApp_Enable -eq "True")
{
    
    #Update App Settings
    $appsettingsfile = $env:AdsOpts_CD_FolderPaths_PublishUnZip + "/functionapp/appsettings.json"
    $appSettings = Get-Content $appsettingsfile | ConvertFrom-Json
    $appSettings.ApplicationOptions.UseMSI = $true
    $appSettings.ApplicationOptions.ServiceConnections.AdsGoFastTaskMetaDataDatabaseServer = "$env:AdsOpts_CD_Services_AzureSQLServer_Name.database.windows.net"
    $appSettings.ApplicationOptions.ServiceConnections.AdsGoFastTaskMetaDataDatabaseName = $env:AdsOpts_CD_Services_AzureSQLServer_AdsGoFastDB_Name
    $appSettings.AzureAdAzureServicesViaAppReg.Domain=$env:AdsOpts_CD_ResourceGroup_Domain
    $appSettings.AzureAdAzureServicesViaAppReg.TenantId=$env:AdsOpts_CD_ResourceGroup_TenantId
    $appSettings.AzureAdAzureServicesViaAppReg.ClientId=$env:AdsOpts_CD_ServicePrincipals_FunctionAppAuthenticationSP_ClientId
    $appSettings | ConvertTo-Json  -Depth 10 | set-content $appsettingsfile

    #Repack CoreFunctionApp
    $CurrentPath = Get-Location
    Set-Location "./../bin/publish"
    $Path = (Get-Location).Path + "/zipped/functionapp" 
    New-Item -ItemType Directory -Force -Path $Path
    $Path = $Path + "/Publish.zip"
    Compress-Archive -Path '.\unzipped\functionapp\*' -DestinationPath $Path -force
    #Move back to workflows 
    Set-Location $CurrentPath
    
    # Deploy CoreFunctionApp App
    az functionapp deployment source config-zip --resource-group $env:AdsOpts_CD_ResourceGroup_Name --name $env:AdsOpts_CD_Services_CoreFunctionApp_Name --src $SourceFile

}
else 
{
    Write-Host "Skipped Configuring Function App"
}
