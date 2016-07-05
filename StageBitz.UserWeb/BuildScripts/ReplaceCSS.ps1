$Dir = $args[0]
$Ver = (get-date).ticks
$VerParam = '?v=' + $Ver

Foreach ($NewFile in  Get-ChildItem -Path  $Dir -Include @("*.css")  -recurse)
{
 If($NewFile.Attributes -eq "Directory") {Continue;}
 (Get-Content $NewFile) | % {$_ -replace '\?v=[\d]{18}', $VerParam} | Set-Content -path $NewFile -Encoding UTF8 -Force
}
