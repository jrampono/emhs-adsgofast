function LocalInstalls
{
    $ProgressPreference = 'SilentlyContinue'
    $AzInstalled = Get-InstalledModule Az
    if($null -eq $AzInstalled)
    {
        write-host "Installing Az Powershell Module"
        Install-Module -Name Az -Scope CurrentUser -Repository PSGallery -Force
    }
    $SqlInstalled = Get-InstalledModule SqlServer
    if($null -eq $SqlInstalled)
    {
        write-host "Installing SqlServer Module"
        Install-Module -Name SqlServer 
    }
    
    # Az Cli Install
    if($env:PerformLocalInstallsAzCli -eq $true)
    {
        write-host "Installing Azure CLI"
        Invoke-WebRequest -Uri https://aka.ms/installazurecliwindows -OutFile .\AzureCLI.msi; Start-Process msiexec.exe -Wait -ArgumentList '/I AzureCLI.msi /quiet'
    }

    if($env:PerformLocalInstallsAzCliAddToPath -eq $true)
    {
        write-host "Adding Azure CLI to Path"
        $env:PATH = $env:PATH + ";C:\Program Files (x86)\Microsoft SDKs\Azure\CLI2\wbin"    
    }

    
    # Note AzureAD needs to be installed into PS 5.1
    #Install-Module AzureAD
    

    Set-ExecutionPolicy -Scope Process -ExecutionPolicy Bypass -Force
}