$ErrorActionPreference = "Stop"

Write-Host "NexX Sensi - build corrigido para erro CET" -ForegroundColor Cyan
Write-Host "Verificando SDKs instalados..." -ForegroundColor DarkCyan
$sdks = dotnet --list-sdks
Write-Host $sdks
if (-not ($sdks -match '^8\.')) {
    Write-Host "" -ForegroundColor Red
    Write-Host "ERRO: .NET 8 SDK nao encontrado." -ForegroundColor Red
    Write-Host "Instale o .NET 8 SDK x64 e abra o projeto novamente." -ForegroundColor Yellow
    Write-Host "Download: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Yellow
    exit 1
}

Write-Host "Limpando bin/obj..." -ForegroundColor DarkCyan
Remove-Item -Recurse -Force .\bin, .\obj -ErrorAction SilentlyContinue

Write-Host "Limpando cache NuGet local..." -ForegroundColor DarkCyan
dotnet nuget locals all --clear

Write-Host "SDK selecionado pelo global.json:" -ForegroundColor DarkCyan
dotnet --version

Write-Host "Restaurando pacotes via nuget.org..." -ForegroundColor Cyan
dotnet restore .\NexXSensi.WinUI3.csproj --source https://api.nuget.org/v3/index.json -r win-x64 -p:CETCompat=false -p:UseSharedCompilation=false

Write-Host "Compilando NexX Sensi Release x64..." -ForegroundColor Cyan
dotnet build .\NexXSensi.WinUI3.csproj -c Release -r win-x64 --no-restore -p:CETCompat=false -p:UseSharedCompilation=false -p:PlatformTarget=x64

Write-Host "Concluido. Verifique a pasta bin\x64\Release." -ForegroundColor Green
