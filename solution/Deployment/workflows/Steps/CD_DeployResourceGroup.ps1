if($env:AdsOpts_CD_ResourceGroup_Enabled -eq $true)
{
    Write-Host "Creating Resource Group"
    az group create --name $env:AdsOpts_CD_ResourceGroup_Name --location $env:AdsOpts_CD_ResourceGroup_Location
}
