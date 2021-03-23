#############################################################
# Installation Requirements
#############################################################
$RootFolder=$PSScriptRoot
#$RootFolder="." #Manual Testing Only
. "$RootFolder/Functions.ps1"

#############################################################
# User Entered Variables
#############################################################
$Subcription=Read-Host "Please enter Subscription Name"
$ResourceGroupName=Read-Host "Please enter a new Resource Group Name"
$AADUser=Read-Host "Please enter ADD User Name (xxxx@company.xxxxx)"
$ServicePrincipalName=Read-Host "Please enter Service Principal Name for Azure Function"
$ServicePrincipalNameWeb=Read-Host "Please enter Service Principal Name for Azure Web App"
$Location=Read-Host "Please enter Azure DC Location"

#############################################################
# Pre-Defined Variables
#############################################################
$TemplateFile = "$RootFolder/azuredeploy.json"
$TemplateParametersFile = "$RootFolder/azuredeploy.parameters.json"
$ADFIRDownloadURL='https://download.microsoft.com/download/E/4/7/E4771905-1079-445B-8BF9-8A1A075D8A10/IntegrationRuntime_5.3.7720.1.msi'
$ADFLocalVMFolder='Temp'
$ADFIRLocalFileLocation="C:\$ADFLocalVMFolder\IntegrationRuntime_5.3.7720.1.msi"
$ADFIRInstallerDownloadURL='https://raw.githubusercontent.com/nabhishek/SelfHosted-IntegrationRuntime_AutomationScripts/master/InstallGatewayOnLocalMachine.ps1'
$ADFIRInstallerLocalFileLocation="C:\$ADFLocalVMFolder\InstallGatewayOnLocalMachine.ps1"
$ADFJDKDownloadURL='https://github.com/AdoptOpenJDK/openjdk15-binaries/releases/download/jdk-15.0.2%2B7/OpenJDK15U-jre_x64_windows_hotspot_15.0.2_7.msi'
$ADFJDKInstallerLocationFile="C:\$ADFLocalVMFolder\OpenJDK15U-jre_x64_windows_hotspot_15.0.2_7.msi"
$webappZipFile="$RootFolder/webapp/Publish.zip"
$ServicePrincipalNameWebWithHttp="http://$ServicePrincipalNameWeb"
$AppSettingsFile="$RootFolder\SampleAppSettings.json"

#############################################################
# Sign-In to Azure
#############################################################
log("Sign-In Az Login")
Az Login
az account set --subscription $Subcription

$tenantId = az account show --subscription $Subcription --query tenantId --output tsv
$SubcriptionId=az account show --subscription $Subcription --query id --output tsv

log("Sign-In Connect-AzAccount")
Connect-AzAccount
Select-AzSubscription -SubscriptionId $SubcriptionId

log("Sign-In Connect-AzureAD")
Connect-AzureAD -TenantId $tenantId

log("Sign-In Completed")
#############################################################
# Create Resource Group and Deploy ARM Template
#############################################################
log("Creating Resource Group")
az group create --name $ResourceGroupName --location $Location
log("Deploying ARM Template")
$output = az deployment group create --name "AdsGoFastDeployment" --resource-group $ResourceGroupName --template-file $TemplateFile --parameters $TemplateParametersFile
#$output = az deployment group show --name "AdsGoFastDeployment" --resource-group $ResourceGroupName 

#############################################################
# Load Variables from ARM Template OutPut and Parameters
#############################################################
log("Loading ARM Template Outputs and Parameters")
$deploymentParameters = Get-Content $TemplateParametersFile | Out-String | ConvertFrom-Json
$deployment = $output | ConvertFrom-Json

