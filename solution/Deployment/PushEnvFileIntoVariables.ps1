


function ParseEnv([string]$Json, [string]$NamePrefix)
{   
    foreach($p in ($Json | ConvertFrom-Json).psobject.properties.where({$_.MemberType -eq "NoteProperty"}))
    { 
        $Name = $p.Name
        Write-Host "Parsing $($Name)"
        if($NamePrefix -ne "")
        {
            Write-Host "Prefix is $NamePrefix"
            $Name =  $NamePrefix + "_" + $p.Name
        } 
        $Value =  $p.Value   
        if($p.TypeNameOfValue -ne "System.Management.Automation.PSCustomObject")
        {
            Write-Host "Writing $Name to env file"
            echo "$Name=$Value" | Out-File -FilePath $Env:GITHUB_ENV -Encoding utf8 -Append
        }
        else {
            Write-Host "Further Parsing of $Name required"
            $JsonString = $p.Value | ConvertTo-Json
            ParseEnv -Json $JsonString -NamePrefix $Name
        }
    }
}

if ($null -eq $Env:GITHUB_ENV) 
{
    $Env:GITHUB_ENV="./bin/GitEnv.txt"
}

$Json = Get-Content -Path "./environments/development.json"  | Out-String
ParseEnv -Json $Json -NamePrefix ""

