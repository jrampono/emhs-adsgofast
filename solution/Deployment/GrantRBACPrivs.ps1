function GrantRBACPrivs
{
    #RBAC Rights
    # MSI Access from Service Principal to ADF
    if ($env:RBACSPToADF -eq $true) 
    {  
        $ServicePrincipalId = az ad sp list --display-name $ServicePrincipalName --output tsv --query "[].{id:objectId}" 
        az role assignment create --assignee $ServicePrincipalId --role "Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.DataFactory/factories/$DataFactoryName"
    }

    if ($env:RBACAFToADF -eq $true) 
    { 
        # MSI Access from Azure Function to ADF
        $AzureFunctionId = az webapp identity assign --resource-group $ResourceGroupName --name $AzureFunction --query principalId --out tsv
        az role assignment create --assignee $AzureFunctionId --role "Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.DataFactory/factories/$DataFactoryName"
    }

    if ($env:RBACAFToLogAnalytics -eq $true) 
    { 
        # MSI Access from Azure Function to ADF Log Analytics
        az role assignment create --assignee $AzureFunctionId --role "Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/microsoft.operationalinsights/workspaces/$LogAnalytics"
    }

    if ($env:RBACAFToADLSGen2 -eq $true) 
    { 
        # MSI Access from AF to ADLS Gen2
        az role assignment create --assignee $AzureFunctionId --role "Storage Blob Data Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"
    }

    if ($env:RBACADFToADLSGen2 -eq $true) 
    { 
        # MSI Access from ADF to ADLS Gen2
        $DataFactoryId = az ad sp list --display-name $DataFactoryName --output tsv --query "[].{id:objectId}" 
        az role assignment create --assignee $DataFactoryId --role "Storage Blob Data Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"
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
        az role assignment create --assignee $WebAppID --role "Storage Blob Data Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"
    }

    if ($env:RBACWebAppToLogAnalytics -eq $true) 
    { 
        # MSI Access from WebApp to ADF Log Analytics
        az role assignment create --assignee $WebAppID --role "Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/microsoft.operationalinsights/workspaces/$LogAnalytics"
    }

    if ($env:RBACAADUserToADLSGen2 -eq $true) 
    { 
        # AAD User Access to ADLS Gen2 - to upload sample data files
        az role assignment create --assignee $AADUserId --role "Storage Blob Data Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"
        az role assignment create --assignee $AADUserId --role "Owner" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"
    }
}