log("Loading Variables")
$DataFactoryName=$deployment.properties.outputs.stringDataFactoryName.value
$LogStorageAccount=$deployment.properties.outputs.stringLogStorageAccount.value
$LogAnalytics=$deployment.properties.outputs.stringLogAnalytics.value
$AppInsights=$deployment.properties.outputs.stringAppInsights.value
$ADLSStorageAccount=$deployment.properties.outputs.stringADLSStorageAccount.value
$SQLServer=$deployment.properties.outputs.stringSQLServer.value
$SQLUser=$deploymentParameters.parameters.'sql-admin-login'.value
$SQLPwd=$deploymentParameters.parameters.'sql-admin-password'.value
$adsGoFastDB = $deployment.properties.outputs.stringadsGoFastDB.value
$stagingDB = $deployment.properties.outputs.stringstagingDB.value
$sampleDB = $deployment.properties.outputs.stringsampleDB.value
$AzureFunction = $deployment.properties.outputs.stringAzureFunction.value
$KeyVaultName=$deployment.properties.outputs.stringKeyVaultName.value
$VMAzIR = $deployment.properties.outputs.stringVMAzIR.value
$VMOnPIR = $deploymentParameters.parameters.'adf-ir-onp-vm-name'.value
$WebApp = $deployment.properties.outputs.stringWebApp.value

#############################################################
# MSI Access
#############################################################
log("Creating MSI Access")

#Create Managed Identity Access for Azure Functions and Web App
az webapp identity assign --resource-group $ResourceGroupName --name $AzureFunction
az webapp identity assign --resource-group $ResourceGroupName --name $WebApp

# Add AAD Group/User to Azure SQL Logical Server
$AADUserId = az ad user show --id $AADUser --query objectId --out tsv
az sql server ad-admin create --display-name $AADUser --object-id $AADUserId --resource-group $ResourceGroupName --server $SQLServer --subscription $SubcriptionId

#Add Ip to SQL Firewall
$myIp = (Invoke-WebRequest ifconfig.me/ip).Content
az sql server firewall-rule create -g $ResourceGroupName -s $SQLServer -n "MySetupIP" --start-ip-address $myIp --end-ip-address $myIp
#Allow Azure services and resources to access this server
az sql server firewall-rule create -g $ResourceGroupName -s $SQLServer -n "Azure" --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0

#Create Service Principal
$ServicePrincipalPassword = az ad sp create-for-rbac --name $ServicePrincipalName --query password --out tsv

