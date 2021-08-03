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

function Get-UniqueString ([string]$id, $length=13)
{
$hashArray = (new-object System.Security.Cryptography.SHA512Managed).ComputeHash($id.ToCharArray())
-join ($hashArray[1..$length] | ForEach-Object { [char]($_ % 26 + [byte][char]'a') })
}

function SignInToAzureCli
{
    #############################################################
    # Sign-In to Azure
    #############################################################
    #Az Login
    $loginstatus = az account get-access-token
    if ($loginstatus -eq $null) {
        Write-Host "Az Not Logged In so attempting login"
        if($env:UseInteractiveAzCliLogin)
        {
            az Login
        }
        else {
            az login --service-principal --username $env:servicePrincipalId --password $env:servicePrincipalKey --tenant $env:tenantId
        }
    }

    az account set --subscription $env:Subscription
    Write-Host "Sign-In Completed"

    $env:tenantId = az account show --subscription $env:Subscription --query tenantId --output tsv
    $env:SubcriptionId=az account show --subscription $env:Subscription --query id --output tsv

}

function SignInToAzureAD
{
    Import-Module AzureAD -UseWindowsPowerShell
    $loginstatus = Get-AzureADTenantDetail
    if ($loginstatus -eq $null) {
        Write-Host "AzureAD Not Logged In so attempting login"
        if($env:UseInteractiveAzCliLogin)
        {
            Connect-AzureAD -TenantId $env:tenantId
        }
        else {
            Write-Error "AzureAD Service Principal Login is yet to be implemented in this deployment script."
        }
    }

    Write-Host "AzureAD Sign-In Completed"

}


function SignInToPowershellAz
{
    $loginstatus = Select-AzSubscription -SubscriptionId $env:SubcriptionId
    if ($loginstatus -eq $null) {
        Write-Host "Powershell Az Not Logged In so attempting login"
        if($env:UseInteractiveAzCliLogin)
        {
            Connect-AzAccount -TenantId $env:tenantId
            Select-AzSubscription -SubscriptionId $env:SubcriptionId
        }
        else {
            Write-Error "Powershell Az Service Principal Login is yet to be implemented in this deployment script."
        }
    }

    Write-Host "Powershell Az Sign-In Completed"

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
    Import-Module AzureADPreview
    $appRole = New-Object Microsoft.Open.AzureAD.Model.AppRole
    $appRole.AllowedMemberTypes = New-Object System.Collections.Generic.List[string]
    $appRole.AllowedMemberTypes.Add("Application");
    $appRole.DisplayName = $RoleName
    $appRole.Id = New-Guid
    $appRole.IsEnabled = $true
    $appRole.Description = $Description
    $appRole.Value = $RoleValue;
    return $appRole
}
