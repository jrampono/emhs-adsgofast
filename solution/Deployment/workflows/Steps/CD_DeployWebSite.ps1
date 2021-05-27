
if ($env:AdsOpts_CD_Services_WebSite_Enable -eq "True")
{   
    if ($env:AdsOpts_CD_Services_AppPlans_WebApp_ResourceGroup -eq $null)
    {
        $rg = $env:AdsOpts_CD_ResourceGroup_Name
    }
    else 
    {
        $rg = $env:AdsOpts_CD_Services_AppPlans_WebApp_ResourceGroup
    }

    
    $sn = $env:AdsOpts_CD_Services_WebSite_AppServiceName
    
    Write-Host "Deploying Wesite to $sn in resource group $rg" 
    az deployment group create -g $env:AdsOpts_CD_ResourceGroup_Name --template-file ./../arm/WebApp.json resource-group-name=$env:AdsOpts_CD_ResourceGroup_Name sites_AdsGoFastWebApp_name=$env:AdsOpts_CD_Services_WebSite_Name appservice-name=$sn}
else 
{
    Write-Host "Skipped Creation of Web Site"
}

