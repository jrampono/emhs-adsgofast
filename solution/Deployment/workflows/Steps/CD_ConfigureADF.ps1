# Create ADF Diagnostic Settings
$logsSetting = "[{'category':'ActivityRuns','enabled':true,'retentionPolicy':{'days': 30,'enabled': true}},{'category':'PipelineRuns','enabled':true,'retentionPolicy':{'days': 30,'enabled': true}},{'category':'TriggerRuns','enabled':true,'retentionPolicy':{'days': 30,'enabled': true}}]".Replace("'",'\"')
$metricsSetting = "[{'category':'AllMetrics','enabled':true,'retentionPolicy':{'days': 30,'enabled': true}}]".Replace("'",'\"')

az monitor diagnostic-settings create --name ADF-Diagnostics --export-to-resource-specific true --resource "$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name" --logs $logsSetting --metrics $metricsSetting --storage-account "$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.Storage/storageAccounts/$env:AdsOpts_CD_Services_Storage_Logging_Name" --workspace "$env:AdsOpts_CD_ResourceGroup_Id/providers/microsoft.operationalinsights/workspaces/$env:AdsOpts_CD_Services_LogAnalytics_Name"

#Create IRs

#Create Managed Network
$subid = (az account show -s $env:AdsOpts_CD_ResourceGroup_Subscription | ConvertFrom-Json).id
$uri = "https://management.azure.com/subscriptions/$subid/resourceGroups/$env:AdsOpts_CD_ResourceGroup_Name/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/managedVirtualNetworks/default" + '?api-version=2018-06-01'

az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body '{\"properties\": {}}'

#VNET
$body = '
{
    "properties": {
        "type": "Managed",
        "typeProperties": {
            "computeProperties": {
                "location": "AutoResolve",
                "dataFlowProperties": {
                    "computeType": "General",
                    "coreCount": 8,
                    "timeToLive": 10,
                    "cleanup": true
                }
            }
        },
        "managedVirtualNetwork": {
            "type": "ManagedVirtualNetworkReference",
            "referenceName": "default"
        }
    }
}' | ConvertFrom-Json
$body = ($body | ConvertTo-Json -compress  -Depth 10 | Out-String).Replace('"','\"')
$uri = "https://management.azure.com/$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/integrationRuntimes/$env:AdsOpts_CD_Services_DataFactory_AzVnetIR_Name" + '?api-version=2018-06-01'
Write-Host "Creating IR: $env:AdsOpts_CD_Services_DataFactory_AzVnetIR_Name"
az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body $body --uri-parameters 'api-version=2018-06-01'

#On Prem - Note we are using a managed VNET IR to mimic on prem
$body = '
{
    "properties": {
        "type": "Managed",
        "typeProperties": {
            "computeProperties": {
                "location": "AutoResolve",
                "dataFlowProperties": {
                    "computeType": "General",
                    "coreCount": 8,
                    "timeToLive": 10,
                    "cleanup": true
                }
            }
        },
        "managedVirtualNetwork": {
            "type": "ManagedVirtualNetworkReference",
            "referenceName": "default"
        }
    }
}' | ConvertFrom-Json
$body = ($body | ConvertTo-Json -compress  -Depth 10 | Out-String).Replace('"','\"')
$uri = "https://management.azure.com/$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/integrationRuntimes/$env:AdsOpts_CD_Services_DataFactory_OnPremVnetIr_Name" + '?api-version=2018-06-01'
Write-Host "Creating IR: $env:AdsOpts_CD_Services_DataFactory_OnPremVnetIr_Name"
az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body $body --uri-parameters 'api-version=2018-06-01'

$dfbase = "$env:AdsOpts_CD_FolderPaths_PublishUnZip/datafactory"

