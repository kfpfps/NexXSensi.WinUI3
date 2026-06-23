using System.Numerics;
using Microsoft.UI;
using Microsoft.UI.Dispatching;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using NexXSensi.Models;
using NexXSensi.Services;
using Windows.Graphics;
using Windows.UI;
using Microsoft.UI.Text;
using WinRT.Interop;

namespace NexXSensi;

public sealed partial class MainWindow : Window
{
    private const string BG_ROOT = "#070B13";
    private const string BG_TOP = "#0A101C";
    private const string BG_SIDE = "#0A101C";
    private const string BG_MAIN = "#070B13";
    private const string BG_CARD = "#0D1422";
    private const string BG_CARD_HOVER = "#101A2C";
    private const string LINE = "#263A57";
    private const string LINE_SOFT = "#1D2F49";
    private const string CYAN = "#22D3EE";
    private const string CYAN_LIGHT = "#8BEFFF";
    private const string GREEN = "#34E8A1";
    private const string AMBER = "#FBBF53";
    private const string RED = "#FF6B7E";
    private const string PURPLE = "#A78BFA";
    private const string WHITE = "#EAF2FB";
    private const string TEXT_PRI = "#EAF2FB";
    private const string TEXT_MUT = "#8EA4C0";
    private const string TEXT_DIM = "#526A8C";

    private readonly Dictionary<string, Button> _navButtons = new();
    private readonly Dictionary<string, FrameworkElement> _pages = new();
    private ContentControl _pageHost = null!;
    private TextBlock _statusText = null!;
    private TextBlock _pathText = null!;
    private Border _mainShell = null!;
    private Border _topbar = null!;
    private Border _sidebar = null!;
    private Border _statusbar = null!;
    private DispatcherQueueTimer? _glowTimer;
    private double _glowPhase;

    public MainWindow()
    {
        InitializeComponent();
        Title = "NexX Sensi";
        BuildWindow();
        BuildUi();
        ShowPage("windows");
        StartBackgroundAnimation();
    }

    private void BuildWindow()
    {
        try
        {
            var hwnd = WindowNative.GetWindowHandle(this);
            var id = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(id);
            appWindow.Resize(new SizeInt32(1220, 780));
            appWindow.SetIcon(Path.Combine(AppContext.BaseDirectory, "Assets", "logo.ico"));
        }
        catch
        {
            // Visual Studio/WinUI fallback: window still opens even if AppWindow APIs fail.
        }
    }

