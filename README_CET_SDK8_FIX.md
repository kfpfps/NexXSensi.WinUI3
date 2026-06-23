# Correção do erro CET no Visual Studio

Erro visto:

`Your Windows doesn't fully support CET. Please install all available Windows updates.`

Neste caso o erro aparece durante a compilação em `Microsoft.CSharp.CurrentVersion.targets`, então ele normalmente ocorre antes do app rodar. Isso costuma acontecer quando o Visual Studio/.NET está usando SDK/compilador mais novo com CET em um Windows 10/Windows sem os patches necessários.

Correções aplicadas neste projeto:

- `global.json` fixa o SDK em .NET 8 com `rollForward: latestFeature`.
- `CETCompat=false` no `.csproj` e no `Directory.Build.props`.
- `UseSharedCompilation=false` para evitar o servidor de compilação compartilhado do C#.
- projeto limitado a `x64` / `win-x64`.
- build script limpa `bin`, `obj` e cache NuGet antes de compilar.

Requisitos:

1. Instale o .NET 8 SDK x64.
2. No Visual Studio, selecione Release | x64.
3. Feche o Visual Studio, apague `bin` e `obj` se existirem.
4. Abra `NexXSensi.WinUI3.sln` novamente.
5. Restaure os pacotes NuGet.
6. Compile.

Alternativa: execute `build_cet_fix.cmd`.

Se o erro continuar mesmo com .NET 8 SDK, instale todas as atualizações do Windows. Em Windows 10 muito desatualizado/LTSC antigo, a correção definitiva pode ser atualizar o sistema.
