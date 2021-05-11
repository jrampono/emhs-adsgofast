[Environment]::SetEnvironmentVariable("ENVIRONMENT_NAME", "development")
. .\Steps\PushEnvFileIntoVariables.ps1
ParseEnvFile("$env:ENVIRONMENT_NAME")
Invoke-Expression -Command  ".\Steps\CD_SetResourceGroupHash.ps1"


if($env:AdsOpts_CD_Services_CoreFunctionApp_Enable -eq "True")
{
    $id = $null
    $id = ((az functionapp identity show --name $env:AdsOpts_CD_Services_CoreFunctionApp_Name --resource-group $env:AdsOpts_CD_ResourceGroup_Name) | ConvertFrom-Json).principalId
    if ($IdentityExists-eq $null) {
        Write-Host "Creating MSI for FunctionApp"
        $id = ((az functionapp identity assign --resource-group $env:AdsOpts_CD_ResourceGroup_Name --name $env:AdsOpts_CD_Services_CoreFunctionApp_Name) | ConvertFrom-Json).principalId
    }
}

if($env:AdsOpts_CD_Services_WebSite_Enable -eq "True")
{
    $id = $null
    $id = ((az webapp identity show --name $env:AdsOpts_CD_Services_WebSite_Name --resource-group $env:AdsOpts_CD_ResourceGroup_Name) | ConvertFrom-Json).principalId
    if ($IdentityExists-eq $null) {
        Write-Host "Creating MSI for WebApp"
        $id = ((az webapp identity assign --resource-group $env:AdsOpts_CD_ResourceGroup_Name --name $env:AdsOpts_CD_Services_WebSite_Name) | ConvertFrom-Json).principalId
    }
}

#Give MSIs Required AD Privileges
#Assign SQL Admin
$cu = az ad signed-in-user show | ConvertFrom-Json
az sql server ad-admin create --display-name $cu.DisplayName --object-id $cu.ObjectId --resource-group $env:AdsOpts_CD_ResourceGroup_Name --server $env:AdsOpts_CD_Services_AzureSQLServer_Name --subscription $env:AdsOpts_CD_ResourceGroup_Subscription

#az login --service-principal --username $env:secrets_AZURE_CREDENTIALS_clientId --password $env:secrets_AZURE_CREDENTIALS_clientSecret --tenant $env:secrets_AZURE_CREDENTIALS_tenantId

#Function App to ADS GO FAST DB
$sqlcommand = "
        DROP USER IF EXISTS [$env:AdsOpts_CD_Services_CoreFunctionApp_Name] 
        CREATE USER [$env:AdsOpts_CD_Services_CoreFunctionApp_Name] FROM EXTERNAL PROVIDER;
        ALTER ROLE db_datareader ADD MEMBER [$env:AdsOpts_CD_Services_CoreFunctionApp_Name];
        ALTER ROLE db_datawriter ADD MEMBER [$env:AdsOpts_CD_Services_CoreFunctionApp_Name];
        ALTER ROLE db_ddladmin ADD MEMBER [$env:AdsOpts_CD_Services_CoreFunctionApp_Name];
        GRANT EXECUTE ON SCHEMA::[dbo] TO [$env:AdsOpts_CD_Services_CoreFunctionApp_Name];
        GO"

$sqlcommand = $sqlcommand + "
        DROP USER IF EXISTS [$env:AdsOpts_CD_Services_WebSite_Name] 
        CREATE USER [$env:AdsOpts_CD_Services_WebSite_Name] FROM EXTERNAL PROVIDER;
        ALTER ROLE db_datareader ADD MEMBER [$env:AdsOpts_CD_Services_WebSite_Name];
        ALTER ROLE db_datawriter ADD MEMBER [$env:AdsOpts_CD_Services_WebSite_Name];
        GRANT EXECUTE ON SCHEMA::[dbo] TO [$env:AdsOpts_CD_Services_WebSite_Name];
        GO
"

$SqlInstalled = Get-InstalledModule SqlServer
if($null -eq $SqlInstalled)
{
    write-host "Installing SqlServer Module"
    Install-Module -Name SqlServer -Scope CurrentUser 
}

$token=$(az account get-access-token --resource=https://database.windows.net --query accessToken --output tsv)
Invoke-Sqlcmd -ServerInstance "$env:AdsOpts_CD_Services_AzureSQLServer_Name.database.windows.net,1433" -Database $env:AdsOpts_CD_Services_AzureSQLServer_AdsGoFastDB_Name -AccessToken $token -query $sqlcommand