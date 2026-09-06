param([switch]$KeepSettings)
$ErrorActionPreference = 'Stop'
$localRoot = [IO.Path]::GetFullPath($env:LOCALAPPDATA)
$installRoot = [IO.Path]::GetFullPath((Join-Path $localRoot 'YouTubeBackground'))
# Only this exact, fixed application directory may be recursively deleted.
if ((Split-Path -Parent $installRoot) -ne $localRoot -or (Split-Path -Leaf $installRoot) -ne 'YouTubeBackground') { throw 'Diretorio de remocao invalido.' }
Get-Process -Name 'YouTubeBackground','YouTubeBackground.Host' -ErrorAction SilentlyContinue | ForEach-Object {
    if ($_.Path -and ([IO.Path]::GetFullPath($_.Path)).StartsWith($installRoot + '\', [StringComparison]::OrdinalIgnoreCase)) {
        $appProcess = $_
        Stop-Process -Id $appProcess.Id -Force
        if (-not $appProcess.WaitForExit(10000)) { throw 'O aplicativo nao encerrou. Feche-o e execute novamente a remocao.' }
    }
}
Remove-Item -LiteralPath 'HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.youtubebackground.host' -Recurse -Force -ErrorAction SilentlyContinue
Remove-ItemProperty -LiteralPath 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Run' -Name 'YouTubeBackground' -ErrorAction SilentlyContinue
Remove-Item -LiteralPath (Join-Path ([Environment]::GetFolderPath('Programs')) 'YouTube Background.lnk') -Force -ErrorAction SilentlyContinue
if (Test-Path -LiteralPath $installRoot) {
    $entry = Get-Item -LiteralPath $installRoot
    if ($entry.Attributes -band [IO.FileAttributes]::ReparsePoint) { throw 'A pasta instalada e um link; remocao automatica interrompida.' }
    if ($KeepSettings) {
        foreach ($entry in Get-ChildItem -LiteralPath $installRoot -Force) {
            if ($entry.Name -ne 'settings.json') {
                $checkedPath = [IO.Path]::GetFullPath($entry.FullName)
                if (-not $checkedPath.StartsWith($installRoot + '\', [StringComparison]::OrdinalIgnoreCase)) { throw 'Caminho fora da pasta instalada.' }
                Remove-Item -LiteralPath $checkedPath -Recurse -Force
            }
        }
    } else { Remove-Item -LiteralPath $installRoot -Recurse -Force }
}
Write-Host 'Aplicativo removido. Remova tambem a extensao em chrome://extensions.'
