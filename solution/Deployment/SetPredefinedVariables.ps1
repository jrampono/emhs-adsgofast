function SetPredefinedVariables
{
    $env:DeploymentServicePrincipalName = "AdsGoFastDeployer"
    $env:TemplateFile = "$env:RootFolder/azuredeploy.json"
    $env:TemplateParametersFile = " $env:RootFolder/azuredeploy.parameters.json"
    $env:ADFIRDownloadURL='https://download.microsoft.com/download/E/4/7/E4771905-1079-445B-8BF9-8A1A075D8A10/IntegrationRuntime_5.3.7720.1.msi'
    $env:ADFLocalVMFolder='Temp'
    $env:ADFIRLocalFileLocation="C:\ $env:ADFLocalVMFolder\IntegrationRuntime_5.3.7720.1.msi"
    $env:ADFIRInstallerDownloadURL='https://raw.githubusercontent.com/nabhishek/SelfHosted-IntegrationRuntime_AutomationScripts/master/InstallGatewayOnLocalMachine.ps1'
    $env:ADFIRInstallerLocalFileLocation="C:\$env:ADFLocalVMFolder\InstallGatewayOnLocalMachine.ps1"
    $env:ADFJDKDownloadURL='https://github.com/AdoptOpenJDK/openjdk15-binaries/releases/download/jdk-15.0.2%2B7/OpenJDK15U-jre_x64_windows_hotspot_15.0.2_7.msi'
    $env:ADFJDKInstallerLocationFile="C:\$env:ADFLocalVMFolder\OpenJDK15U-jre_x64_windows_hotspot_15.0.2_7.msi"
    $env:webappZipFile="$env:RootFolder/webapp/Publish.zip"
    $env:ServicePrincipalNameWebWithHttp="http://$env:ServicePrincipalNameWeb"
    $env:AppSettingsFile="$env:RootFolder\SampleAppSettings.json"
}