#Connect Azure SQL DB 
Install-Module -Name SqlServer
$token=$(az account get-access-token --resource=https://database.windows.net --query accessToken --output tsv)

# MSI Access Service Principal on Azure SQL AdsGoFast DB
Invoke-Sqlcmd -ServerInstance "$SQLServer.database.windows.net,1433" -Database $adsGoFastDB -AccessToken $token -query "
CREATE USER [$ServicePrincipalName] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [$ServicePrincipalName];
ALTER ROLE db_datawriter ADD MEMBER [$ServicePrincipalName];
ALTER ROLE db_ddladmin ADD MEMBER [$ServicePrincipalName];
GRANT EXECUTE ON SCHEMA::[dbo] TO [$ServicePrincipalName];
GO"

# MSI Access Azure Function on Azure SQL AdsGoFast DB
Invoke-Sqlcmd -ServerInstance "$SQLServer.database.windows.net,1433" -Database $adsGoFastDB -AccessToken $token -query "
CREATE USER [$AzureFunction] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [$AzureFunction];
ALTER ROLE db_datawriter ADD MEMBER [$AzureFunction];
ALTER ROLE db_ddladmin ADD MEMBER [$AzureFunction];
GRANT EXECUTE ON SCHEMA::[dbo] TO [$AzureFunction];
GO"

# MSI Access Azure Data Factory on Azure SQL Staging DB
Invoke-Sqlcmd -ServerInstance "$SQLServer.database.windows.net,1433" -Database $stagingDB -AccessToken $token -query "
CREATE USER [$DataFactoryName] FROM EXTERNAL PROVIDER;
ALTER ROLE db_owner ADD MEMBER [$DataFactoryName];
GO"

# MSI Access WebApp on Azure SQL ADSGoFast DB
Invoke-Sqlcmd -ServerInstance "$SQLServer.database.windows.net,1433" -Database $adsGoFastDB -AccessToken $token -query "
CREATE USER [$WebApp] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [$WebApp];
ALTER ROLE db_datawriter ADD MEMBER [$WebApp];
ALTER ROLE db_ddladmin ADD MEMBER [$WebApp];
GO"

# MSI Access Azure Data Factory on Azure SQL AdventureWorld DB
Invoke-Sqlcmd -ServerInstance "$SQLServer.database.windows.net,1433" -Database $sampleDB -AccessToken $token -query "
CREATE USER [$DataFactoryName] FROM EXTERNAL PROVIDER;
ALTER ROLE db_datareader ADD MEMBER [$DataFactoryName];
ALTER ROLE db_datawriter ADD MEMBER [$DataFactoryName];
ALTER ROLE db_ddladmin ADD MEMBER [$DataFactoryName];
GO"

# MSI Access from Service Principal to ADF
$ServicePrincipalId = az ad sp list --display-name $ServicePrincipalName --output tsv --query "[].{id:objectId}" 
az role assignment create --assignee $ServicePrincipalId --role "Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.DataFactory/factories/$DataFactoryName"

# MSI Access from Azure Function to ADF
$AzureFunctionId = az webapp identity assign --resource-group $ResourceGroupName --name $AzureFunction --query principalId --out tsv
az role assignment create --assignee $AzureFunctionId --role "Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.DataFactory/factories/$DataFactoryName"

# MSI Access from Azure Function to ADF Log Analytics
az role assignment create --assignee $AzureFunctionId --role "Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/microsoft.operationalinsights/workspaces/$LogAnalytics"

# MSI Access from AF to ADLS Gen2
az role assignment create --assignee $AzureFunctionId --role "Storage Blob Data Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"

# MSI Access from ADF to ADLS Gen2
$DataFactoryId = az ad sp list --display-name $DataFactoryName --output tsv --query "[].{id:objectId}" 
az role assignment create --assignee $DataFactoryId --role "Storage Blob Data Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"

# MSI Access from ADF to KeyVault
az keyvault set-policy --name $KeyVaultName --object-id $DataFactoryId --certificate-permissions get list --key-permissions get list --resource-group $ResourceGroupName --secret-permissions get list --subscription $SubcriptionId

# MSI Access from WebApp to ADLS Gen2
$WebAppID = az webapp identity assign --resource-group $ResourceGroupName --name $WebApp --query principalId --out tsv  
az role assignment create --assignee $WebAppID --role "Storage Blob Data Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"

# MSI Access from WebApp to ADF Log Analytics
az role assignment create --assignee $WebAppID --role "Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/microsoft.operationalinsights/workspaces/$LogAnalytics"

# AAD User Access to ADLS Gen2 - to upload sample data files
az role assignment create --assignee $AADUserId --role "Storage Blob Data Contributor" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"
az role assignment create --assignee $AADUserId --role "Owner" --scope "/subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$ADLSStorageAccount"

$Stop=Read-Host "Manual Step - Configure your new Azure Function ($AzureFunction) Authentication. In Azure Function go to 'Authentication / Authorization', enable 'App Service Authentication', in AAD select 'Express' and Select 'Create New AD App', Click Ok and Save!"

#############################################################
# Azure KeyVault
#############################################################
log("Enabling Access to KeyVault and Adding Secrets")
#Set KeyVault Policy
az keyvault set-policy --name $KeyVaultName --certificate-permissions backup create delete deleteissuers get getissuers import list listissuers managecontacts manageissuers purge recover restore setissuers update --key-permissions backup create decrypt delete encrypt get import list purge recover restore sign unwrapKey update verify wrapKey --object-id $AADUserId --resource-group $ResourceGroupName --secret-permissions backup delete get list purge recover restore set --storage-permissions backup delete deletesas get getsas list listsas purge recover regeneratekey restore set setsas update --subscription $SubcriptionId

#Save Service Principal to KeyVault
az keyvault secret set --name "AdsGoFastAppId" --vault-name $KeyVaultName --disabled false --subscription $SubcriptionId --value $ServicePrincipalName
az keyvault secret set --name "AdsGoFastAppSecret" --vault-name $KeyVaultName --disabled false --subscription $SubcriptionId --value $ServicePrincipalPassword

#############################################################
# Azure SQL Configuration/Deployment
#############################################################
log("Deploying Database Objects and Data")

# Database - Objects Deployment
$LogAnalyticsId = az monitor log-analytics workspace show --resource-group $ResourceGroupName --workspace-name $LogAnalytics --query customerId --out tsv
.\sqlpackage\sqlpackage.exe /a:script /SourceFile:".\database\ADSGoFast.dacpac" /tsn:tcp:"$SQLServer.database.windows.net,1433" /tdn:$adsGoFastDB /tu:$SQLUser /tp:$SQLPwd /OutputPath:".\database\Deployment.sql"
Invoke-Sqlcmd -ServerInstance "$SQLServer.database.windows.net,1433" -Database $adsGoFastDB -AccessToken $token -InputFile "$RootFolder/database/Deployment.sql"
# Database - Post Script Deployment
$SQLPostDeploymentStringArray = "VMOnPIR='$VMOnPIR'", "ADLSStorageAccount='$ADLSStorageAccount'", "ADFName='$DataFactoryName'", "SQLServer='$SQLServer'", "sampleDB='$sampleDB'", "stagingDB='$stagingDB'", "adsGoFastDB='$adsGoFastDB'", "ResourceGroup='$ResourceGroupName'", "SubscriptionUid='$SubcriptionId'", "DefaultKeyVaultURL='https://$KeyVaultName.vault.azure.net/'", "LogAnalyticsWorkspaceId='$LogAnalyticsId'"
Invoke-Sqlcmd -ServerInstance "$SQLServer.database.windows.net,1433" -Database $adsGoFastDB -AccessToken $token -InputFile "$RootFolder/database/PostDeployment.sql" -Variable $SQLPostDeploymentStringArray
#Create Pre-Defined Tables for sample CSV, JSON and XLSX data source
Invoke-Sqlcmd -ServerInstance "$SQLServer.database.windows.net,1433" -Database $stagingDB -AccessToken $token -InputFile "$RootFolder/database/Pre-Defined Tables for sample CSV, JSON and XLSX data source.sql" 

#############################################################
# Azure Function Deployment
#############################################################
log("Deploying Azure Function")
az functionapp deployment source config-zip -g $ResourceGroupName -n $AzureFunction --src "$RootFolder/azurefunction/Publish.zip" --subscription $Subcription

log("Configuring Azure Function")
$HostKeyValue = az functionapp keys set -g $ResourceGroupName -n $AzureFunction --key-type functionKeys --key-name ADFHostKey01 --query value --out tsv
az keyvault secret set --name "AFKeyADFHostKey01" --vault-name $KeyVaultName --disabled false --subscription $SubcriptionId --value $HostKeyValue

# Get Objects Id for the AppSetting configuration
$AppInsightsWorkspaceId = az monitor app-insights component show --resource-group $ResourceGroupName --app $AppInsights --query appId --out tsv

# Azure Functions - Apps Settings
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "AdsGoFastTaskMetaDataDatabaseName=$adsGoFastDB"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "AdsGoFastTaskMetaDataDatabaseServer=$SQLServer.database.windows.net"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "AzureFunctionURL=https://$AzureFunction.azurewebsites.net"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "AzureWebJobs.PrepareFrameworkTasksTimerTrigger.Disabled=0"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "AzureWebJobs.RunFrameworkTasksTimerTrigger.Disabled=0"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "EnablePrepareFrameworkTasks=True"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "EnableRunFrameworkTasks=True"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "FrameworkNumberOfRetries=3"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "FrameworkWideMaxConcurrency=100"

az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "GetSASUriSendEmailHttpTriggerAzureFunctionKey="
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "LogAnalyticsWorkspaceId=$LogAnalyticsId"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "RunFrameworkTasksHttpTriggerAzureFunctionKey="
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "SENDGRID_APIKEY="
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "SQLTemplateLocation=D:\home\site\wwwroot\SqlTemplates\"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "TaskMetaDataStorageAccount=$ADLSStorageAccount"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "TaskMetaDataStorageContainer=taskmetadata"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "TaskMetaDataStorageFolder="
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "TenantId=$tenantId"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "UseMSI=True"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "EnableGetADFStats=True"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "KQLTemplateLocation=D:\home\site\wwwroot\KqlTemplates\"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "HTMLTemplateLocation=D:\home\site\wwwroot\HTMLEmailTemplates\"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "AZStorageCacheFileListHttpTriggerAzureFunctionKey=$AZStorageCacheFileListHttpTriggerAzureFunctionKey"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "DefaultSentFromEmailAddress=noreply@test.com"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "DefaultSentFromEmailName=ADS Go Fast (No Reply)"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "GenerateTaskObjectTestFiles=False"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "TaskObjectTestFileLocation=D:\home\site\wwwroot\UnitTestResults\"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "EnableGetActivityLevelLogs=True"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "EnableGetADFStats=True"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "GetADFActivityErrors=True"
az functionapp config appsettings set --name $AzureFunction --resource-group $ResourceGroupName --settings "AppInsightsWorkspaceId=$AppInsightsWorkspaceId"

# Create new App Role object for Service Principal created for AF
$app = Get-AzureADApplication -Filter "identifierUris/any(uri:uri eq 'https://$AzureFunction.azurewebsites.net')"
$appRoles = $app.AppRoles
$newRole = CreateAppRole -RoleName "All" -Description "This is the Role to be used in Azure Functions" -RoleValue 'All'
$appRoles.Add($newRole)
Set-AzureADApplication -ObjectId $app.ObjectId -AppRoles $appRoles

#Get the Object ID of MSI of Azure Function
$managedIdentityObjectId = (Get-AzureADServicePrincipal -SearchString $AzureFunction)[0].ObjectId

#Get the Object ID of Service principal created for AF
#This is the Object ID from Enterprise Application of AAD, not App Registrations
$ServicePrincipalAF = (Get-AzureADServicePrincipal -SearchString $AzureFunction)[1]

#Get Role ID of role with value 'All' from the service principal
$appRoleValue = 'All'
$appRoleId = ($ServicePrincipalAF.AppRoles | Where-Object {$_.Value -eq $appRoleValue }).Id

# Assign the Function App managed identity access to the app role.
New-AzureADServiceAppRoleAssignment -ObjectId $managedIdentityObjectId `
-Id $appRoleId -PrincipalId $managedIdentityObjectId `
-ResourceId $ServicePrincipalAF.ObjectId

#############################################################
# Azure Data Factory Deployment
#############################################################
log("Deploying Azure Data Factory Artifacts")

# Create ADF Diagnostic Settings
$logsSetting = "[{'category':'ActivityRuns','enabled':true,'retentionPolicy':{'days': 30,'enabled': false}},{'category':'PipelineRuns','enabled':true,'retentionPolicy':{'days': 30,'enabled': false}},{'category':'TriggerRuns','enabled':true,'retentionPolicy':{'days': 30,'enabled': false}}]".Replace("'",'\"')
$metricsSetting = "[{'category':'AllMetrics','enabled':true,'retentionPolicy':{'days': 30,'enabled': false}}]".Replace("'",'\"')
az monitor diagnostic-settings create --name ADF-Diagnostics --resource /subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.DataFactory/factories/$DataFactoryName --logs $logsSetting --metrics $metricsSetting --storage-account /subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$LogStorageAccount --workspace /subscriptions/$SubcriptionId/resourcegroups/$ResourceGroupName/providers/microsoft.operationalinsights/workspaces/$LogAnalytics

$dfbase = "$RootFolder/../solution/DataFactory"

# Data Factory - Deploy - Integration Run-Time
az extension add --name datafactory
Get-ChildItem "$dfbase/integrationRuntime" -Filter *.json | 
Foreach-Object {
    $irName = $_.BaseName

    az datafactory integration-runtime self-hosted create --factory-name $DataFactoryName --integration-runtime-name $irName --resource-group $ResourceGroupName 
}
log("Configuring Azure Data Factory IR - Azure")
# Data Factory - VM AZ IR - Download IR
az vm run-command invoke  --command-id RunPowerShellScript --name $VMAzIR -g $ResourceGroupName --scripts "New-Item -Path 'C:\' -Name $ADFLocalVMFolder -ItemType Directory"
az vm run-command invoke  --command-id RunPowerShellScript --name $VMAzIR -g $ResourceGroupName --scripts "Invoke-WebRequest -Uri $ADFIRDownloadURL -OutFile $ADFIRLocalFileLocation"
az vm run-command invoke  --command-id RunPowerShellScript --name $VMAzIR -g $ResourceGroupName --scripts "Invoke-WebRequest -Uri $ADFIRInstallerDownloadURL -OutFile $ADFIRInstallerLocalFileLocation"
az vm run-command invoke  --command-id RunPowerShellScript --name $VMAzIR -g $ResourceGroupName --scripts "Invoke-WebRequest -Uri $ADFJDKDownloadURL -OutFile $ADFJDKInstallerLocationFile"
az vm run-command invoke  --command-id RunPowerShellScript --name $VMAzIR -g $ResourceGroupName --scripts "msiexec /i $ADFJDKInstallerLocationFile ADDLOCAL=FeatureMain,FeatureEnvironment,FeatureJarFileRunWith,FeatureJavaHome /quiet"

# Data Factory - VM AZ IR - Install IR
$irKey1 = az datafactory integration-runtime list-auth-key --factory-name $DataFactoryName --name "SelfHostedIntegrationRuntime-Azure-VNET" --resource-group $ResourceGroupName --query authKey1 --out tsv
az vm run-command invoke  --command-id RunPowerShellScript --name $VMAzIR -g $ResourceGroupName --scripts "$ADFIRInstallerLocalFileLocation -path $ADFIRLocalFileLocation -authKey '$irKey1'"

log("Configuring Azure Data Factory IR - OnPrem")
# Data Factory - VM OnPrem IR - Download IR
az vm run-command invoke  --command-id RunPowerShellScript --name $VMOnPIR -g $ResourceGroupName --scripts "New-Item -Path 'C:\' -Name $ADFLocalVMFolder -ItemType Directory"
az vm run-command invoke  --command-id RunPowerShellScript --name $VMOnPIR -g $ResourceGroupName --scripts "Invoke-WebRequest -Uri $ADFIRDownloadURL -OutFile $ADFIRLocalFileLocation"
az vm run-command invoke  --command-id RunPowerShellScript --name $VMOnPIR -g $ResourceGroupName --scripts "Invoke-WebRequest -Uri $ADFIRInstallerDownloadURL -OutFile $ADFIRInstallerLocalFileLocation"
az vm run-command invoke  --command-id RunPowerShellScript --name $VMOnPIR -g $ResourceGroupName --scripts "Invoke-WebRequest -Uri $ADFJDKDownloadURL -OutFile $ADFJDKInstallerLocationFile"
az vm run-command invoke  --command-id RunPowerShellScript --name $VMOnPIR -g $ResourceGroupName --scripts "msiexec /i $ADFJDKInstallerLocationFile ADDLOCAL=FeatureMain,FeatureEnvironment,FeatureJarFileRunWith,FeatureJavaHome /quiet"
# Data Factory - VM OnPrem IR - Install IR
$irKey1 = az datafactory integration-runtime list-auth-key --factory-name $DataFactoryName --name "SelfHostedIntegrationRuntime-OnPem-Net" --resource-group $ResourceGroupName --query authKey1 --out tsv
az vm run-command invoke  --command-id RunPowerShellScript --name $VMOnPIR -g $ResourceGroupName --scripts "$ADFIRInstallerLocalFileLocation -path $ADFIRLocalFileLocation -authKey '$irKey1'"

# Update Azure Function Linked Service with the correct URL and Credential
$AzureFunctionKeys = az functionapp keys list -g $ResourceGroupName -n $AzureFunction
$AzureFunctionKeys = $AzureFunctionKeys | ConvertFrom-Json

$pathToJson = "$dfbase/linkedService/AzureFunctionAdsGoFastDataLakeAccelFunApp.json"
$AzureFunctionAdsGoFastDataLakeAccelFunApp = Get-Content $pathToJson | ConvertFrom-Json
$AzureFunctionAdsGoFastDataLakeAccelFunApp.properties.typeProperties.functionAppUrl = "https://$AzureFunction.azurewebsites.net"
$AzureFunctionAdsGoFastDataLakeAccelFunApp.properties.typeProperties.encryptedCredential = $AzureFunctionKeys.masterKey
$AzureFunctionAdsGoFastDataLakeAccelFunApp | ConvertTo-Json | set-content $pathToJson

#Data Factory - LinkedServices
Get-ChildItem "$dfbase/linkedService" -Filter *.json | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName
    write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
    Set-AzDataFactoryV2LinkedService -DataFactoryName $DataFactoryName -ResourceGroupName $ResourceGroupName -Name $lsName -DefinitionFile $fileName
}

