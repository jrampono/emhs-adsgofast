
if ($env:AdsOpts_CD_Services_DataFactory_Enable -eq "True")
{
    Write-Host "Creating Data Factory"
    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/DataFactory.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location adf-name=$env:AdsOpts_CD_Services_DataFactory_Name 
}
else 
{
    Write-Host "Skipped Creation of Data Factory"
}
