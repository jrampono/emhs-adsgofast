function ConfigureAzureSQL{

    if ($env:AzureSQLServerFirewallExceptions-eq $true) 
    {
        #Add Ip to SQL Firewall
        $myIp = (Invoke-WebRequest ifconfig.me/ip).Content
        az sql server firewall-rule create -g $ResourceGroupName -s $SQLServer -n "MySetupIP" --start-ip-address $myIp --end-ip-address $myIp
        #Allow Azure services and resources to access this server
        az sql server firewall-rule create -g $ResourceGroupName -s $SQLServer -n "Azure" --start-ip-address 0.0.0.0 --end-ip-address 0.0.0.0
    }

    if ($env:ServicePrincipalCreate-eq $true) 
    {
        #Create Service Principal
        $env:ServicePrincipalPassword = az ad sp create-for-rbac --name $env:ServicePrincipalName --query password --out tsv
    }
    #Grant Azure AD Accounts rights to Databases 
    $sqlcommand = ""

    if ($env:AzureSQLServerGrantSPRightsToAdsGoFastDB -eq $true) 
    {
        # MSI Access Service Principal on Azure SQL AdsGoFast DB
        $sqlcommand = $sqlcommand + "
        CREATE USER [$env:ServicePrincipalName] FROM EXTERNAL PROVIDER;
        ALTER ROLE db_datareader ADD MEMBER [$env:ServicePrincipalName];
        ALTER ROLE db_datawriter ADD MEMBER [$env:ServicePrincipalName];
        ALTER ROLE db_ddladmin ADD MEMBER [$env:ServicePrincipalName];
        GRANT EXECUTE ON SCHEMA::[dbo] TO [$env:ServicePrincipalName];
        GO"
    }

    if ($env:AzureSQLServerGrantAFRightsToAdsGoFastDB -eq $true) 
    {
        # MSI Access Azure Function on Azure SQL AdsGoFast DB
        $sqlcommand = $sqlcommand + "
        CREATE USER [$env:AzureFunction] FROM EXTERNAL PROVIDER;
        ALTER ROLE db_datareader ADD MEMBER [$env:AzureFunction];
        ALTER ROLE db_datawriter ADD MEMBER [$env:AzureFunction];
        ALTER ROLE db_ddladmin ADD MEMBER [$env:AzureFunction];
        GRANT EXECUTE ON SCHEMA::[dbo] TO [$env:AzureFunction];
        GO"
    }

    if ($env:AzureSQLServerGrantADFRightsToStagingDB-eq $true) 
    {
        # MSI Access Azure Data Factory on Azure SQL Staging DB
        $sqlcommand = $sqlcommand +  "
        CREATE USER [$env:DataFactoryName] FROM EXTERNAL PROVIDER;
        ALTER ROLE db_owner ADD MEMBER [$env:DataFactoryName];
        GO"
    }

    if ($env:AzureSQLServerGrantWebAppRightsToAdsGoFastDB -eq $true) 
    {
        # MSI Access WebApp on Azure SQL ADSGoFast DB
        $sqlcommand = $sqlcommand +  "
        CREATE USER [$env:WebApp] FROM EXTERNAL PROVIDER;
        ALTER ROLE db_datareader ADD MEMBER [$env:WebApp];
        ALTER ROLE db_datawriter ADD MEMBER [$env:WebApp];
        ALTER ROLE db_ddladmin ADD MEMBER [$env:WebApp];
        GO"
    }

    if ($env:AzureSQLServerGrantADFRightsToAdventureWorksDB-eq $true) 
    {
        # MSI Access Azure Data Factory on Azure SQL AdventureWorld DB
        $sqlcommand = $sqlcommand +  "
        CREATE USER [$env:DataFactoryName] FROM EXTERNAL PROVIDER;
        ALTER ROLE db_datareader ADD MEMBER [$env:DataFactoryName];
        ALTER ROLE db_datawriter ADD MEMBER [$env:DataFactoryName];
        ALTER ROLE db_ddladmin ADD MEMBER [$env:DataFactoryName];
        GO"
    }


    if ($env:AzureSQLServerAutoCreateAzureAdAccess-eq $true) 
    {    
        $token=$(az account get-access-token --resource=https://database.windows.net --query accessToken --output tsv)
        Invoke-Sqlcmd -ServerInstance "$SQLServer.database.windows.net,1433" -Database $sampleDB -AccessToken $token -query $sqlcommand
    }
    else 
    {
        Write-Host "Printing SQL Statement to grant Azure AD privileges.. please execute manually by logging into the Azure SQL instance using an AzureAD account."
        Write-Host $sqlcommand
    }

}