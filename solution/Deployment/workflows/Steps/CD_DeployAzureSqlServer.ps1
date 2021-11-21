Write-Debug"Creating Azure SQL Server"
Write-Debug$env:AdsOpts_CD_Services_AzureSQLServer_Name
Write-Debug$env:AdsOpts_CD_Services_AzureSQLServer_SampleDB_Name
Write-Debug$env:AdsOpts_CD_Services_AzureSQLServer_AdsGoFastDB_Name
Write-Debug$env:AdsOpts_CD_Services_AzureSQLServer_AdminUser
Write-Debug$env:AdsOpts_CD_Services_AzureSQLServer_StagingDB_Name
if($env:AdsOpts_CD_Services_AzureSQLServer_Enable -eq "True")
{
    Write-Debug"Creating Azure SQL Server"
    #StorageAccount For Logging
    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/AzureSQLServer.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location sql_server_name=$env:AdsOpts_CD_Services_AzureSQLServer_Name sql_admin_login=$env:AdsOpts_CD_Services_AzureSQLServer_AdminUser sql_admin_password=$env:AdsOpts_CD_Services_AzureSQLServer_AdminPassword sample_db_name=$env:AdsOpts_CD_Services_AzureSQLServer_SampleDB_Name ads_go_fast_db_name=$env:AdsOpts_CD_Services_AzureSQLServer_AdsGoFastDB_Name staging_db_name=$env:AdsOpts_CD_Services_AzureSQLServer_StagingDB_Name vnet_name=$env:AdsOpts_CD_Services_Vnet_Name
    
    #Make sure password is correct
    az sql server update -n $env:AdsOpts_CD_Services_AzureSQLServer_Name -g $env:AdsOpts_CD_ResourceGroup_Name -p ($env:AdsOpts_CD_Services_AzureSQLServer_AdminPassword  | Out-String)
}
else 
{
    Write-Warning "Skipped Creation of Azure SQL Server"
}