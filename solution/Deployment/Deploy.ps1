$env:RootFolder=$PSScriptRoot

#############################################################
# Switch between local development and DevOps Deployment
#############################################################
$LocalDev=$true
if($LocalDev -eq $true)
{
    $env:RootFolder="." #Manual Testing Only
    . "$env:RootFolder/LoadDevelopmentSettings.ps1"
    LoadDevelopmentSettings
}

#Folder into which you have extracted the binaries from the release 
$env:PublishAssetsFolder = $env:RootFolder+"\..\..\deploy"

#############################################################
# Do Local Prereq Installs
#############################################################
if($env:PerformLocalInstalls -eq $true)
{
    Write-Host "Performing Pre-req Installs"
    . "$env:RootFolder/LocalInstalls.ps1"
    LocalInstalls
}

#############################################################
# Local Script Files
#############################################################
. "$env:RootFolder/Functions.ps1"


#############################################################
# Pre-Defined Variables
#############################################################
. "$env:RootFolder/SetPredefinedVariables.ps1"
SetPredefinedVariables


#############################################################
# Sign-In to Azure 
#############################################################

#Azure CLI 
SignInToAzureCli

#Azure Powershell
SignInToPowershellAz

#AzureAD Powershell !! Windows Only and Powershell 5.1 only - Runs in compat mode for Powershell 7
SignInToAzureAD

#############################################################
# Create Resource Group and Deployment SP
#############################################################
if($env:CreateResourceGroup -eq $true)
{
    Write-Host "Creating Resource Group"
    az group create --name $env:ResourceGroupName --location $env:Location
}

$env:SubcriptionId = (az account show -s "Jorampon Internal Consumption" | ConvertFrom-Json).id

if($env:CreateDeploymentSP -eq $true)
{
    Write-Host "Creating Deployment Service Principal"
    az ad sp create-for-rbac --name $env:ServicePrincipal_Deployment --role contributor --scopes /subscriptions/$env:SubcriptionId/resourceGroups/$env:ResourceGroupName
}

$env:ResourceGroupHash = Get-UniqueString ($env:SubcriptionId, $length=13)

#############################################################
# Deploy ARM Template
#############################################################


az deployment group create --name "AdsGoFastDeployment" --resource-group $env:ResourceGroupName --template-file $env:TemplateFile -- $env:TemplateParametersFile

if($env:PerformDeployment -eq $true)
{
    Write-Host "Deploying ARM Template"
    az deployment group create --name "AdsGoFastDeployment" --resource-group $env:ResourceGroupName --template-file $env:TemplateFile --parameters $env:TemplateParametersFile
}

$env:LogStorageAccountName = "logstg" + $env:ResourceGroupHash
if($env:PerformDeploymentStorageLogging -eq $true)
{
    #StorageAccount For Logging
    az deployment group create -g $env:ResourceGroupName --template-file ./arm/Storage_Logging.json --parameters location=$env:location storage-log-account-name=$env:LogStorageAccountName
}

$env:LogAnalyticsWorkspaceName = "adsloganalytics" + $env:ResourceGroupHash
if($env:PerformDeploymentLogAnalytics -eq $true)
{
    #LogAnalytics
    az deployment group create -g $env:ResourceGroupName --template-file ./arm/LogAnalytics.json --parameters location=$env:location workspaces_adsgofastloganalytics_name=$env:LogAnalyticsWorkspaceName
}

$env:AzureFunctionSiteName = "AdsFuncApp" + $env:ResourceGroupHash
$env:WebAppSiteName = "AdsWebApp" + $env:ResourceGroupHash
$env:ApplicationInsightsName = "appinsights-adsgofast" + $env:ResourceGroupHash
if($env:PerformDeploymentAppService -eq $true)
{
    #App Service (Includes both functions and web)
    $storageaccountkey = (az storage account keys list -g $env:ResourceGroupName -n $env:LogStorageAccountName | ConvertFrom-Json)[0].value
    az deployment group create -g $env:ResourceGroupName --template-file ./arm/AppService.json --parameters location=$env:location azure-function-site-name=$env:LogAnalyticsWorkspaceName resource-group-name=$env:ResourceGroupName app-insights-name=$env:ApplicationInsightsName sites_AdsGoFastWebApp_name=$env:WebAppSiteName storage-log-account-name=$env:LogStorageAccountName storage-log-account-key=$storageaccountkey
}

