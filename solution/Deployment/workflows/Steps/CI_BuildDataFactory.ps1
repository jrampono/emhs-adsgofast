#Move From Workflows to Function App
$CurrentPath = (Get-Location).Path
Set-Location "..\..\DataFactory"
Copy-Item -Path ".\*" -Destination "..\Deployment\bin\publish\unzipped\datafactory\"  -PassThru -Force -Recurse
#Move back to workflows 
Set-Location $CurrentPath
Set-Location "../bin/publish"
$Path = (Get-Location).Path + "/zipped/datafactory" 
New-Item -ItemType Directory -Force -Path $Path
$Path = $Path + "/Publish.zip"
Compress-Archive -Path '.\unzipped\datafactory\*' -DestinationPath $Path -force
#Move back to workflows 
Set-Location $CurrentPath


