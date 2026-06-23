# NexX Sensi — WinUI 3 C#

Conversão do painel Python/CustomTkinter para C# com WinUI 3 / Windows App SDK.

## Requisitos

- Windows 10/11.
- Visual Studio 2022/2026 com workload de desenvolvimento desktop Windows.
- .NET 8 SDK ou superior.
- Windows App SDK restaurado via NuGet.

O projeto usa `Microsoft.WindowsAppSDK` `2.2.0` e está configurado como app WinUI 3 **unpackaged** com `WindowsAppSDKSelfContained=true`.

## Como abrir

1. Extraia este ZIP.
2. Abra `NexXSensi.WinUI3.sln` no Visual Studio.
3. Selecione a plataforma `x64`.
4. Compile em `Release`.

Também pode compilar por terminal:

```powershell
dotnet restore .\NexXSensi.WinUI3.csproj
dotnet build .\NexXSensi.WinUI3.csproj -c Release -r win-x64
```

## Pasta Scripts

O app mantém o mesmo backend lógico do Python: `.bat`, `.reg`, `powercfg`, limpeza e reparos.

Coloque a pasta `Scripts` ao lado do `.exe` final, mantendo a estrutura original:

```text
Scripts/
  01_Otimizacao_Windows/
  02_Servicos/
  03_Perifericos/
  04_GPU_e_Jogos/
  05_Plano_de_Energia/
  07_Reparos/
  08_Recursos_do_Windows/
```

O programa também procura `Scripts` no diretório de execução atual caso não exista ao lado do executável.

## Elevação/UAC

A janela abre sem pedir administrador. Cada ação que precisa modificar o sistema chama `PowerShell Start-Process -Verb RunAs`, seguindo a lógica do Python original.

## O que foi convertido

- Interface principal escura com WinUI 3.
- Sidebar com categorias.
- Cards e linhas internas arredondadas.
- Botões com animação de hover/click.
- Animação leve de brilho nas bordas internas.
- Logo embutido em `Assets/logo.png`.
- Ícones por `Segoe Fluent Icons`, sem caixas e sem bordas.
- Execução de `.bat`, `.reg`, comandos CMD, limpeza, energia e reparos.


## Correção NU1101 / pacote não encontrado

Se o Visual Studio mostrar que não encontra `Microsoft.WindowsAppSDK`, `Microsoft.NETCore.App.Runtime` ou `Microsoft.Windows.SDK.NET.Ref` e listar apenas `Microsoft Visual Studio Offline Packages`, o NuGet está sem a fonte online.

Este projeto inclui `NuGet.config` apontando para:

```text
https://api.nuget.org/v3/index.json
```

No Visual Studio, também confira em **Ferramentas > Opções > Gerenciador de Pacotes NuGet > Origens do Pacote** se `nuget.org` está habilitado.

Depois rode:

```powershell
dotnet nuget locals all --clear
dotnet restore .\NexXSensi.WinUI3.csproj --source https://api.nuget.org/v3/index.json
dotnet build .\NexXSensi.WinUI3.csproj -c Release -r win-x64 --no-restore
```


## Erro CET

Se aparecer `Your Windows doesn't fully support CET`, leia `README_CET_SDK8_FIX.md` e use `build_cet_fix.cmd`.