#Data Factory - LinkedServices
Get-ChildItem "$dfbase/linkedService" -Filter *.json | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName
    $jsonobject = $_ | Get-Content | ConvertFrom-Json

    #Swap out Key Vault Url for Function App Linked Service
    if($lsName -eq "AdsGoFastKeyVault")
    {
        $jsonobject.properties.typeProperties.baseUrl = "https://$env:AdsOpts_CD_Services_KeyVault_Name.vault.azure.net/"
    }

    #Swap out Function App Url
    if($lsName -eq "AzureFunctionAdsGoFastDataLakeAccelFunApp")
    {
        $jsonobject.properties.typeProperties.functionAppUrl = "https://$env:AdsOpts_CD_Services_CoreFunctionApp_Name.azurewebsites.net"
    }
    
    #ParseOut the Name Attribute
    $name = $jsonobject.name

    #Persist File Back
    $jsonobject | ConvertTo-Json  -Depth 100 | set-content $_

    #Make a copy of the file for upload 
    Copy-Item  -Path $fileName -Destination "FileForUpload.json"

    if  (
            (($env:AdsOpts_CD_Services_DataFactory_OnPremVnetIr_Enable -eq "True") -and ($lsName.Contains("_OnPrem_Net") -eq $true)) -or
            (($env:AdsOpts_CD_Services_DataFactory_AzVnetIR_Enable -eq "True") -and ($lsName.Contains("_SH_IR") -eq $true)) -or
            (($lsName.Contains("_SH_IR") -eq $false) -and ($lsName.Contains("_OnPrem_Net") -eq $false))
        )
    {
        write-host ("LinkedService:" + $lsName) -ForegroundColor Yellow -BackgroundColor DarkGreen
        #Set-AzDataFactoryV2LinkedService -DataFactoryName $env:AdsOpts_CD_Services_DataFactory_Name -ResourceGroupName $env:AdsOpts_CD_ResourceGroup_Name -Name $lsName -DefinitionFile $fileName -force
        $body = ($jsonobject | ConvertTo-Json -compress  -Depth 10 | Out-String).Replace('"','\"')
        $uri = "https://management.azure.com/$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/linkedservices/$name" 
        write-host $uri
        az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body "@FileForUpload.json" --uri-parameters 'api-version=2018-06-01'

    }
    else 
    {
        Write-Host "Skipped: $lsName" -ForegroundColor Black -BackgroundColor Yellow
    }
}

#Data Factory - Dataset
Get-ChildItem "$dfbase/dataset" -Filter *.json | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName

    #ParseOut the Name Attribute
    $jsonobject = $_ | Get-Content | ConvertFrom-Json
    $name = $jsonobject.name

    #Persist File Back
    $jsonobject | ConvertTo-Json  -Depth 100 | set-content $_

    #Make a copy of the file for upload 
    Copy-Item  -Path $fileName -Destination "FileForUpload.json"

    if  (
            (($env:AdsOpts_CD_Services_DataFactory_OnPremVnetIr_Enable -eq "True") -and ($lsName.Contains("_OnPrem_SH_IR") -eq $true)) -or
            (($env:AdsOpts_CD_Services_DataFactory_AzVnetIR_Enable -eq "True") -and ($lsName.Contains("_SH_IR") -eq $true) -and ($lsName.Contains("_OnPrem_SH_IR") -eq $false)) -or
            (($lsName.Contains("_SH_IR") -eq $false) -and ($lsName.Contains("_OnPrem_SH_IR") -eq $false))
        )
    {
        write-host ("Dataset: " + $fileName) -ForegroundColor Yellow -BackgroundColor DarkGreen
        #Set-AzDataFactoryV2Dataset -DataFactoryName $env:AdsOpts_CD_Services_DataFactory_Name -ResourceGroupName $env:AdsOpts_CD_ResourceGroup_Name -Name $lsName -DefinitionFile $fileName -Force
        $body = ($jsonobject | ConvertTo-Json -compress  -Depth 10 | Out-String).Replace('"','\"')
        $uri = "https://management.azure.com/$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/datasets/$name" 
        az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body "@FileForUpload.json" --uri-parameters 'api-version=2018-06-01'
    }
    else 
    {
        Write-Host "Skipped: $lsName" -ForegroundColor Black -BackgroundColor Yellow
    }
}


#Move to pipelines directory
$CurrentPath = (Get-Location).Path
Set-Location "..\bin\publish\unzipped\datafactory\pipeline"

#Data Factory - Pipelines
Get-ChildItem "./" -Recurse -Include "AZ-Function*.json", "OnP-SQL-Watermark-OnP-SH-IR.json", "AZ-SQL-Watermark-SH-IR.json" | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName
    
    #ParseOut the Name Attribute
    $jsonobject = $_ | Get-Content | ConvertFrom-Json
    $name = $jsonobject.name

    #Persist File Back
    $jsonobject | ConvertTo-Json  -Depth 100 | set-content $_

    #Make a copy of the file for upload 
    Copy-Item  -Path $fileName -Destination "FileForUpload.json"

    if  (
            (($env:AdsOpts_CD_Services_DataFactory_OnPremVnetIr_Enable -eq "True") -and ($lsName.Contains('-OnP-SH-IR') -eq $true)) -or
            (($env:AdsOpts_CD_Services_DataFactory_AzVnetIR_Enable -eq "True") -and ($lsName.Contains('-SH-IR') -eq $true) -and ($lsName.Contains('-OnP-SH-IR') -eq $false)) -or
            (($lsName.Contains('SH-IR') -eq $false) -and ($lsName.Contains('-OnP-SH-IR') -eq $false))
        )
    {
        write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
        #Set-AzDataFactoryV2Pipeline -DataFactoryName $env:AdsOpts_CD_Services_DataFactory_Name -ResourceGroupName $env:AdsOpts_CD_ResourceGroup_Name -Name $lsName -DefinitionFile $fileName -force
        #$body = ($jsonobject | ConvertTo-Json -compress  -Depth 100 | Out-String).Replace('"','\"')
        $uri = "https://management.azure.com/$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/pipelines/$name" 
        az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body "@FileForUpload.json" --uri-parameters 'api-version=2018-06-01'
    }
    else 
    {
        Write-Host "Skipped: $lsName" -ForegroundColor Black -BackgroundColor Yellow
    }
}

