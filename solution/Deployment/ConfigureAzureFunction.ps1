function ConfigureAzureFunction 
{
    if($env:AzureFunctionPublish -eq $true)
    {
        Write-Host  "Publishing Azure Function from source code"
        az functionapp deployment source config-zip -g $env:ResourceGroupName -n $env:AzureFunction --src "$env:RootFolder/azurefunction/Publish.zip" --subscription $env:Subscription
    }

    if($env:AzureFunctionGenerateFunctionKeys -eq $true)
    {
        Write-Host "Setting Function App Keys"
        $HostKeyValue = az functionapp keys set -g $env:ResourceGroupName -n $env:AzureFunction --key-type functionKeys --key-name ADFHostKey01 --query value --out tsv
        az keyvault secret set --name "AFKeyADFHostKey01" --vault-name $env:KeyVaultName --disabled false --subscription $env:SubcriptionId --value $HostKeyValue
    }

    if($env:AzureFunctionSetAppSettings -eq $true)
    {
        # Get Objects Id for the AppSetting configuration
        $AppInsightsWorkspaceId = az monitor app-insights component show --resource-group $env:ResourceGroupName --app $env:AppInsights --query appId --out tsv
        
        # Azure Functions - Apps Settings
        $functionsettings = New-Object System.Collections.Generic.Dictionary"[String,String]"
        $functionsettings.Add("AdsGoFastTaskMetaDataDatabaseName",$env:adsGoFastDB)
        $functionsettings.Add("AdsGoFastTaskMetaDataDatabaseServer","$env:SQLServer.database.windows.net")
        $functionsettings.Add("AzureFunctionURL","https://$env:AzureFunction.azurewebsites.net")
        $functionsettings.Add("AzureWebJobs.PrepareFrameworkTasksTimerTrigger.Disabled","0")
        $functionsettings.Add("AzureWebJobs.RunFrameworkTasksTimerTrigger.Disabled","0")
        $functionsettings.Add("EnablePrepareFrameworkTasks","True")
        $functionsettings.Add("EnableRunFrameworkTasks","True")
        $functionsettings.Add("FrameworkNumberOfRetries","3")
        $functionsettings.Add("FrameworkWideMaxConcurrency","100")
        $functionsettings.Add("GetSASUriSendEmailHttpTriggerAzureFunctionKey","")
        $functionsettings.Add("LogAnalyticsWorkspaceId",$env:LogAnalyticsId)
        $functionsettings.Add("RunFrameworkTasksHttpTriggerAzureFunctionKey","")
        $functionsettings.Add("SENDGRID_APIKEY","")
        $functionsettings.Add("SQLTemplateLocation","D:\home\site\wwwroot\SqlTemplates\")
        $functionsettings.Add("TaskMetaDataStorageAccount",$env:ADLSStorageAccount)
        $functionsettings.Add("TaskMetaDataStorageContainer","taskmetadata")
        $functionsettings.Add("TaskMetaDataStorageFolder","")
        $functionsettings.Add("TenantId",$env:tenantId)
        $functionsettings.Add("UseMSI","True")
        $functionsettings.Add("EnableGetADFStats","True")
        $functionsettings.Add("KQLTemplateLocation","D:\home\site\wwwroot\KqlTemplates\")
        $functionsettings.Add("HTMLTemplateLocation","D:\home\site\wwwroot\HTMLEmailTemplates\")
        $functionsettings.Add("AZStorageCacheFileListHttpTriggerAzureFunctionKey",$env:AZStorageCacheFileListHttpTriggerAzureFunctionKey)
        $functionsettings.Add("DefaultSentFromEmailAddress","noreply@test.com")
        $functionsettings.Add("DefaultSentFromEmailName","ADS Go Fast (No Reply)")
        $functionsettings.Add("GenerateTaskObjectTestFiles","False")
        $functionsettings.Add("TaskObjectTestFileLocation","D:\home\site\wwwroot\UnitTestResults\")
        $functionsettings.Add("EnableGetActivityLevelLogs","True")
        $functionsettings.Add("GetADFActivityErrors","True")
        $functionsettings.Add("AppInsightsWorkspaceId",$env:AppInsightsWorkspaceId)

        foreach ($s in $functionsettings.GetEnumerator()) {
            $key = $s.Key
            $value = $s.Value
            az functionapp config appsettings set --name $env:AzureFunction --resource-group $env:ResourceGroupName --settings "$key=$value" 
        }
    }
    
    if ($env:AzureFunctionEnableAADAuth -eq $true) 
    { 
        $Stop=Read-Host "Manual Step - Configure your new Azure Function ($env:AzureFunction) Authentication. In Azure Function go to 'Authentication / Authorization', enable 'App Service Authentication', in AAD select 'Express' and Select 'Create New AD App', Click Ok and Save!"
    }

    if($env:AzureFunctionCreateRole -eq $true)
    {
        Import-Module AzureADPreview
        # Create new App Role object for Service Principal created for AF
        $app = Get-AzureADApplication -Filter "Displayname eq '$env:AzureFunction'"  
        $appRoles = $app.AppRoles
        $newRole = CreateAppRole -RoleName "All" -Description "This is the Role to be used in Azure Functions" -RoleValue 'All'
        $appRoles.Add($newRole)
        Set-AzureADApplication -ObjectId $app.ObjectId -AppRoles $appRoles
        
        $sps = (Get-AzureADServicePrincipal -SearchString $env:AzureFunction)
        #Get the Object ID of MSI of Azure Function
        foreach($x in $sps)
        {
            if($app.AppId -eq $x.AppId)
            {
                $ServicePrincipalAF = $x
            }
            else 
            {
                $managedIdentityObjectId = $x.ObjectId
            }
        }

        #Get Role ID of role with value 'All' from the service principal !! Assumes only one AppRole
        $appRoleValue = 'All'
        $str = $ServicePrincipalAF.AppRoles[0]
        foreach($a in $splitstr)
        {
            $appRoleId =  $str.Substring($str.IndexOf("Id:")+4,36)
        }
        # Assign the Function App managed identity access to the app role.
        New-AzureADServiceAppRoleAssignment -ObjectId $managedIdentityObjectId `
        -Id $appRoleId -PrincipalId $managedIdentityObjectId `
        -ResourceId $ServicePrincipalAF.ObjectId
    }


}