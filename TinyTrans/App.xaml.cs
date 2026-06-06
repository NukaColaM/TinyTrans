using System.Windows;
using TinyTrans.Core;

namespace TinyTrans;

public partial class App : Application
{
    private const string MutexName = "TinyTrans-SingleInstanceMutex";
    private const string ActivateEventName = "TinyTrans-ActivateEvent";

    private SingleInstanceCoordinator? _singleInstanceCoordinator;
    private MainWindow? _mainWindow;
    private TrayIconService? _trayIcon;
    private AppConfig? _config;
    private AppConfigStore? _configStore;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _singleInstanceCoordinator = new SingleInstanceCoordinator(MutexName, ActivateEventName, Dispatcher);
        if (!_singleInstanceCoordinator.TryStart(ActivateMainWindow))
        {
            Shutdown();
            return;
        }

        var configLoadResult = LoadConfig();
        _config = configLoadResult.Config;

        var translationProvider = CreateTranslationProvider(_config);

        _mainWindow = new MainWindow(_config, configLoadResult.LoadedFromFile, translationProvider);
        _mainWindow.Closing += (s, args) => SaveConfig();

        // Show then immediately hide the window so that WPF fully
        // initialises the HWND, HwndSource, and the WndProc hook.
        // Without this the global hotkey (Ctrl+Alt+T) won't work
        // until the user first clicks the tray icon.
        _mainWindow.Show();
        _mainWindow.Hide();

        TryCreateTrayIcon();
    }

    private AppConfigLoadResult LoadConfig()
    {
        var configPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        _configStore = new AppConfigStore(configPath);
        return _configStore.LoadOrCreate();
    }

    private static ITranslationProvider CreateTranslationProvider(AppConfig config)
    {
        var httpClient = new HttpClient();
        return new OpenAiCompatibleTranslationProvider(
            httpClient,
            config.Endpoint,
            config.Model,
            config.ApiKey);
    }

    private void TryCreateTrayIcon()
    {
        if (_mainWindow == null)
            return;

        try
        {
            _trayIcon = new TrayIconService(_mainWindow, CreateStartAtLoginService());
        }
        catch
        {
            // Tray icon unavailable — window still works
        }
    }

    private static StartAtLoginService CreateStartAtLoginService()
    {
        var executablePath = Environment.ProcessPath;
        if (string.IsNullOrWhiteSpace(executablePath))
            executablePath = System.IO.Path.Combine(AppContext.BaseDirectory, "TinyTrans.exe");

        return new StartAtLoginService(
            new WindowsRunKeyStartAtLoginRegistration(),
            "TinyTrans",
            executablePath);
    }

    private void ActivateMainWindow()
    {
        if (_mainWindow == null)
            return;

        _mainWindow.Show();
        _mainWindow.Activate();
        _mainWindow.ToggleVisibility(show: true);
    }

    private void SaveConfig()
    {
        if (_mainWindow != null && _config != null)
        {
            _config.WindowLeft = _mainWindow.Left;
            _config.WindowTop = _mainWindow.Top;
            _config.AlwaysOnTop = _mainWindow.Topmost;
        }

        if (_config != null)
            _configStore?.Save(_config);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _trayIcon?.Dispose();
        _singleInstanceCoordinator?.Dispose();
        base.OnExit(e);
    }
}
