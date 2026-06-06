using System.Drawing;
using System.Reflection;
using System.Windows;
using TinyTrans.Core;

namespace TinyTrans;

public class TrayIconService : IDisposable
{
    private readonly System.Windows.Forms.NotifyIcon _notifyIcon;
    private readonly MainWindow _mainWindow;
    private readonly StartAtLoginService _startAtLoginService;

    public TrayIconService(MainWindow mainWindow, StartAtLoginService startAtLoginService)
    {
        _mainWindow = mainWindow;
        _startAtLoginService = startAtLoginService;

        _notifyIcon = new System.Windows.Forms.NotifyIcon
        {
            Icon = LoadTranslateIcon(),
            Text = "TinyTrans",
            Visible = true
        };

        _notifyIcon.Click += (s, e) =>
        {
            if (e is System.Windows.Forms.MouseEventArgs me && me.Button == System.Windows.Forms.MouseButtons.Left)
            {
                _mainWindow.ToggleVisibility();
            }
        };

        _notifyIcon.MouseUp += (s, e) =>
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                ShowContextMenu();
            }
        };
    }

    private static Icon LoadTranslateIcon()
    {
        try
        {
            using var stream = Assembly.GetExecutingAssembly()
                .GetManifestResourceStream("TinyTrans.Resources.translate.ico");
            if (stream != null)
                return new Icon(stream);
        }
        catch
        {
            // If icon resource is corrupt or missing, fall through to fallback
        }
        // Fallback: use application icon
        return SystemIcons.Application;
    }

    private void ShowContextMenu()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();

        // Header
        menu.Items.Add(new System.Windows.Forms.ToolStripMenuItem("TinyTrans") { Enabled = false });
        var shortcutLabel = _mainWindow.IsToggleShortcutRegistered
            ? $"Show/hide: {_mainWindow.ToggleShortcutText}"
            : "Show/hide shortcut unavailable";
        menu.Items.Add(new System.Windows.Forms.ToolStripMenuItem(shortcutLabel) { Enabled = false });
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        // Language radio items
        var enItem = new System.Windows.Forms.ToolStripMenuItem("English") { Checked = _mainWindow.TargetLanguage == "EN" };
        var zhItem = new System.Windows.Forms.ToolStripMenuItem("Chinese") { Checked = _mainWindow.TargetLanguage == "ZH" };

        enItem.Click += (s, e) =>
        {
            enItem.Checked = true;
            zhItem.Checked = false;
            _mainWindow.SetTargetLanguage("EN");
        };

        zhItem.Click += (s, e) =>
        {
            zhItem.Checked = true;
            enItem.Checked = false;
            _mainWindow.SetTargetLanguage("ZH");
        };

        menu.Items.Add(enItem);
        menu.Items.Add(zhItem);
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        // Always on Top toggle
        var alwaysOnTopItem = new System.Windows.Forms.ToolStripMenuItem("Always on Top")
        {
            Checked = _mainWindow.IsAlwaysOnTop,
            CheckOnClick = true
        };
        alwaysOnTopItem.Click += (s, e) =>
        {
            _mainWindow.SetAlwaysOnTop(alwaysOnTopItem.Checked);
        };
        menu.Items.Add(alwaysOnTopItem);

        // Start at login toggle
        var startAtLoginItem = new System.Windows.Forms.ToolStripMenuItem("Start at login")
        {
            Checked = IsStartAtLoginEnabled(),
            CheckOnClick = true
        };
        startAtLoginItem.Click += (s, e) =>
        {
            try
            {
                _startAtLoginService.SetEnabled(startAtLoginItem.Checked);
                startAtLoginItem.Checked = IsStartAtLoginEnabled();
            }
            catch
            {
                startAtLoginItem.Checked = IsStartAtLoginEnabled();
            }
        };
        menu.Items.Add(startAtLoginItem);
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());

        // Exit
        menu.Items.Add(new System.Windows.Forms.ToolStripMenuItem("Exit", null, (s, e) =>
        {
            _notifyIcon.Visible = false;
            Application.Current.Shutdown();
        }));

        menu.Show(System.Windows.Forms.Cursor.Position);
    }

    private bool IsStartAtLoginEnabled()
    {
        try
        {
            return _startAtLoginService.IsEnabled();
        }
        catch
        {
            return false;
        }
    }

    public void Dispose()
    {
        _notifyIcon.Visible = false;
        _notifyIcon.Dispose();
    }
}
