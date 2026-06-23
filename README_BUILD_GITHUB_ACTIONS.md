# Compilar sem Visual Studio usando GitHub Actions

1. Crie um repositório no GitHub.
2. Envie todos os arquivos desta pasta `NexXSensi.WinUI3` para o repositório.
3. No GitHub, abra a aba **Actions**.
4. Selecione **Build NexX Sensi WinUI 3**.
5. Clique em **Run workflow**.
6. Quando terminar, baixe o artefato **NexX-Sensi-win-x64**.

O workflow usa `windows-latest`, instala .NET 8, restaura pelo NuGet.org e publica em Release x64.
