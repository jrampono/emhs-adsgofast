

function Generate($IR, $targetdir)
{
    
    Get-ChildItem -Path ".\Templates\$targetdir" -Filter "*.json" |
    ForEach-Object {
        $targetfile = ".\ADF\$targetdir" + "\" + ($_.BaseName).Replace("{IR}",$IR) + ".json"
        $fileName = $_.FullName
        $jsonobject = ($_ | Get-Content).Replace("@GF{IR}",$IR)    
        $jsonobject | set-content $targetfile
        
    }
}

$Irc =  Get-Content .\IRConfig.json | ConvertFrom-Json
foreach($Ir in $Irc.IRList) 
{
    if ($Ir.Enable)
    {
        Write-Output ("Generating Files for Integration Runtime: " + $Ir.ADFPostFix)
        Generate -IR $Ir.ADFPostFix -targetdir "linkedService"
        Generate -IR $Ir.ADFPostFix -targetdir "dataset"
        Generate -IR $Ir.ADFPostFix -targetdir "pipeline"
    }
}



