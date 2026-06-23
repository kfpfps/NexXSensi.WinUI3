# NexX Sensi — correção e renomeação

Este pacote foi renomeado de NexXOtimization.WinUI3 para NexXSensi.WinUI3.

Correções incluídas:

- Solução: `NexXSensi.WinUI3.sln`
- Projeto: `NexXSensi.WinUI3.csproj`
- Namespace C#: `NexXSensi`
- Título do app: `NexX Sensi`
- Assembly/Product/ApplicationTitle atualizados
- Correção CET mantida: `<CETCompat>false</CETCompat>`
- `RuntimeIdentifier` reduzido para `win-x64` para evitar restauração desnecessária de x86/arm64
- `NuGet.config` apontando para `nuget.org`
- `build.ps1` atualizado para restore/build x64

## Como compilar

No PowerShell, dentro desta pasta:

```powershell
.\build.ps1
```

Ou no Visual Studio:

1. Abra `NexXSensi.WinUI3.sln`.
2. Selecione `Release | x64`.
3. Restaure os pacotes NuGet.
4. Compile/Recompile a solução.

Se aparecer erro CET novamente, apague `bin` e `obj` e compile outra vez.
