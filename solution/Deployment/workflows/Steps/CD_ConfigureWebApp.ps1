Write-Host "Configuring Web App"

$SourceFile = $env:AdsOpts_CD_FolderPaths_PublishZip + "/webapplication/Publish.zip"
if($env:AdsOpts_CD_Services_WebSite_Enable -eq "True")
{

    #Update App Settings
    $appsettingsfile = $env:AdsOpts_CD_FolderPaths_PublishUnZip + "/webapplication/appsettings.json"
    $appSettings = Get-Content $appsettingsfile | ConvertFrom-Json
    $appSettings.ApplicationOptions.UseMSI = $true
    $appSettings.ApplicationOptions.AdsGoFastTaskMetaDataDatabaseServer = "$env:AdsOpts_CD_Services_AzureSQLServer_Name.database.windows.net"
    $appSettings.ApplicationOptions.AdsGoFastTaskMetaDataDatabaseName = $env:AdsOpts_CD_Services_AzureSQLServer_AdsGoFastDB_Name
    $appSettings.AzureAdAuth.Domain=$env:AdsOpts_CD_ResourceGroup_Domain
    $appSettings.AzureAdAuth.TenantId=$env:AdsOpts_CD_ResourceGroup_TenantId
    $appSettings.AzureAdAuth.ClientId= $env:AdsOpts_CD_ServicePrincipals_WebAppAuthenticationSP_ClientId
    $appSettings | ConvertTo-Json  -Depth 10 | set-content $appsettingsfile

    #Repack WebApp
    $CurrentPath = Get-Location
    Set-Location "./../bin/publish"
    $Path = (Get-Location).Path + "/zipped/webapplication" 
    New-Item -ItemType Directory -Force -Path $Path
    $Path = $Path + "/Publish.zip"
    Compress-Archive -Path '.\unzipped\webapplication\*' -DestinationPath $Path -force
    #Move back to workflows 
    Set-Location $CurrentPath
    
    # Deploy Web App
    az webapp deployment source config-zip --resource-group $env:AdsOpts_CD_ResourceGroup_Name --name $env:AdsOpts_CD_Services_WebSite_Name --src $SourceFile

}
else 
{
    Write-Host "Skipped Configuring Web App"
}