#Data Factory - Dataset
Get-ChildItem "$dfbase/dataset" -Filter *.json | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName
    write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
    Set-AzDataFactoryV2Dataset -DataFactoryName $DataFactoryName -ResourceGroupName $ResourceGroupName -Name $lsName -DefinitionFile $fileName
}

#Data Factory - Pipeline
Get-ChildItem "$dfbase/pipeline" -Recurse -Include "AZ-Function*.json", "OnP-SQL-Watermark-OnP-SH-IR.json", "AZ-SQL-Watermark-SH-IR.json" | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName
    write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
    Set-AzDataFactoryV2Pipeline -DataFactoryName $DataFactoryName -ResourceGroupName $ResourceGroupName -Name $lsName -DefinitionFile $fileName
}
Get-ChildItem "$dfbase/pipeline" -Recurse -Include "az-sql-full-load-sh-ir.json", "sh-sql-full-load-sh-ir.json", "onp-sql-full-load-onp-sh-ir.json", "sh-sql-watermark-sh-ir.json" | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName
    write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
    Set-AzDataFactoryV2Pipeline -DataFactoryName $DataFactoryName -ResourceGroupName $ResourceGroupName -Name $lsName -DefinitionFile $fileName
}

Get-ChildItem "$dfbase/pipeline" -Filter *chunk*.json | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName
    write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
    Set-AzDataFactoryV2Pipeline -DataFactoryName $DataFactoryName -ResourceGroupName $ResourceGroupName -Name $lsName -DefinitionFile $fileName
}
Get-ChildItem "$dfbase/pipeline" -Exclude "Master.json","AZ-Function*.json", "OnP-SQL-Watermark-OnP-SH-IR.json", "AZ-SQL-Watermark-SH-IR.json", "*chunk*.json", "az-sql-full-load-sh-ir.json", "sh-sql-full-load-sh-ir.json", "onp-sql-full-load-onp-sh-ir.json", "sh-sql-watermark-sh-ir.json" | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName
    write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
    Set-AzDataFactoryV2Pipeline -DataFactoryName $DataFactoryName -ResourceGroupName $ResourceGroupName -Name $lsName -DefinitionFile $fileName
}
Get-ChildItem "$dfbase/pipeline" -Filter Master.json | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName
    write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
    Set-AzDataFactoryV2Pipeline -DataFactoryName $DataFactoryName -ResourceGroupName $ResourceGroupName -Name $lsName -DefinitionFile $fileName
}

