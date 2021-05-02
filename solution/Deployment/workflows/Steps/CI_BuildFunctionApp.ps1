$CurrentPath = (Get-Location).Path
Set-Location "..\..\FunctionApp"
dotnet publish --no-restore --configuration Release --output '..\Deployment\bin\publish\unzipped\functionapp\'
Set-Location $CurrentPath