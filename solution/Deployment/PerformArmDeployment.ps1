#############################################################
# Create Resource Group and Deploy ARM Template
#############################################################
Write-Host "Creating Resource Group"
$resoucegroup = az group create --name $ResourceGroupName --location $Location
Write-Host "Deploying ARM Template"
$output = az deployment group create --name "AdsGoFastDeployment" --resource-group $ResourceGroupName --template-file $TemplateFile --parameters $TemplateParametersFile

#Create Deployment Service Principal
$DeploymentServicePrincipalPassword = az ad sp create-for-rbac --name $DeploymentServicePrincipalName --query password --out tsv

New-AzRoleAssignment -RoleDefinitionName Reader -ServicePrincipalName $sp.ApplicationId