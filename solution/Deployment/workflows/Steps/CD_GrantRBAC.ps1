    $subid = (az account show -s $env:AdsOpts_CD_ResourceGroup_Subscription | ConvertFrom-Json).id
    $basescope = "/subscriptions/$subid/resourceGroups/$env:AdsOpts_CD_ResourceGroup_Name/providers"
    $DataFactoryId = az ad sp list --display-name $env:AdsOpts_CD_Services_DataFactory_Name --output tsv --query "[].{id:objectId}"
    $AzureFunctionId = ((az webapp identity show --resource-group $env:AdsOpts_CD_ResourceGroup_Name --name $env:AdsOpts_CD_Services_CoreFunctionApp_Name) | ConvertFrom-Json).principalId
    $WebAppID = ((az webapp identity show --resource-group  $env:AdsOpts_CD_ResourceGroup_Name --name $env:AdsOpts_CD_Services_WebSite_Name) | ConvertFrom-Json).principalId 
    $AADUserId = (az ad signed-in-user show | ConvertFrom-Json).objectId
    
    #RBAC Rights
    # MSI Access from Azure Function to ADF
    az role assignment create --assignee $AzureFunctionId --role "Contributor" --scope "$basescope/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name"

    # MSI Access from Azure Function to ADF Log Analytics
    az role assignment create --assignee $AzureFunctionId --role "Contributor" --scope "$basescope/microsoft.operationalinsights/workspaces/$env:AdsOpts_CD_Services_LogAnalytics_Name"
    # MSI Access from AF to ADLS Gen2
    az role assignment create --assignee $AzureFunctionId --role "Storage Blob Data Contributor" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
    # MSI Access from AF to Blob Gen2
    az role assignment create --assignee $AzureFunctionId --role "Storage Blob Data Contributor" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
    
    # MSI Access from ADF to ADLS Gen2
    az role assignment create --assignee $DataFactoryId --role "Storage Blob Data Contributor" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
    # MSI Access from ADF to KeyVault
    az keyvault set-policy --name $env:AdsOpts_CD_Services_KeyVault_Name --object-id $DataFactoryId --certificate-permissions get list --key-permissions get list --resource-group $env:AdsOpts_CD_ResourceGroup_Name --secret-permissions get list --subscription $subid
    # MSI Access from WebApp to ADLS Gen2 
    az role assignment create --assignee $WebAppID --role "Storage Blob Data Contributor" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
    # MSI Access from WebApp to ADF Log Analytics
    az role assignment create --assignee $WebAppID --role "Contributor" --scope "$basescope/microsoft.operationalinsights/workspaces/$env:AdsOpts_CD_Services_LogAnalytics_Name"
    # MSI Access from WebApp to ADF App Insights
    az role assignment create --assignee $WebAppID --role "Contributor" --scope "$basescope/microsoft.insights/components/$env:AdsOpts_CD_Services_AppInsights_Name"
    # AAD User Access to ADLS Gen2 - to upload sample data files
    az role assignment create --assignee $AADUserId --role "Storage Blob Data Contributor" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
    az role assignment create --assignee $AADUserId --role "Owner" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
  