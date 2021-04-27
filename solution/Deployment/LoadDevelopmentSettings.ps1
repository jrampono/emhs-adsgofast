
function LoadDevelopmentSettings {
    #Core Input Parameters
    $env:Subscription = "Jorampon Internal Consumption"
    $env:ResourceGroupName = "AdsTest"
    $env:AADUser="jorampon@microsoft.com"
    # Service Principal Used by Azure Functions for downstream azure service connections 
    $env:ServicePrincipalNameAFDownstream="AdsGoFastDeployTestSP"
    # Service Principal Used for Web App AAD integration
    $env:ServicePrincipalNameWebAuth="adsgofastwebappauth"
    # Service Principal Used for Function pp AAD integration
    $env:ServicePrincipalNameAFAuth="adsgofastwebappauth"
    
    $env:Location="australiaeast"
    $env:TemplateFile="$env:RootFolder/azuredeploy.json"
    $env:TemplateParametersFile="$env:RootFolder/azuredeploy.parameters.json"
    
    #Azure Cli Login 
    $env:UseInteractiveAzCliLogin=$true #Set to false if you are going to use a Service Principal to Log in to Azure CLI. If so you need to populate the two variables below. Note Azure DevOps will auto populate these if you are deploying via DevOps
    $env:servicePrincipalId=$null 
    $env:servicePrincipalKey=$null

    #Set to true if you want Pre-req programs etc to be installed automatically
    $env:PerformLocalInstalls=$true
    $env:PerformLocalInstallsAzCli=$false
    $env:PerformLocalInstallsAzCliAddToPath=$true

    #ARM Deployment Options
    $env:PerformDeployment=$false #Set to true if you are performing ARM deployment
    $env:FetchDeploymentDetails=$false  #Set to true if you want additional details to be fetched from the ARM deployment if this is false then the values below need to be provided
    $env:DataFactoryName="adsgofastdatakakeacceladf"
    $env:LogStorageAccount=$null
    $env:LogAnalytics="ADSGoFastLogAnalytics"
    $env:AppInsights="AdsGoFastFunc2"
    $env:ADLSStorageAccount=$null
    $env:SQLServer="adsgofastdatakakeaccelsqlsvr"
    $env:adsGoFastDB="AdsGoFast"
    $env:stagingDB="staging"
    $env:sampleDB="AwSample"
    $env:AzureFunction="AdsGoFastFunc2"
    $env:KeyVaultName="adsgofastkeyvault"
    $env:VMAzIR=$null
    $env:WebApp="AdsGoFast"

    #Toggle Post Deployment Installs -- Should All be true for end to end new prod install
    #WebApp
    $env:WebAppPublish=$false
    #ServicePrincipal
    $env:ServicePrincipalCreate=$false
    #AzureSQL Options
    $env:AzureSQLServerAdAdminGrant=$false
    $env:AzureSQLServerFirewallExceptions=$false
    $env:AzureSQLServerGrantADFRightsToAdventureWorksDB=$false
    $env:AzureSQLServerGrantWebAppRightsToAdsGoFastDB=$false
    $env:AzureSQLServerGrantADFRightsToStagingDB=$false
    $env:AzureSQLServerGrantAFRightsToAdsGoFastDB=$true
    $env:AzureSQLServerGrantSPRightsToAdsGoFastDB=$false
    $env:AzureSQLServerAutoCreateAzureAdAccess=$false #Will attempt to automatically grant AzureAD accounts rights to db. May fail if your AAD security settings do not allow this kind of login. When set to false this will print the SQL statement for manual execution
    #RBACOptions
    $env:RBACSPToADF=$false
    $env:RBACAFToADF=$false
    $env:RBACAFToLogAnalytics=$false
    $env:RBACAFToADLSGen2=$false
    $env:RBACADFToADLSGen2=$false
    $env:RBACADFToKeyVault=$false
    $env:RBACWebAppToADLSGen2=$false
    $env:RBACWebAppToLogAnalytics=$false
    $env:RBACAADUserToADLSGen2=$false   
    $env:RBACAADUserToADLSGen2=$false
    #AzureFunctionOptions
    $env:AzureFunctionEnableAADAuth=$false
    $env:AzureFunctionPublish=$true
    $env:AzureFunctionGenerateFunctionKeys=$false
    $env:AzureFunctionSetAppSettings=$true
    $env:AzureFunctionCreateRole=$false
    
}