    private void BuildUi()
    {
        Root.Background = Brush(BG_ROOT);

        _mainShell = new Border
        {
            Background = Brush(BG_ROOT),
            BorderBrush = Brush(LINE_SOFT),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(8),
            Margin = new Thickness(8)
        };
        Root.Children.Add(_mainShell);

        var shellGrid = new Grid();
        shellGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        shellGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        shellGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        _mainShell.Child = shellGrid;

        _topbar = BuildTopbar();
        Grid.SetRow(_topbar, 0);
        shellGrid.Children.Add(_topbar);

        var bodyGrid = new Grid { Margin = new Thickness(0, 8, 0, 8) };
        bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(236) });
        bodyGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetRow(bodyGrid, 1);
        shellGrid.Children.Add(bodyGrid);

        _sidebar = BuildSidebar();
        Grid.SetColumn(_sidebar, 0);
        bodyGrid.Children.Add(_sidebar);

        _pageHost = new ContentControl { Margin = new Thickness(8, 0, 0, 0) };
        Grid.SetColumn(_pageHost, 1);
        bodyGrid.Children.Add(_pageHost);

        BuildPages();

        _statusbar = BuildStatusbar();
        Grid.SetRow(_statusbar, 2);
        shellGrid.Children.Add(_statusbar);
    }

    private Border BuildTopbar()
    {
        var border = PanelBorder(BG_TOP, LINE_SOFT, 18, 2);
        border.Height = 104;
        border.Padding = new Thickness(16, 8, 16, 8);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        border.Child = grid;

        var left = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 14, VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(left, 0);
        grid.Children.Add(left);

        var logo = new Image
        {
            Width = 88,
            Height = 88,
            Stretch = Stretch.Uniform,
            Source = new BitmapImage(new Uri("ms-appx:///Assets/logo.png"))
        };
        left.Children.Add(logo);

        var brand = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        brand.Children.Add(Text("NexX Sensi", 24, WHITE, FontWeights.Bold));
        brand.Children.Add(Text("System Tuner · Windows 10/11", 13, TEXT_MUT));
        left.Children.Add(brand);

        var pills = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, VerticalAlignment = VerticalAlignment.Center };
        pills.Children.Add(Pill("● Sistema OK", GREEN, "#0C2629"));
        pills.Children.Add(Pill(ScriptRunner.IsAdministrator() ? "● ADMIN" : "● SEM ADMIN", ScriptRunner.IsAdministrator() ? CYAN : AMBER, ScriptRunner.IsAdministrator() ? "#0C2232" : "#201E1E"));
        Grid.SetColumn(pills, 2);
        grid.Children.Add(pills);

        return border;
    }

    private Border BuildSidebar()
    {
        var border = PanelBorder(BG_SIDE, LINE_SOFT, 18, 2);
        border.Padding = new Thickness(10, 14, 10, 14);

        var stack = new StackPanel { Spacing = 6 };
        border.Child = stack;
        stack.Children.Add(Text("CATEGORIAS", 12, TEXT_DIM, FontWeights.Bold, margin: new Thickness(8, 0, 0, 5)));

        AddNav(stack, "windows", "Windows", "windows");
        AddNav(stack, "servicos", "Serviços", "servicos");
        AddNav(stack, "perifericos", "Periféricos & GPU", "perifericos");
        AddNav(stack, "energia", "Energia", "energia");
        AddNav(stack, "limpeza", "Limpeza", "limpeza");
        AddNav(stack, "recursos", "Recursos", "recursos");
        AddNav(stack, "reparos", "Reparos", "reparos");

        return border;
    }

    private Border BuildStatusbar()
    {
        var border = PanelBorder("#080C16", LINE_SOFT, 18, 2);
        border.Height = 42;
        border.Padding = new Thickness(16, 0, 16, 0);

        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        border.Child = grid;

        _statusText = Text("● Scripts carregados.", 15, GREEN, FontWeights.Bold);
        _statusText.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_statusText, 0);
        grid.Children.Add(_statusText);

        var line = new Border { Height = 2, Background = Brush("#102238"), Margin = new Thickness(18, 0, 18, 0), VerticalAlignment = VerticalAlignment.Center, CornerRadius = new CornerRadius(2) };
        Grid.SetColumn(line, 1);
        grid.Children.Add(line);

        var scriptPath = ScriptRunner.ScriptsDir;
        if (scriptPath.Length > 92) scriptPath = "..." + scriptPath[^89..];
        _pathText = Text(scriptPath, 11, "#4F6885");
        _pathText.FontFamily = new FontFamily("Consolas");
        _pathText.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(_pathText, 2);
        grid.Children.Add(_pathText);

        return border;
    }

    private void AddNav(StackPanel stack, string key, string title, string iconKey)
    {
        var row = new Grid { ColumnSpacing = 10 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.Children.Add(Icon(iconKey, 24, TEXT_MUT));
        var label = Text(title, 15, TEXT_MUT, FontWeights.SemiBold);
        label.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(label, 1);
        row.Children.Add(label);

        var button = new Button
        {
            Content = row,
            Height = 50,
            HorizontalContentAlignment = HorizontalAlignment.Stretch,
            Padding = new Thickness(12, 0, 12, 0),
            Background = Brush(BG_SIDE),
            BorderBrush = Brush(BG_SIDE),
            BorderThickness = new Thickness(2),
            CornerRadius = new CornerRadius(15)
        };
        AnimateButton(button, BG_SIDE, BG_CARD_HOVER, BG_SIDE, LINE);
        button.Click += (_, _) => ShowPage(key);
        stack.Children.Add(button);
        _navButtons[key] = button;
    }

    private void BuildPages()
    {
        _pages["windows"] = Page(scroll =>
        {
            Banner(scroll, "windows", "Otimização do Sistema", "Pequenos ajustes para melhorar resposta, reduzir carga e manter o desempenho estável.", CYAN);
            Section(scroll, "Otimização principal");
            var grid = CardGrid(4);
            scroll.Children.Add(grid);
            AddFeature(grid, 0, new OptimizationAction("boost", "Boost Geral", "SFC + bcdedit + rede", "Alto", () => ScriptRunner.RunBatFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/otimizacao_sistema.bat"), true), CYAN, true));
            AddFeature(grid, 1, new OptimizationAction("memory", "Ajustes de Memória", "Prioridade, cache e pagefile", "Alto", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/Ajustes na memoria.reg")), GREEN));
            AddFeature(grid, 2, new OptimizationAction("system", "Sistema & Outros", "Power Throttling e prioridade", "Alto", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/03_Sistema_e_Outros.reg")), PURPLE));
            AddFeature(grid, 3, new OptimizationAction("advanced", "Otimização Avançada", "Rede, NDU, prefetch e energia", "Alto", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/04_Otimizacao_Avancada.reg")), AMBER));

            Section(scroll, "Ajustes rápidos");
            Row(scroll, new OptimizationAction("windows", "Desativar Apps em Segundo Plano", "Libera CPU e RAM", "Baixo", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/Desativar apps em segundo plano.reg")), CYAN));
            Row(scroll, new OptimizationAction("system", "Desativar Efeitos Visuais", "Reduz uso de GPU e memória", "Baixo", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/Desativar Efeitos Visuais.reg")), CYAN));
            Row(scroll, new OptimizationAction("clean", "Desativar Compactação", "Ajuste de compactação do Windows Update", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/Desativar Compactação.reg")), AMBER));
            Row(scroll, new OptimizationAction("memory", "Cache para Programas", "LargeSystemCache=0", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/Ajustar o Cache do Sistema para programas.reg")), GREEN));
            Row(scroll, new OptimizationAction("memory", "Diminuir Processos do Windows", "SvcHostSplitThresholdInKB", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/Diminuir processos do windows.reg")), AMBER));
            Row(scroll, new OptimizationAction("advanced", "Win32PrioritySeparation = 26", "Prioridade CPU para primeiro plano", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("01_Otimizacao_Windows/win32priorityseparation_26.reg")), AMBER));
        });

        _pages["servicos"] = Page(scroll =>
        {
            Banner(scroll, "servicos", "Serviços do Windows", "Serviços inexistentes, parados ou já desativados não devem gerar falso Falhou.", RED);
            Section(scroll, "Serviços");
            Row(scroll, new OptimizationAction("advanced", "Desativar Telemetria", "AllowTelemetry=0", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("02_Servicos/Desativar Telemetria.reg")), RED));
            Row(scroll, new OptimizationAction("perifericos", "Desabilitar Bluetooth", "BTAGService e bthserv", "Alto", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("02_Servicos/Desabilitar Serviço Bluetooth.reg")), RED));
            Row(scroll, new OptimizationAction("system", "Desativar Atualizações Auto.", "Política NoAutoUpdate", "Alto", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("02_Servicos/Desativar Atualizações Automáticas.reg")), RED));
            Row(scroll, new OptimizationAction("gpu", "Desativar Serviços Xbox", "XblGameSave, XboxNet, Gip e Auth", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("02_Servicos/Desativar Serviços Xbox.reg")), RED));
            Row(scroll, new OptimizationAction("clean", "Desativar Serviços Inúteis", "DiagTrack, WSearch e SysMain", "Alto", () => ScriptRunner.RunBatFileAsync(ScriptRunner.Script("02_Servicos/Desativar Serviços Inúteis do Windows.bat"), true), RED, true));
            Row(scroll, new OptimizationAction("servicos", "Desativar Serviços (CUIDADO)", "Diagnóstico, Bluetooth e remotos", "Alto", () => ScriptRunner.RunBatFileAsync(ScriptRunner.Script("02_Servicos/1 - Desativar seviços ( CUIDADO ).bat"), true), RED, true));
        });

        _pages["perifericos"] = Page(scroll =>
        {
            Banner(scroll, "perifericos", "Periféricos & GPU", "Mouse, teclado, Game Mode, GPU Scheduling, GameDVR, FSO e perfil de jogos.", CYAN);
            Section(scroll, "Periféricos");
            var grid = CardGrid(2);
            scroll.Children.Add(grid);
            AddFeature(grid, 0, new OptimizationAction("mouse", "Mouse / Teclado", "Reduz atrasos e menus", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("03_Perifericos/01_Perifericos.reg")), CYAN));
            AddFeature(grid, 1, new OptimizationAction("keyboard", "Periféricos Extra", "Ajustes adicionais", "Baixo/Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("03_Perifericos/otimizacao_perifericos.reg")), GREEN));
            Section(scroll, "GPU & Jogos");
            Row(scroll, new OptimizationAction("gpu", "Game Mode", "Ativa AutoGameMode no registro", "Baixo", () => ScriptRunner.RunBatFileAsync(ScriptRunner.Script("04_GPU_e_Jogos/Ativar Game Mode.bat"), true), CYAN));
            Row(scroll, new OptimizationAction("gpu", "GPU Scheduling (HAGS)", "HwSchMode=2 — Win10/11", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("04_GPU_e_Jogos/Ativar GPU Scheduling (win 10 e 11).reg")), CYAN));
            Row(scroll, new OptimizationAction("gpu", "GPU & Gaming Reg", "GameDVR, FSO e perfil multimídia", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("04_GPU_e_Jogos/02_GPU_e_Gaming.reg")), CYAN));
            Row(scroll, new OptimizationAction("windows", "Windows para Jogos", "Prioridade GPU/CPU e SystemResponsiveness", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("04_GPU_e_Jogos/Configurar Windows para Jogos.reg")), CYAN));
            Row(scroll, new OptimizationAction("gpu", "Desativar FSO", "Fullscreen Optimization / GameConfigStore", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("04_GPU_e_Jogos/Desativar FSO (Otimizar win para jogos).reg")), CYAN));
            Row(scroll, new OptimizationAction("gpu", "Desativar GameDVR", "Remove GameDVR + XboxGameBar por política", "Moderado", () => ScriptRunner.RunBatFileAsync(ScriptRunner.Script("04_GPU_e_Jogos/Desativar GameDVR - XboxGameBar.bat"), true), CYAN, true));
            Row(scroll, new OptimizationAction("gpu", "Desativar Xbox Bar", "Game Bar, dicas e Auto Game Mode", "Moderado", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("04_GPU_e_Jogos/Desativar Xbox Bar.reg")), CYAN));
            Row(scroll, new OptimizationAction("gpu", "GPU Scheduler Mode 22", "GPU_SCHEDULER_MODE=22", "Questionável", () => ScriptRunner.RunRegFileAsync(ScriptRunner.Script("04_GPU_e_Jogos/gpu_scheduler_mode_22.reg"), true), CYAN));
        });

        _pages["energia"] = Page(scroll =>
        {
            Banner(scroll, "energia", "Plano de Energia", "Importação do NexX Sensi com GUID fixo, renomeação e ativação.", AMBER);
            Section(scroll, "Plano personalizado NexX");
            Row(scroll, new OptimizationAction("power", "NexX Sensi", "Importa o .pow da pasta Scripts, renomeia o plano e ativa no Windows.", "Moderado", ScriptRunner.ImportNexxPowerPlanAsync, AMBER, true));
            Section(scroll, "Planos nativos do Windows");
            Row(scroll, new OptimizationAction("power", "Equilibrado", "powercfg /setactive SCHEME_BALANCED", "Baixo", () => ScriptRunner.RunCmdLinesAsync(new[] { "powercfg /setactive SCHEME_BALANCED" }, forceSuccess: true), AMBER));
            Row(scroll, new OptimizationAction("boost", "Alto Desempenho", "powercfg /setactive SCHEME_MIN", "Baixo", () => ScriptRunner.RunCmdLinesAsync(new[] { "powercfg /setactive SCHEME_MIN" }, forceSuccess: true), AMBER));
            Row(scroll, new OptimizationAction("energia", "Máximo Desempenho", "Duplica o esquema Ultimate Performance e ativa", "Baixo", () => ScriptRunner.RunCmdLinesAsync(new[] { "powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61", "powercfg /setactive e9a42b02-d5df-448d-aa00-03f14749eb61" }, forceSuccess: true), AMBER));
            Row(scroll, new OptimizationAction("memory", "Economia", "powercfg /setactive SCHEME_MAX", "Baixo", () => ScriptRunner.RunCmdLinesAsync(new[] { "powercfg /setactive SCHEME_MAX" }, forceSuccess: true), AMBER));
            Row(scroll, new OptimizationAction("clean", "Desativar Hibernação", "Remove hiberfil.sys e pode afetar Inicialização Rápida", "Moderado", () => ScriptRunner.RunBatFileAsync(ScriptRunner.Script("05_Plano_de_Energia/Desativar Hibernação.bat"), true), AMBER));
        });

        _pages["limpeza"] = Page(scroll =>
        {
            Banner(scroll, "limpeza", "Limpeza", "Rotinas tolerantes: arquivo em uso, pasta inexistente ou cache já apagado não geram falso Falhou.", RED);
            Section(scroll, "Rotinas de limpeza");
            Row(scroll, new OptimizationAction("limpeza", "Limpar Arquivos Temporários", "Temp, Windows Temp, Prefetch e logs antigos", "Alto", ScriptRunner.CleanupTempFilesAsync, RED, true));
            Row(scroll, new OptimizationAction("advanced", "Limpar Cache de Atualizações", "Para WU/BITS, limpa SoftwareDistribution e reinicia", "Moderado", ScriptRunner.CleanupWindowsUpdateCacheAsync, RED, true));
            Row(scroll, new OptimizationAction("clean", "LIMPAR rápido", "Temp, prefetch, histórico e recentes", "Moderado", ScriptRunner.CleanupSimpleAsync, RED, true));
            Row(scroll, new OptimizationAction("clean", "LIMPEZA TOTAL", "Temp, logs, prefetch e caches de navegadores", "Alto", ScriptRunner.CleanupTotalAsync, RED, true));
        });

        _pages["recursos"] = Page(scroll =>
        {
            Banner(scroll, "recursos", "Recursos Windows", "Recursos adicionais usados por otimizações e políticas locais.", CYAN);
            Section(scroll, "Group Policy");
            Row(scroll, new OptimizationAction("recursos", "Ativar Otimizador de Grupo", "Adiciona pacotes de Group Policy via DISM — habilita gpedit.msc em edições Home.", "Moderado", () => ScriptRunner.RunBatFileAsync(ScriptRunner.Script("08_Recursos_do_Windows/ativarotimizadorwindows.bat"), true), CYAN, true));
        });

        _pages["reparos"] = Page(scroll =>
        {
            Banner(scroll, "reparos", "Reparos do Sistema", "Ferramentas de manutenção com SFC, DISM e otimização de disco.", AMBER);
            Section(scroll, "Reparos");
            Row(scroll, new OptimizationAction("reparos", "Verificar Arquivos Corrompidos", "SFC /scannow + DISM RestoreHealth", "Baixo", () => ScriptRunner.RunBatFileAsync(ScriptRunner.Script("07_Reparos/Verificar Arquivos Corrompidos.bat"), true), AMBER, true));
            Row(scroll, new OptimizationAction("reparos", "Verificar + Desfragmentar (HDD)", "Defrag, SFC e DISM — indicado para HDs mecânicos", "Moderado", () => ScriptRunner.RunBatFileAsync(ScriptRunner.Script("07_Reparos/Verificar Arquivos Corrompidos (HDD).bat"), true), AMBER, true));
            Banner(scroll, "reparos", "Nota", "Não feche o terminal durante SFC/DISM. Reinicie o computador depois de concluir.", AMBER);
        });
    }

    private FrameworkElement Page(Action<StackPanel> build)
    {
        var stack = new StackPanel { Spacing = 0, Padding = new Thickness(0, 0, 4, 8) };
        build(stack);
        return new ScrollViewer
        {
            Content = stack,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Background = Brush(BG_MAIN)
        };
    }

    private void ShowPage(string key)
    {
        foreach (var pair in _navButtons)
        {
            var active = pair.Key == key;
            pair.Value.Background = Brush(active ? BG_CARD_HOVER : BG_SIDE);
            pair.Value.BorderBrush = Brush(active ? LINE : BG_SIDE);
            pair.Value.Foreground = Brush(active ? CYAN : TEXT_MUT);
        }
        _pageHost.Content = _pages[key];
    }

    private void Banner(StackPanel parent, string icon, string title, string desc, string accent)
    {
        var card = PanelBorder(BG_CARD, LINE, 20, 2);
        card.Margin = new Thickness(0, 0, 0, 12);
        card.Padding = new Thickness(18, 14, 18, 14);

        var row = new Grid { ColumnSpacing = 14 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        card.Child = row;

        var ic = Icon(icon, 42, accent);
        ic.VerticalAlignment = VerticalAlignment.Center;
        row.Children.Add(ic);

        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        text.Children.Add(Text(title, 22, TEXT_PRI, FontWeights.Bold));
        text.Children.Add(Text(desc, 15, TEXT_MUT));
        Grid.SetColumn(text, 1);
        row.Children.Add(text);
        parent.Children.Add(card);
    }

    private void Section(StackPanel parent, string title)
    {
        parent.Children.Add(Text(title.ToUpperInvariant(), 13, CYAN, FontWeights.Bold, new Thickness(2, 8, 0, 10)));
    }

    private Grid CardGrid(int columns)
    {
        var grid = new Grid { ColumnSpacing = 10, Margin = new Thickness(0, 0, 0, 14) };
        for (var i = 0; i < columns; i++) grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        return grid;
    }

    private void AddFeature(Grid grid, int column, OptimizationAction action)
    {
        var card = PanelBorder(BG_CARD, LINE, 20, 2);
        card.Padding = new Thickness(16, 14, 16, 14);
        var stack = new StackPanel { Spacing = 8 };
        stack.Children.Add(Icon(action.Icon, 44, action.Accent));
        stack.Children.Add(Text(action.Title, 16, TEXT_PRI, FontWeights.Bold));
        stack.Children.Add(Text(action.Description, 14, TEXT_MUT));
        stack.Children.Add(RiskBadge(action.Risk));
        stack.Children.Add(ActionButton(action, true));
        card.Child = stack;
        Grid.SetColumn(card, column);
        grid.Children.Add(card);
    }

    private void Row(StackPanel parent, OptimizationAction action)
    {
        var border = PanelBorder(BG_CARD, LINE, 18, 2);
        border.Margin = new Thickness(0, 0, 0, 8);
        border.Padding = new Thickness(16, 12, 16, 12);

        var grid = new Grid { ColumnSpacing = 14 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        border.Child = grid;

        var icon = Icon(action.Icon, 42, action.Accent);
        icon.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(icon, 0);
        grid.Children.Add(icon);

        var text = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        text.Children.Add(Text(action.Title, 16, TEXT_PRI, FontWeights.Bold));
        text.Children.Add(Text(action.Description, 14, TEXT_MUT));
        Grid.SetColumn(text, 1);
        grid.Children.Add(text);

        var risk = RiskBadge(action.Risk);
        risk.VerticalAlignment = VerticalAlignment.Center;
        Grid.SetColumn(risk, 2);
        grid.Children.Add(risk);

        var button = ActionButton(action, false);
        Grid.SetColumn(button, 3);
        grid.Children.Add(button);

        parent.Children.Add(border);
    }

    private Button ActionButton(OptimizationAction action, bool stretch)
    {
        var button = new Button
        {
            Content = "Aplicar",
            MinWidth = stretch ? 0 : 88,
            Height = 40,
            HorizontalAlignment = stretch ? HorizontalAlignment.Stretch : HorizontalAlignment.Right,
            Background = Brush("#0C2531"),
            BorderBrush = Brush("#1B6578"),
            BorderThickness = new Thickness(2),
            Foreground = Brush(CYAN),
            FontSize = 15,
            FontWeight = FontWeights.Bold,
            CornerRadius = new CornerRadius(15),
            Padding = new Thickness(14, 0, 14, 0)
        };
        AnimateButton(button, "#0C2531", "#123B4A", "#1B6578", CYAN);
        button.Click += async (_, _) => await RunAction(button, action);
        return button;
    }

    private async Task RunAction(Button button, OptimizationAction action)
    {
        if (!ScriptRunner.IsWindows)
        {
            SetButton(button, "Só Windows", RED);
            SetStatus("Este painel só executa no Windows.", RED);
            return;
        }

        button.IsEnabled = false;
        SetButton(button, "Executando", AMBER);
        SetStatus($"Executando: {action.Title}", AMBER);

        var result = await action.Runner();
        button.IsEnabled = true;
        if (result.Success)
        {
            SetButton(button, "Aplicado ✓", GREEN);
            SetStatus($"Aplicado: {action.Title}", GREEN);
        }
        else
        {
            SetButton(button, "Falhou", RED);
            var output = string.IsNullOrWhiteSpace(result.Output) ? "" : " — " + result.Output.Replace('\n', ' ').Replace('\r', ' ').Trim();
            if (output.Length > 140) output = output[..140];
            SetStatus($"Falhou: {action.Title}{output}", RED);
        }
    }

    private void SetButton(Button button, string text, string color)
    {
        button.Content = text;
        button.Foreground = Brush(color);
    }

    private void SetStatus(string text, string color)
    {
        _statusText.Text = "● " + text;
        _statusText.Foreground = Brush(color);
    }

    private Border RiskBadge(string risk)
    {
        var (bg, fg, label) = risk switch
        {
            "Baixo" => ("#0C2629", GREEN, "BAIXO"),
            "Baixo/Moderado" => ("#0C2629", GREEN, "BAIXO/MOD."),
            "Moderado" => ("#201E1E", AMBER, "MÉDIO"),
            "Alto" => ("#241625", RED, "ALTO"),
            "Questionável" => ("#171832", PURPLE, "QUEST."),
            _ => ("#0C2232", CYAN, risk.ToUpperInvariant())
        };
        return new Border
        {
            Background = Brush(bg),
            CornerRadius = new CornerRadius(11),
            Padding = new Thickness(9, 4, 9, 4),
            Child = Text(label, 12, fg, FontWeights.Bold)
        };
    }

    private Border Pill(string text, string fg, string bg) => new()
    {
        Background = Brush(bg),
        CornerRadius = new CornerRadius(18),
        Padding = new Thickness(12, 5, 12, 5),
        Child = Text(text, 13, fg, FontWeights.Bold)
    };

    private Border PanelBorder(string bg, string border, double radius, double thickness) => new()
    {
        Background = Brush(bg),
        BorderBrush = Brush(border),
        BorderThickness = new Thickness(thickness),
        CornerRadius = new CornerRadius(radius)
    };

    private TextBlock Text(string text, double size, string color, Windows.UI.Text.FontWeight? weight = null, Thickness? margin = null)
    {
        Text = text,
        Foreground = Brush(color),
        FontSize = size,
        FontWeight = weight ?? FontWeights.Normal,
        FontFamily = new FontFamily("Segoe UI Variable"),
        Margin = margin ?? new Thickness(0),
        TextWrapping = TextWrapping.Wrap
    };

    private FontIcon Icon(string key, double size, string color) => new()
    {
        Glyph = GlyphFor(key),
        FontFamily = new FontFamily("Segoe Fluent Icons"),
        FontSize = size,
        Foreground = Brush(color),
        Width = size + 8,
        Height = size + 8,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
    };

    private static string GlyphFor(string key) => key switch
    {
        "windows" => "\uE782",
        "servicos" => "\uE713",
        "perifericos" => "\uE7FC",
        "energia" => "\uE945",
        "limpeza" => "\uE74D",
        "recursos" => "\uE8A5",
        "reparos" => "\uE90F",
        "boost" => "\uE7C1",
        "memory" => "\uE950",
        "system" => "\uE9D9",
        "advanced" => "\uF18B",
        "mouse" => "\uE962",
        "keyboard" => "\uE765",
        "gpu" => "\uE950",
        "power" => "\uE7E8",
        "clean" => "\uE74D",
        _ => "\uE10F"
    };

    private void AnimateButton(Button button, string normalBg, string hoverBg, string normalBorder, string hoverBorder)
    {
        button.PointerEntered += (_, _) =>
        {
            button.Background = Brush(hoverBg);
            button.BorderBrush = Brush(hoverBorder);
            button.CenterPoint = new Vector3((float)(button.ActualWidth / 2), (float)(button.ActualHeight / 2), 0);
            button.Scale = new Vector3(1.025f, 1.025f, 1);
        };
        button.PointerExited += (_, _) =>
        {
            button.Background = Brush(normalBg);
            button.BorderBrush = Brush(normalBorder);
            button.Scale = Vector3.One;
        };
        button.PointerPressed += (_, _) => button.Scale = new Vector3(0.985f, 0.985f, 1);
        button.PointerReleased += (_, _) => button.Scale = new Vector3(1.015f, 1.015f, 1);
    }

    private void StartBackgroundAnimation()
    {
        _glowTimer = DispatcherQueue.CreateTimer();
        _glowTimer.Interval = TimeSpan.FromMilliseconds(120);
        _glowTimer.Tick += (_, _) =>
        {
            _glowPhase += 0.055;
            var t = (Math.Sin(_glowPhase) + 1.0) / 2.0;
            var color = Mix(LINE_SOFT, CYAN, t * 0.45);
            _mainShell.BorderBrush = Brush(color);
            _topbar.BorderBrush = Brush(Mix(LINE_SOFT, CYAN, t * 0.30));
            _sidebar.BorderBrush = Brush(Mix(LINE_SOFT, CYAN, (1 - t) * 0.25));
            _statusbar.BorderBrush = Brush(Mix(LINE_SOFT, CYAN, t * 0.20));
        };
        _glowTimer.Start();
    }

    private static SolidColorBrush Brush(string hex) => new(Hex(hex));

    private static Color Hex(string hex)
    {
        hex = hex.TrimStart('#');
        return Color.FromArgb(255, Convert.ToByte(hex[..2], 16), Convert.ToByte(hex.Substring(2, 2), 16), Convert.ToByte(hex.Substring(4, 2), 16));
    }

    private static string Mix(string a, string b, double t)
    {
        var ca = Hex(a); var cb = Hex(b);
        byte r = (byte)(ca.R + (cb.R - ca.R) * t);
        byte g = (byte)(ca.G + (cb.G - ca.G) * t);
        byte bl = (byte)(ca.B + (cb.B - ca.B) * t);
        return $"#{r:X2}{g:X2}{bl:X2}";
    }
}
