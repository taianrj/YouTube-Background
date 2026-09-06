$ErrorActionPreference = 'Stop'
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
Push-Location $PSScriptRoot
try {
    node --test tests/core.test.cjs tests/content.test.cjs
    if ($LASTEXITCODE -ne 0) { throw 'Testes JavaScript falharam.' }
    dotnet run --project tests/ProtocolTests.csproj -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Testes do protocolo falharam.' }
    dotnet publish src/App/App.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o package/app --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build do aplicativo falhou.' }
    dotnet publish src/Host/Host.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o package/host --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Build do auxiliar falhou.' }
    Copy-Item -LiteralPath extension -Destination package -Recurse -Force
    foreach ($name in @('install.ps1','uninstall.ps1','Instalar.cmd','Desinstalar.cmd','LEIA-ME.md','VALIDACAO.md')) { Copy-Item -LiteralPath $name -Destination package -Force }
    Write-Host 'Pacote pronto na pasta package.'
} finally { Pop-Location }
