param([Parameter(Mandatory=$true)][string]$InstallRoot)
$ErrorActionPreference = 'Stop'
$checkedRoot = [IO.Path]::GetFullPath($InstallRoot).TrimEnd('\')
$expectedRoot = [IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA 'YouTubeBackground'))
if ($checkedRoot -ne $expectedRoot) { throw 'Pasta de instalacao inesperada.' }
Get-Process -Name 'YouTubeBackground','YouTubeBackground.Host' -ErrorAction SilentlyContinue | ForEach-Object {
    if ($_.Path -and ([IO.Path]::GetFullPath($_.Path)).StartsWith($checkedRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        $owned = $_
        Stop-Process -Id $owned.Id -Force
        if (-not $owned.WaitForExit(10000)) { throw 'O aplicativo ainda esta em uso.' }
    }
}
