using System.Diagnostics;
using System.Security.Principal;
using System.Text;
using NexXSensi.Models;

namespace NexXSensi.Services;

public static class ScriptRunner
{
    private const string PowerPlanGuid = "7f2f5e11-0b57-4b23-a98d-6f5f6b2d4a75";
    private const string PowerPlanName = "NexX Sensi";

    public static readonly string AppDir = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
    public static string ScriptsDir
    {
        get
        {
            var local = Path.Combine(AppDir, "Scripts");
            if (Directory.Exists(local)) return local;
            return Path.Combine(Directory.GetCurrentDirectory(), "Scripts");
        }
    }

    public static string Script(string relative)
    {
        var normalized = relative.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        return Path.Combine(ScriptsDir, normalized);
    }

    public static bool IsWindows => OperatingSystem.IsWindows();

    public static bool IsAdministrator()
    {
        if (!IsWindows) return false;
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    public static Task<ActionResult> RunBatFileAsync(string path, bool forceSuccess = true)
    {
        if (!File.Exists(path)) return Task.FromResult(ActionResult.Fail($"Arquivo não encontrado: {path}"));
        var temp = SanitizeBatch(path, forceSuccess);
        return RunAndDeleteTempAsync(temp, () => RunProcessAsync("cmd.exe", new[] { "/c", temp }, Path.GetDirectoryName(path) ?? AppDir, elevate: true, forceSuccess: forceSuccess));
    }

    public static Task<ActionResult> RunRegFileAsync(string path, bool forceSuccess = false)
    {
        if (!File.Exists(path)) return Task.FromResult(ActionResult.Fail($"Arquivo não encontrado: {path}"));
        return RunProcessAsync("reg.exe", new[] { "import", path }, Path.GetDirectoryName(path) ?? AppDir, elevate: true, forceSuccess: forceSuccess);
    }

    public static Task<ActionResult> RunCmdLinesAsync(IEnumerable<string> commands, string? cwd = null, bool forceSuccess = false)
    {
        var temp = Path.Combine(Path.GetTempPath(), $"nexx_{Guid.NewGuid():N}.cmd");
        var lines = new List<string> { "@echo off" };
        lines.AddRange(commands);
        lines.Add(forceSuccess ? "exit /b 0" : "exit /b %ERRORLEVEL%");
        File.WriteAllLines(temp, lines, Encoding.UTF8);
        return RunAndDeleteTempAsync(temp, () => RunProcessAsync("cmd.exe", new[] { "/c", temp }, cwd ?? AppDir, elevate: true, forceSuccess: forceSuccess));
    }

    public static Task<ActionResult> CleanupTempFilesAsync() => RunCmdLinesAsync(new[]
    {
        "del /f /s /q \"%TEMP%\\*\" >nul 2>&1",
        "for /d %%D in (\"%TEMP%\\*\") do rd /s /q \"%%D\" >nul 2>&1",
        "del /f /s /q \"%WINDIR%\\Temp\\*\" >nul 2>&1",
        "for /d %%D in (\"%WINDIR%\\Temp\\*\") do rd /s /q \"%%D\" >nul 2>&1",
        "del /f /s /q \"%WINDIR%\\Prefetch\\*\" >nul 2>&1",
        "del /f /s /q \"%WINDIR%\\Logs\\*\" >nul 2>&1"
    }, forceSuccess: true);

    public static Task<ActionResult> CleanupWindowsUpdateCacheAsync() => RunCmdLinesAsync(new[]
    {
        "net stop wuauserv >nul 2>&1",
        "net stop bits >nul 2>&1",
        "del /f /s /q \"%WINDIR%\\SoftwareDistribution\\Download\\*\" >nul 2>&1",
        "for /d %%D in (\"%WINDIR%\\SoftwareDistribution\\Download\\*\") do rd /s /q \"%%D\" >nul 2>&1",
        "net start bits >nul 2>&1",
        "net start wuauserv >nul 2>&1"
    }, forceSuccess: true);

    public static Task<ActionResult> CleanupSimpleAsync() => RunCmdLinesAsync(new[]
    {
        "del /f /s /q \"%TEMP%\\*\" >nul 2>&1",
        "for /d %%D in (\"%TEMP%\\*\") do rd /s /q \"%%D\" >nul 2>&1",
        "del /f /s /q \"%WINDIR%\\Temp\\*\" >nul 2>&1",
        "del /f /s /q \"%WINDIR%\\Prefetch\\*\" >nul 2>&1",
        "del /f /s /q \"%APPDATA%\\Microsoft\\Windows\\Recent\\*\" >nul 2>&1"
    }, forceSuccess: true);

    public static Task<ActionResult> CleanupTotalAsync() => RunCmdLinesAsync(new[]
    {
        "taskkill /f /im chrome.exe >nul 2>&1",
        "taskkill /f /im msedge.exe >nul 2>&1",
        "taskkill /f /im brave.exe >nul 2>&1",
        "taskkill /f /im firefox.exe >nul 2>&1",
        "taskkill /f /im vivaldi.exe >nul 2>&1",
        "del /f /s /q \"%TEMP%\\*\" >nul 2>&1",
        "for /d %%D in (\"%TEMP%\\*\") do rd /s /q \"%%D\" >nul 2>&1",
        "del /f /s /q \"%WINDIR%\\Temp\\*\" >nul 2>&1",
        "for /d %%D in (\"%WINDIR%\\Temp\\*\") do rd /s /q \"%%D\" >nul 2>&1",
        "del /f /s /q \"%WINDIR%\\Prefetch\\*\" >nul 2>&1",
        "del /f /s /q \"%LOCALAPPDATA%\\Google\\Chrome\\User Data\\Default\\Cache\\*\" >nul 2>&1",
        "del /f /s /q \"%LOCALAPPDATA%\\Microsoft\\Edge\\User Data\\Default\\Cache\\*\" >nul 2>&1",
        "del /f /s /q \"%LOCALAPPDATA%\\BraveSoftware\\Brave-Browser\\User Data\\Default\\Cache\\*\" >nul 2>&1",
        "del /f /s /q \"%APPDATA%\\Mozilla\\Firefox\\Profiles\\*\\cache2\\entries\\*\" >nul 2>&1"
    }, forceSuccess: true);

    public static Task<ActionResult> ImportNexxPowerPlanAsync()
    {
        var powPath = Script(Path.Combine("05_Plano_de_Energia", "kfpznX_Otimization.pow"));
        if (!File.Exists(powPath)) return Task.FromResult(ActionResult.Fail($"Arquivo .pow não encontrado: {powPath}"));
        return RunCmdLinesAsync(new[]
        {
            "powercfg /setactive SCHEME_BALANCED >nul 2>&1",
            $"powercfg -delete {PowerPlanGuid} >nul 2>&1",
            $"powercfg /import \"{powPath}\" {PowerPlanGuid}",
            "if errorlevel 1 exit /b 1",
            $"powercfg /changename {PowerPlanGuid} \"{PowerPlanName}\" \"Plano de energia personalizado NexX Sensi\"",
            $"powercfg /setactive {PowerPlanGuid}",
            "if errorlevel 1 exit /b 1"
        }, cwd: Path.GetDirectoryName(powPath), forceSuccess: false);
    }

    private static async Task<ActionResult> RunAndDeleteTempAsync(string temp, Func<Task<ActionResult>> run)
    {
        try { return await run(); }
        finally
        {
            try { File.Delete(temp); } catch { }
        }
    }

    private static string SanitizeBatch(string srcPath, bool forceSuccess)
    {
        var bytes = File.ReadAllBytes(srcPath);
        string text;
        try { text = new UTF8Encoding(false, true).GetString(bytes); }
        catch { text = Encoding.Latin1.GetString(bytes); }

        var cleaned = new List<string> { "@echo off" };
        foreach (var original in text.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            var line = original.TrimEnd();
            var stripped = line.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(stripped)) continue;
            if (stripped is "pause" or "pause>nul" or "pause >nul") continue;
            if (stripped.StartsWith("timeout ") || stripped.StartsWith("title ") || stripped.StartsWith("color ")) continue;
            if (forceSuccess && (stripped.StartsWith("del ") || stripped.StartsWith("erase ") || stripped.StartsWith("rd ") || stripped.StartsWith("rmdir ")))
                cleaned.Add(line + " >nul 2>&1");
            else
                cleaned.Add(line);
        }
        cleaned.Add(forceSuccess ? "exit /b 0" : "exit /b %ERRORLEVEL%");

        var temp = Path.Combine(Path.GetTempPath(), $"nexx_{Guid.NewGuid():N}.cmd");
        File.WriteAllLines(temp, cleaned, Encoding.UTF8);
        return temp;
    }

