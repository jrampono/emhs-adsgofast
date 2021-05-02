


function ParseEnvFragment([string]$Json, [string]$NamePrefix)
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
            #Push Variables to the GitHub Actions Compatible Store
            Write-Host "Writing $Name to env file"
            echo "$Name=$Value" | Out-File -FilePath $Env:GITHUB_ENV -Encoding utf8 -Append
            #Also Push Variables to the Session Env Variables for local testing
            [Environment]::SetEnvironmentVariable($Name, "$Value")
        }
        else {
            Write-Host "Further Parsing of $Name required"
            $JsonString = $p.Value | ConvertTo-Json
            ParseEnvFragment -Json $JsonString -NamePrefix $Name
        }
    }
}


function ParseEnvFile ($EnvFile)
{
    if ($null -eq $Env:GITHUB_ENV) 
    {
        [Environment]::SetEnvironmentVariable("GITHUB_ENV","..\bin\GitEnv.txt")
        $FileNameOnly = Split-Path $env:GITHUB_ENV -leaf
        $PathOnly =  Split-Path $env:GITHUB_ENV
        if ((Test-Path $env:GITHUB_ENV))
        {      
        #    Remove-Item -Path $env:GITHUB_ENV
        }
        
        
        New-Item -Path $PathOnly -Name $FileNameOnly -type "file" -value ""
        
    }

    $Json = Get-Content -Path "..\environments\$($EnvFile).json"  | Out-String
    ParseEnvFragment -Json $Json -NamePrefix ""

    

}
