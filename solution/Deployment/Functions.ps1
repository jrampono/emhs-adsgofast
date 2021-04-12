#############################################################
# Functions
#############################################################
function publish{
    param(
        $projectName        
    )

    $projectPath="src/$($projectName)/$($projectName).csproj"
    $publishDestPath="publish/" + [guid]::NewGuid().ToString()

    log "publishing project '$($projectName)' in folder '$($publishDestPath)' ..." 
    dotnet publish $projectPath -c Release -o $publishDestPath

    $zipArchiveFullPath="$($publishDestPath).Zip"
    log "creating zip archive '$($zipArchiveFullPath)'"
    $compress = @{
        Path = $publishDestPath + "/*"
        CompressionLevel = "Fastest"
        DestinationPath = $zipArchiveFullPath
    }
    Compress-Archive @compress

    log "cleaning up ..."
    Remove-Item -path "$($publishDestPath)" -recurse

    return $zipArchiveFullPath
}

function log{
    param(
        $text
    )

    write-host $text -ForegroundColor Yellow -BackgroundColor DarkGreen
}

function deployAzureFunction{
    param(
        $zipArchiveFullPath,
        $subscription,
        $resourceGroup,        
        $appName
    )    

    log "deploying '$($appName)' to Resource Group '$($resourceGroup)' in Subscription '$($subscription)' from zip '$($zipArchiveFullPath)' ..."
    az functionapp deployment source config-zip -g "$($resourceGroup)" -n "$($appName)" --src "$($zipArchiveFullPath)" --subscription "$($subscription)"   
}

Function CreateAppRole([string] $RoleName, [string] $Description, [string] $RoleValue)
{
    $appRole = New-Object Microsoft.Open.AzureAD.Model.AppRole
    $appRole.AllowedMemberTypes = New-Object System.Collections.Generic.List[string]
    $appRole.AllowedMemberTypes.Add("User");
    $appRole.AllowedMemberTypes.Add("Application");
    $appRole.DisplayName = $RoleName
    $appRole.Id = New-Guid
    $appRole.IsEnabled = $true
    $appRole.Description = $Description
    $appRole.Value = $RoleValue;
    return $appRole
}