#############################################################
# Azure Web App Deployment
#############################################################
log("Deploying Azure Web App")

# Deploy Web App
az webapp deployment source config-zip --resource-group $ResourceGroupName --name $WebApp --src $webappZipFile

#Create Service Principal for Web App to be used in AAD Authentication
$ServicePrincipalPasswordWeb = az ad sp create-for-rbac --name $ServicePrincipalNameWebWithHttp --role Contributor --query password --out tsv

#Add Reply Web URI to the service principal
Import-Module Az.Resources
$ServicePrincipalAppIdWeb = (Get-AzADApplication -DisplayName $ServicePrincipalNameWeb).ObjectId
Set-AzureADApplication -ObjectId $ServicePrincipalAppIdWeb -ReplyUrls "https://$WebApp.azurewebsites.net/.auth/login/aad/callback"

#Save Service Principal to KeyVault
az keyvault secret set --name "AppNameForWebApp" --vault-name $KeyVaultName --disabled false --subscription $SubcriptionId --value $ServicePrincipalNameWeb
az keyvault secret set --name "AppSecretForWebApp" --vault-name $KeyVaultName --disabled false --subscription $SubcriptionId --value $ServicePrincipalPasswordWeb

#Enable App Service Authetication on Web App and Set AAD providers to use a service principal
$ServicePrincipalAppIdWeb = (Get-AzADServicePrincipal -ServicePrincipalName $ServicePrincipalNameWebWithHttp).ApplicationId
az webapp auth update  -g "$ResourceGroupName" -n "$WebApp" --enabled true `
  --action LoginWithAzureActiveDirectory `
  --aad-allowed-token-audiences https://$WebApp.azurewebsites.net/.auth/login/aad/callback `
  --aad-client-id $ServicePrincipalAppIdWeb `
  --aad-token-issuer-url https://sts.windows.net/$tenantId/ `
  --token-store true