Get-ChildItem "./" -Recurse -Include "az-sql-full-load-sh-ir.json", "sh-sql-full-load-sh-ir.json", "onp-sql-full-load-onp-sh-ir.json", "sh-sql-watermark-sh-ir.json" | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName

    #ParseOut the Name Attribute
    $jsonobject = $_ | Get-Content | ConvertFrom-Json
    $name = $jsonobject.name

    #Persist File Back
    $jsonobject | ConvertTo-Json  -Depth 100 | set-content $_
    
    #Make a copy of the file for upload 
    Copy-Item  -Path $fileName -Destination "FileForUpload.json"

    if  (
            (($env:AdsOpts_CD_Services_DataFactory_OnPremVnetIr_Enable -eq "True") -and ($lsName.Contains('-OnP-SH-IR') -eq $true)) -or
            (($env:AdsOpts_CD_Services_DataFactory_AzVnetIR_Enable -eq "True") -and ($lsName.Contains('-SH-IR') -eq $true) -and ($lsName.Contains('-OnP-SH-IR') -eq $false)) -or
            (($lsName.Contains('SH-IR') -eq $false) -and ($lsName.Contains('-OnP-SH-IR') -eq $false))
        )
    {
        write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
        #Set-AzDataFactoryV2Pipeline -DataFactoryName $env:AdsOpts_CD_Services_DataFactory_Name -ResourceGroupName $env:AdsOpts_CD_ResourceGroup_Name -Name $lsName -DefinitionFile $fileName -Force
        $body = ($jsonobject | ConvertTo-Json -compress  -Depth 100 | Out-String).Replace('"','\"')
        $uri = "https://management.azure.com/$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/pipelines/$name" 
        az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body "@FileForUpload.json" --uri-parameters 'api-version=2018-06-01'
    }
    else 
    {
        Write-Host "Skipped: $lsName" -ForegroundColor Black -BackgroundColor Yellow
    }
}

Get-ChildItem "./" -Filter *chunk*.json | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName

    #ParseOut the Name Attribute
    $jsonobject = $_ | Get-Content | ConvertFrom-Json
    $name = $jsonobject.name

    #Persist File Back
    $jsonobject | ConvertTo-Json  -Depth 100 | set-content $_

    if  (
            (($env:AdsOpts_CD_Services_DataFactory_OnPremVnetIr_Enable -eq "True") -and ($lsName.Contains('-OnP-SH-IR') -eq $true)) -or
            (($env:AdsOpts_CD_Services_DataFactory_AzVnetIR_Enable -eq "True") -and ($lsName.Contains('-SH-IR') -eq $true) -and ($lsName.Contains('-OnP-SH-IR') -eq $false)) -or
            (($lsName.Contains('SH-IR') -eq $false) -and ($lsName.Contains('-OnP-SH-IR') -eq $false))
        )
    {
        write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
        #Set-AzDataFactoryV2Pipeline -DataFactoryName $env:AdsOpts_CD_Services_DataFactory_Name -ResourceGroupName $env:AdsOpts_CD_ResourceGroup_Name -Name $lsName -DefinitionFile $fileName -Force
        $body = ($jsonobject | ConvertTo-Json -compress  -Depth 100 | Out-String).Replace('"','\"')
        $uri = "https://management.azure.com/$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/pipelines/$name" 
        az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body "@FileForUpload.json" --uri-parameters 'api-version=2018-06-01'
    }
    else 
    {
        Write-Host "Skipped: $lsName" -ForegroundColor Black -BackgroundColor Yellow 
    }
}


