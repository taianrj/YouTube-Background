$ErrorActionPreference = 'Stop'
$installRoot = [IO.Path]::GetFullPath((Join-Path $env:LOCALAPPDATA 'YouTubeBackground'))
$packageRoot = $PSScriptRoot
if (-not (Test-Path -LiteralPath (Join-Path $packageRoot 'app\YouTubeBackground.exe'))) { throw 'Executavel nao encontrado. Use o ZIP Windows-x64 (programa pronto), nao o ZIP Codigo-Fonte. Extraia todo o pacote antes de instalar.' }
if (-not (Test-Path -LiteralPath (Join-Path $packageRoot 'host\YouTubeBackground.Host.exe'))) { throw 'Executavel auxiliar ausente. Extraia todo o ZIP.' }
if ($installRoot -eq [IO.Path]::GetFullPath($packageRoot)) { throw 'Execute a instalacao a partir do pacote extraido, fora da pasta instalada.' }
New-Item -ItemType Directory -Force -Path $installRoot | Out-Null
Get-Process -Name 'YouTubeBackground','YouTubeBackground.Host' -ErrorAction SilentlyContinue | ForEach-Object {
    if ($_.Path -and ([IO.Path]::GetFullPath($_.Path)).StartsWith($installRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        $appProcess = $_
        Stop-Process -Id $appProcess.Id -Force
        if (-not $appProcess.WaitForExit(10000)) { throw 'O aplicativo nao encerrou. Feche-o e execute novamente o instalador.' }
    }
}
foreach ($folder in @('app','host','extension')) { Copy-Item -LiteralPath (Join-Path $packageRoot $folder) -Destination $installRoot -Recurse -Force }
Copy-Item -LiteralPath (Join-Path $packageRoot 'uninstall.ps1') -Destination $installRoot -Force
$extensionManifest = Get-Content -LiteralPath (Join-Path $installRoot 'extension\manifest.json') -Raw | ConvertFrom-Json
$hash = [Security.Cryptography.SHA256]::Create().ComputeHash([Convert]::FromBase64String($extensionManifest.key))
$extensionId = -join ($hash[0..15] | ForEach-Object { [char](97 + ($_ -shr 4)); [char](97 + ($_ -band 15)) })
$hostManifest = @{
    name = 'com.youtubebackground.host'
    description = 'YouTube Background Native Messaging Host'
    path = (Join-Path $installRoot 'host\YouTubeBackground.Host.exe')
    type = 'stdio'
    allowed_origins = @("chrome-extension://$extensionId/")
}
$manifestPath = Join-Path $installRoot 'native-host.json'
[IO.File]::WriteAllText($manifestPath, ($hostManifest | ConvertTo-Json), (New-Object Text.UTF8Encoding($false)))
$registryPath = 'HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.youtubebackground.host'
New-Item -Path $registryPath -Force | Out-Null
Set-Item -Path $registryPath -Value $manifestPath
$startupEnabled = $true
$settingsPath = Join-Path $installRoot 'settings.json'
if (Test-Path -LiteralPath $settingsPath) { try { $startupEnabled = (Get-Content -LiteralPath $settingsPath -Raw | ConvertFrom-Json).Startup -ne $false } catch {} }
$runPath = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run'
New-Item -Path $runPath -Force | Out-Null
$appPath = Join-Path $installRoot 'app\YouTubeBackground.exe'
if ($startupEnabled) { New-ItemProperty -Path $runPath -Name 'YouTubeBackground' -Value ('"' + $appPath + '"') -PropertyType String -Force | Out-Null }
$shellObject = New-Object -ComObject WScript.Shell
$shortcutPath = Join-Path ([Environment]::GetFolderPath('Programs')) 'YouTube Background.lnk'
$shortcut = $shellObject.CreateShortcut($shortcutPath)
$shortcut.TargetPath = $appPath
$shortcut.Arguments = '--settings'
$shortcut.Save()
Start-Process -FilePath $appPath -WindowStyle Hidden
Write-Host "Instalado em: $installRoot"
Write-Host 'No Chrome, abra chrome://extensions, ative Modo do desenvolvedor e clique em Carregar sem compactacao.'
Write-Host ('Selecione: ' + (Join-Path $installRoot 'extension'))
Write-Host "ID esperado da extensao: $extensionId"
Write-Host 'Recarregue as abas do YouTube que ja estavam abertas.'
