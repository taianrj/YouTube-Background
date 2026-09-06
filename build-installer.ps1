param([string]$Compiler = 'ISCC.exe', [string]$OutputDirectory = (Join-Path $PSScriptRoot 'dist'))
$ErrorActionPreference = 'Stop'
$resolvedOutput = [IO.Path]::GetFullPath($OutputDirectory)
New-Item -ItemType Directory -Path $resolvedOutput -Force | Out-Null
& $Compiler ('/DReleaseDir=' + $resolvedOutput) (Join-Path $PSScriptRoot 'setup\YouTubeBackground.iss')
if ($LASTEXITCODE -ne 0) { throw 'Falha ao compilar o instalador.' }