Get-ChildItem "./" -Exclude "FileForUpload.json", "Master.json","AZ-Function*.json", "OnP-SQL-Watermark-OnP-SH-IR.json", "AZ-SQL-Watermark-SH-IR.json", "*chunk*.json", "az-sql-full-load-sh-ir.json", "sh-sql-full-load-sh-ir.json", "onp-sql-full-load-onp-sh-ir.json", "sh-sql-watermark-sh-ir.json" | 
Foreach-Object {
    $lsName = $_.BaseName 
    $fileName = $_.FullName

    #ParseOut the Name Attribute
    $jsonobject = $_ | Get-Content | ConvertFrom-Json
    $name = $jsonobject.name

    #Persist File Back
    $jsonobject | ConvertTo-Json  -Depth 100 | set-content $_

    #Make a copy of the file for upload 
    Copy-Item  -Path $fileName -Destination "FileForUpload.json"

    if  (
            (($env:AdsOpts_CD_Services_DataFactory_OnPremVnetIr_Enable -eq "True") -and ($lsName.Contains('-OnP-SH-IR') -eq $true)) -or
            (($env:AdsOpts_CD_Services_DataFactory_AzVnetIR_Enable -eq "True") -and ($lsName.Contains('-SH-IR') -eq $true) -and ($lsName.Contains('-OnP-SH-IR') -eq $false)) -or
            (($lsName.Contains('SH-IR') -eq $false) -and ($lsName.Contains('-OnP-SH-IR') -eq $false))
        )
    {
        write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
        #Set-AzDataFactoryV2Pipeline -DataFactoryName $env:AdsOpts_CD_Services_DataFactory_Name -ResourceGroupName $env:AdsOpts_CD_ResourceGroup_Name -Name $lsName -DefinitionFile $fileName -Force
        $body = ($jsonobject | ConvertTo-Json -compress  -Depth 100 | Out-String).Replace('"','\"')
        $uri = "https://management.azure.com/$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/pipelines/$name" 
        az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body "@FileForUpload.json" --uri-parameters 'api-version=2018-06-01'
    }
    else 
    {
        Write-Host "Skipped: $lsName" -ForegroundColor Black -BackgroundColor Yellow
    }
}

Get-ChildItem "./" -Filter Master.json | 
Foreach-Object {
    #filter out any undelpoyed pipelines
    $jsonobject = $_ | Get-Content | ConvertFrom-Json
    $name = $jsonobject.name
    $cases = $jsonobject.properties.activities[0].typeProperties.cases
    $casesfiltered = @()
    foreach ($item in $cases) {
        if  (
            (($env:AdsOpts_CD_Services_DataFactory_OnPremVnetIr_Enable -eq "True") -and ($item.Value.Contains('-OnP-SH-IR') -eq $true)) -or
            (($env:AdsOpts_CD_Services_DataFactory_AzVnetIR_Enable -eq "True") -and ($item.Value.Contains('-SH-IR') -eq $true) -and ($item.Value.Contains('-OnP-SH-IR') -eq $false)) -or
            (($item.Value.Contains('SH-IR') -eq $false) -and ($item.Value.Contains('-OnP-SH-IR') -eq $false)) 
        )
        {
            #Exclude pipelines that require Self Hosted IRs (eg. Excel ) TODO - make this optional
            if(($item.Value.Contains('Excel') -eq $false))
            {
                $casesfiltered = $casesfiltered + $item
            }
        }
        else 
        {
            Write-Host "Skipped: $item.Value" -ForegroundColor Black -BackgroundColor Yellow
        }
    }
    $jsonobject.properties.activities[0].typeProperties.cases = $casesfiltered
    
    #Persist File Back
    $jsonobject | ConvertTo-Json  -Depth 100 | set-content $_

    $lsName = $_.BaseName 
    $fileName = $_.FullName

    #Make a copy of the file for upload 
    Copy-Item  -Path $fileName -Destination "FileForUpload.json"

    write-host $fileName -ForegroundColor Yellow -BackgroundColor DarkGreen
    #Set-AzDataFactoryV2Pipeline -DataFactoryName $env:AdsOpts_CD_Services_DataFactory_Name -ResourceGroupName $env:AdsOpts_CD_ResourceGroup_Name -Name $lsName -DefinitionFile $fileName -Force
    $body = ($jsonobject | ConvertTo-Json -compress  -Depth 100 | Out-String).Replace('"','\"')
    $uri = "https://management.azure.com/$env:AdsOpts_CD_ResourceGroup_Id/providers/Microsoft.DataFactory/factories/$env:AdsOpts_CD_Services_DataFactory_Name/pipelines/$name" 
    az rest --method put --uri $uri --headers '{\"Content-Type\":\"application/json\"}' --body "@FileForUpload.json" --uri-parameters 'api-version=2018-06-01'
}

#Change Back to Workflows dir
Set-Location $CurrentPath