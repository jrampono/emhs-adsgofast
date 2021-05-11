
if ($env:AdsOpts_CD_Services_CoreFunctionApp_Enable -eq "True")
{
    #App Service (Includes both functions and web)
    $storageaccountkey = (az storage account keys list -g $env:AdsOpts_CD_ResourceGroup_Name -n $env:AdsOpts_CD_Services_Storage_Logging_Name | ConvertFrom-Json)[0].value

    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/AppService.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location azure-function-site-name=$env:AdsOpts_CD_Services_CoreFunctionApp_Name resource-group-name=$env:AdsOpts_CD_ResourceGroup_Name app-insights-name=$env:AdsOpts_CD_Services_AppInsights_Name sites_AdsGoFastWebApp_name=$env:AdsOpts_CD_Services_WebSite_Name storage-log-account-name=$env:AdsOpts_CD_Services_Storage_Logging_Name storage-log-account-key=$storageaccountkey
}
else 
{
    Write-Host "Skipped Creation of App Service"
}