    private static async Task<ActionResult> RunProcessAsync(string fileName, IEnumerable<string> args, string cwd, bool elevate, bool forceSuccess)
    {
        if (!IsWindows) return ActionResult.Fail("Este painel só executa funções no Windows.");

        try
        {
            if (elevate && !IsAdministrator())
            {
                var argList = string.Join(", ", args.Select(a => "'" + PsQuote(a) + "'"));
                var psCommand =
                    $"$p = Start-Process -FilePath '{PsQuote(fileName)}' " +
                    $"-ArgumentList @({argList}) " +
                    $"-WorkingDirectory '{PsQuote(cwd)}' " +
                    "-Verb RunAs -Wait -PassThru; " +
                    "if ($p.ExitCode -eq $null) { exit 0 } else { exit $p.ExitCode }";

                var result = await StartAndCaptureAsync("powershell.exe", new[] { "-NoProfile", "-ExecutionPolicy", "Bypass", "-Command", psCommand }, AppDir, false);
                var rc = forceSuccess && result.ExitCode != 1223 ? 0 : result.ExitCode;
                return rc == 0 ? ActionResult.Ok(result.Output) : ActionResult.Fail(result.Output);
            }

            var normal = await StartAndCaptureAsync(fileName, args, cwd, true);
            var code = forceSuccess ? 0 : normal.ExitCode;
            return code == 0 ? ActionResult.Ok(normal.Output) : ActionResult.Fail(normal.Output);
        }
        catch (Exception ex)
        {
            return ActionResult.Fail(ex.Message);
        }
    }

    private static async Task<(int ExitCode, string Output)> StartAndCaptureAsync(string fileName, IEnumerable<string> args, string cwd, bool noWindow)
    {
        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            WorkingDirectory = cwd,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = noWindow
        };
        foreach (var arg in args) psi.ArgumentList.Add(arg);

        using var process = Process.Start(psi) ?? throw new InvalidOperationException("Não foi possível iniciar o processo.");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        return (process.ExitCode, (await stdout) + (await stderr));
    }

    private static string PsQuote(string value) => value.Replace("'", "''");
}
