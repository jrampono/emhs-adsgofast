Write-Host "Creating Azure SQL Server"
Write-Host $env:AdsOpts_CD_Services_AzureSQLServer_Name
Write-Host $env:AdsOpts_CD_Services_AzureSQLServer_SampleDB_Name
Write-Host $env:AdsOpts_CD_Services_AzureSQLServer_AdsGoFastDB_Name
Write-Host $env:AdsOpts_CD_Services_AzureSQLServer_AdminUser
Write-Host $env:AdsOpts_CD_Services_AzureSQLServer_StagingDB_Name
if($env:AdsOpts_CD_Services_AzureSQLServer_Enable -eq "True")
{
    Write-Host "Creating Azure SQL Server"
    #StorageAccount For Logging
    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/AzureSQLServer.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location sql-server-name=$env:AdsOpts_CD_Services_AzureSQLServer_Name sql-admin-login=$env:AdsOpts_CD_Services_AzureSQLServer_AdminUser sql-admin-password=$env:AdsOpts_CD_Services_AzureSQLServer_AdminPassword sample-db-name=$env:AdsOpts_CD_Services_AzureSQLServer_SampleDB_Name ads-go-fast-db-name=$env:AdsOpts_CD_Services_AzureSQLServer_AdsGoFastDB_Name staging-db-name=$env:AdsOpts_CD_Services_AzureSQLServer_StagingDB_Name vnet-name=$env:AdsOpts_CD_Services_Vnet_Name
    
    #Make sure password is correct
    az sql server update -n $env:AdsOpts_CD_Services_AzureSQLServer_Name -g $env:AdsOpts_CD_ResourceGroup_Name -p ($env:AdsOpts_CD_Services_AzureSQLServer_AdminPassword  | Out-String)
}
else 
{
    Write-Host "Skipped Creation of Azure SQL Server"
}