$env:ApplicationInsightsName = "adsloganalytics" + $env:ResourceGroupHash
if($env:PerformDeploymentLogAnalytics -eq $true)
{
    #Application Insights
    az deployment group create -g $env:ResourceGroupName --template-file ./arm/LogAnalytics.json --parameters location=$env:location workspaces_adsgofastloganalytics_name=$env:LogAnalyticsWorkspaceName
}

if($env:FetchDeploymentDetails -eq $true)
{
    . "$env:RootFolder/LookupDeploymentSettings.ps1"
    LookupDeploymentSettings
}
else 
{
    $azuredeployparams = Get-Content -Raw -Path "$env:RootFolder/azuredeploy.parameters.json" | ConvertFrom-Json
    $env:SQLUser=$azuredeployparams.parameters.'sql-admin-login'.value
    $env:SQLPwd=$azuredeployparams.parameters.'sql-admin-password'.value
    $env:VMOnPIR=$azuredeployparams.parameters.'adf-ir-onp-vm-name'.value
}

#############################################################
# Create Security Group for Web App
#############################################################
$AADUserId = az ad user show --id $AADUser --query objectId --out tsv
#az ad group create --display-name $ADGroup --mail-nickname $ADGroup
#az ad group member check --group $ADGroup --member-id $AADUserId

#############################################################
# MSI Access
#############################################################
log("Creating MSI Access")

#Create Managed Identity Access for Azure Functions and Web App
$IdentityExists = az webapp identity show --name $env:AzureFunction --resource-group $env:ResourceGroupName
if ($IdentityExists-eq $null) {
    Write-Host "Creating MSI for Azure Function"
    az webapp identity assign --resource-group $env:ResourceGroupName --name $env:AzureFunction
}
$IdentityExists=$null
$IdentityExists = az webapp identity show --name $env:WebApp --resource-group $env:ResourceGroupName
if ($IdentityExists-eq $null) {
    Write-Host "Creating MSI for WebApp"
    az webapp identity assign --resource-group $env:ResourceGroupName --name $env:WebApp
}

if ($env:AzureSQLServerAdAdminGrant-eq $true) {
    write-host "Adding AAD Group/User to Azure SQL Logical Server"
    az sql server ad-admin create --display-name $AADUser --object-id $AADUserId --resource-group $ResourceGroupName --server $SQLServer --subscription $SubcriptionId
}

#############################################################
# Configure Azure SQL
#############################################################
. "$env:RootFolder/ConfigureAzureSQL.ps1"
ConfigureAzureSQL

#############################################################
# RBAC Privs
#############################################################
. "$env:RootFolder/GrantRBACPrivs.ps1"
GrantRBACPrivs

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


#############################################################
# Azure Data Factory Deployment
#############################################################
log("Deploying Azure Data Factory Artifacts")

# Create ADF Diagnostic Settings
$logsSetting = "[{'category':'ActivityRuns','enabled':true,'retentionPolicy':{'days': 30,'enabled': true}},{'category':'PipelineRuns','enabled':true,'retentionPolicy':{'days': 30,'enabled': true}},{'category':'TriggerRuns','enabled':true,'retentionPolicy':{'days': 30,'enabled': true}}]".Replace("'",'\"')
$metricsSetting = "[{'category':'AllMetrics','enabled':true,'retentionPolicy':{'days': 30,'enabled': true}}]".Replace("'",'\"')
az monitor diagnostic-settings create --name ADF-Diagnostics --export-to-resource-specific true --resource /subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.DataFactory/factories/$DataFactoryName --logs $logsSetting --metrics $metricsSetting --storage-account /subscriptions/$SubcriptionId/resourceGroups/$ResourceGroupName/providers/Microsoft.Storage/storageAccounts/$LogStorageAccount --workspace /subscriptions/$SubcriptionId/resourcegroups/$ResourceGroupName/providers/microsoft.operationalinsights/workspaces/$LogAnalytics

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
$appSettings = Get-Content $env:AppSettingsFile | ConvertFrom-Json
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