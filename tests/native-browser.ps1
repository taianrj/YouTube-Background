$ErrorActionPreference = 'Stop'
$sourceRoot = Split-Path -Parent $PSScriptRoot
$testKey = 'HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.youtubebackground.host'
if (Test-Path -LiteralPath $testKey) { throw 'Uma instalacao existente foi detectada. Este teste nao altera instalacoes existentes.' }
if (Get-Process -Name YouTubeBackground -ErrorAction SilentlyContinue) { throw 'Feche o aplicativo antes deste teste.' }
dotnet build (Join-Path $PSScriptRoot 'Integration\Integration.csproj') --nologo
if ($LASTEXITCODE -ne 0) { throw 'Build do disparador de teste falhou.' }
$tempManifest = Join-Path $sourceRoot 'package\test-native-host.json'
$manifest = @{ name = 'com.youtubebackground.host'; description = 'Temporary integration test'; path = (Join-Path $sourceRoot 'package\host\YouTubeBackground.Host.exe'); type = 'stdio'; allowed_origins = @('chrome-extension://jobnoaknnkhgmfjfbfkelbpfgabdogfb/') }
[IO.File]::WriteAllText($tempManifest, ($manifest | ConvertTo-Json), (New-Object Text.UTF8Encoding($false)))
$testApp = $null
try {
    New-Item -Path $testKey -Force | Out-Null
    Set-Item -Path $testKey -Value $tempManifest
    $testApp = Start-Process -FilePath (Join-Path $sourceRoot 'package\app\YouTubeBackground.exe') -WindowStyle Hidden -PassThru
    node (Join-Path $PSScriptRoot 'browser-smoke.cjs') (Join-Path $sourceRoot 'browser-test-profile') --live --native
    if ($LASTEXITCODE -ne 0) { throw 'Teste de integracao real falhou.' }
} finally {
    if ($testApp -and -not $testApp.HasExited) { Stop-Process -Id $testApp.Id -Force }
    Remove-Item -LiteralPath $testKey -Force -ErrorAction SilentlyContinue
    Remove-Item -LiteralPath $tempManifest -Force -ErrorAction SilentlyContinue
}
