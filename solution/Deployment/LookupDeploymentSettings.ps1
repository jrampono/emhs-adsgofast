function LookupDeploymentSettings
{
    $DeploymentSettings = New-Object System.Collections.Generic.Dictionary"[String,String]"

    Write-Host "Looking Up Details for Deployment in ResourceGroup: $env:ResourceGroupName"
    $Deployment= az deployment group show --name "AdsGoFastDeployment" --resource-group "$env:ResourceGroupName" 
    $deployment = ($Deployment | ConvertFrom-Json)

    if ($env:FetchDeploymentDetails)
    {
        $DeploymentSettings.Add("DATAFACTORYNAME",$deployment.properties.outputs.stringDataFactoryName.value)
        $DeploymentSettings.Add("LOGSTORAGEACCOUNT",$deployment.properties.outputs.stringLogStorageAccount.value)
    #ToDO


    }

    #Make all settings available to downstream pipeline tasks
    foreach ($s in $DeploymentSettings.GetEnumerator()) {
        if ($LocalDev -eq $false)
        {
            Write-Host "##vso[task.setvariable variable=$($s.Key)]$($s.Value)"
        }
        else {
            $key = $s.Key
            $value = $s.Value
            Set-Item "env:$key" $value
        }
    }
}