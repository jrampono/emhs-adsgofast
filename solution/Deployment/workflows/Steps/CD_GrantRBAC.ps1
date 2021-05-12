#function GrantRBACPrivs
#{
    $subid = (az account show -s $env:AdsOpts_CD_ResourceGroup_Subscription | ConvertFrom-Json).id
    $basescope = "/subscriptions/$subid/resourceGroups/$env:AdsOpts_CD_ResourceGroup_Name/providers"
    #RBAC Rights
    # MSI Access from Service Principal to ADF
    if ($env:RBACSPToADF -eq $true) 
    {  
        $ServicePrincipalId = az ad sp list --display-name $ServicePrincipalName --output tsv --query "[].{id:objectId}" 
        az role assignment create --assignee $ServicePrincipalId --role "Contributor" --scope "$basescope/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name"
    }

    if ($env:RBACAFToADF -eq $true) 
    { 
        # MSI Access from Azure Function to ADF
        $AzureFunctionId = az webapp identity assign --resource-group $ResourceGroupName --name $AzureFunction --query principalId --out tsv
        az role assignment create --assignee $AzureFunctionId --role "Contributor" --scope "$basescope/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name"
    }

    if ($env:RBACAFToLogAnalytics -eq $true) 
    { 
        # MSI Access from Azure Function to ADF Log Analytics
        az role assignment create --assignee $AzureFunctionId --role "Contributor" --scope "$basescope/microsoft.operationalinsights/workspaces/$env:AdsOpts_CD_Services_Storage_Logging_Name"
    }

    if ($env:RBACAFToADLSGen2 -eq $true) 
    { 
        # MSI Access from AF to ADLS Gen2
        az role assignment create --assignee $AzureFunctionId --role "Storage Blob Data Contributor" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
    }

    if ($env:RBACADFToADLSGen2 -eq $true) 
    { 
        # MSI Access from ADF to ADLS Gen2
        $DataFactoryId = az ad sp list --display-name $DataFactoryName --output tsv --query "[].{id:objectId}" 
        az role assignment create --assignee $DataFactoryId --role "Storage Blob Data Contributor" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
    }

    if ($env:RBACADFToKeyVault -eq $true) 
    { 
        # MSI Access from ADF to KeyVault
        az keyvault set-policy --name $KeyVaultName --object-id $DataFactoryId --certificate-permissions get list --key-permissions get list --resource-group $ResourceGroupName --secret-permissions get list --subscription $SubcriptionId
    }

    if ($env:RBACWebAppToADLSGen2 -eq $true) 
    { 
        # MSI Access from WebApp to ADLS Gen2
        $WebAppID = az webapp identity assign --resource-group $ResourceGroupName --name $WebApp --query principalId --out tsv  
        az role assignment create --assignee $WebAppID --role "Storage Blob Data Contributor" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
    }

    if ($env:RBACWebAppToLogAnalytics -eq $true) 
    { 
        # MSI Access from WebApp to ADF Log Analytics
        az role assignment create --assignee $WebAppID --role "Contributor" --scope "$basescope/microsoft.operationalinsights/workspaces/$env:AdsOpts_CD_Services_Storage_Logging_Name"
    }

    if ($env:RBACAADUserToADLSGen2 -eq $true) 
    { 
        # AAD User Access to ADLS Gen2 - to upload sample data files
        az role assignment create --assignee $AADUserId --role "Storage Blob Data Contributor" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
        az role assignment create --assignee $AADUserId --role "Owner" --scope "$basescope/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_ADLS_Name"
    }
#}