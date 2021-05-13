
if ($env:AdsOpts_CD_Services_WebSite_Enable -eq "True")
{   
    if ($env:AdsOpts_CD_Services_WebSite_AppServiceResourceGroup -eq $null)
    {
        $rg = $env:AdsOpts_CD_ResourceGroup_Name
    }
    else 
    {
        $rg = $env:AdsOpts_CD_Services_WebSite_AppServiceResourceGroup
    }

    if ($env:AdsOpts_CD_Services_WebSite_AppServiceName -eq $null)
    {
        $sn = $env:AdsOpts_CD_Services_AppPlans_WebApp_Name
    }
    else 
    {
        $sn = $env:AdsOpts_CD_Services_WebSite_AppServiceName
    }
    Write-Host "Deploying Wesite to $sn in resource group $rg" 
    $appserviceid = ((az appservice plan show --name $sn --resource-group $rg) | ConvertFrom-Json).id
    $appinsightsid = (az monitor app-insights component show -g $env:AdsOpts_CD_ResourceGroup_Name --app $env:AdsOpts_CD_Services_AppInsights_Name | ConvertFrom-Json).id
    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/WebApp.json --parameters location=$env:AdsOpts_CD_ResourceGroup_Location resource-group-name=$env:AdsOpts_CD_ResourceGroup_Name sites_AdsGoFastWebApp_name=$env:AdsOpts_CD_Services_WebSite_Name appservice-name=$sn}
else 
{
    Write-Host "Skipped Creation of Web Site"
}

