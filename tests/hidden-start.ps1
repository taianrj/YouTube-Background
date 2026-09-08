param([string]$Package = (Join-Path (Split-Path -Parent $PSScriptRoot) 'package'))
$ErrorActionPreference = 'Stop'
if (Get-Process -Name YouTubeBackground -ErrorAction SilentlyContinue) { throw 'Feche o aplicativo instalado antes de testar.' }
$exe = Join-Path $Package 'app\YouTubeBackground.exe'
$ownedProcess = Start-Process -FilePath $exe -WindowStyle Hidden -PassThru
try {
    $request = Start-Process -FilePath $exe -ArgumentList '--settings' -WindowStyle Hidden -PassThru
    $request.WaitForExit(8000) | Out-Null
    $visible = $false
    for ($attempt = 0; $attempt -lt 50; $attempt++) {
        $ownedProcess.Refresh()
        if ($ownedProcess.MainWindowTitle -match '^YouTube Background — (Configurações|Settings|Ajustes)$') { $visible = $true; break }
        Start-Sleep -Milliseconds 100
    }
    if (-not $visible) { throw 'A primeira janela ficou oculta apos inicializacao SW_HIDE.' }
    Write-Output 'PASS: primeira janela visivel apos inicializacao oculta e solicitacao imediata.'
} finally { if (-not $ownedProcess.HasExited) { Stop-Process -Id $ownedProcess.Id -Force } }
