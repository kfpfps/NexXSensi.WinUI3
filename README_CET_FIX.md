# Correção do erro CET

Erro corrigido:

`Your Windows doesn't fully support CET. Please install all available Windows updates.`

Foram adicionados:

```xml
<CETCompat>false</CETCompat>
```

em `NexXSensi.WinUI3.csproj` e também em `Directory.Build.props`.

Compile em Release/x64. Se ainda aparecer o erro, limpe `bin` e `obj`, feche o Visual Studio e abra a solução novamente.