$Stop=Read-Host "Press Enter to Generate AppSettings and Update the Web AppSettings.json on Azure Web App"

# Provide AppSettings.json output
$appSettings = Get-Content $AppSettingsFile | ConvertFrom-Json
$appSettings.UseMSI = "true"
$appSettings.AdsGoFastTaskMetaDataDatabaseServer = "$SQLServer.database.windows.net"
$appSettings.AdsGoFastTaskMetaDataDatabaseName = $adsGoFastDB
$appSettings.AzureAd.Domain= Read-Host "Please enter your Domain name, such as Contoso.com"
$appSettings.AzureAd.TenantId= $tenantId
$appSettings.AzureAd.ClientId= $ServicePrincipalAppIdWeb
$appSettings | ConvertTo-Json | set-content $AppSettingsFile

log("Open file $AppSettingsFile and copy the content to your Azure Web App appsettings.json")

#############################################################
# Azure Storage Upload Sample data CSV, JSON and XLSX
#############################################################
log("Upload Sample XLSX, JSON and CSV to ADLS")

./azcopy/azcopy.exe login

$destinationStorage="https://$ADLSStorageAccount.dfs.core.windows.net/datalakeraw"
$sourceFolder="$RootFolder/sample-data/Gov"
./azcopy/azcopy.exe copy $sourceFolder $destinationStorage --recursive

$destinationStorage="https://$ADLSStorageAccount.dfs.core.windows.net/datalakeraw"
$sourceFolder="$RootFolder/sample-data/NYTaxi"
./azcopy/azcopy.exe copy $sourceFolder $destinationStorage --recursive

log("Deployment Complete")