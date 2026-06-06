using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using TinyTrans.Core;

namespace TinyTrans;

public partial class MainWindow : Window
{
    // ── Constants ──────────────────────────────────────────────────────
    private const double SingleLineHeight = 28;
    private const double MaxLines = 5;
    private const double MaxTextBoxHeight = SingleLineHeight + (MaxLines - 1) * 20; // ~108
    private const double TextBoxWidth = 380;
    private const double TextBoxPadding = 12; // 6px each side
    private const int CopyFeedbackMs = 1500;
    private const int ToggleWindowHotkeyId = 1;
    private const string ToggleWindowShortcutText = "Ctrl+Alt+T";

    private static readonly string[] SpinnerFrames = ["⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏"];

    // ── Fields ─────────────────────────────────────────────────────────
    private readonly AppConfig _config;
    private readonly TranslationCommandController _translationController;
    private readonly bool _hasSavedPosition;
    private HwndSource? _hwndSource;
    private IntPtr _hwnd;
    private CancellationTokenSource? _spinnerCts;

    // ── Constructor ────────────────────────────────────────────────────
    public MainWindow(AppConfig config, bool hasSavedPosition, ITranslationProvider translationProvider)
    {
        InitializeComponent();

        _config = config;
        _hasSavedPosition = hasSavedPosition;
        _translationController = new TranslationCommandController(
            new TranslationOrchestrator(translationProvider, _config.LastTargetLanguage));

        // Restore window position with multi-monitor safety
        RestoreSavedPosition();

        // Apply persisted topmost setting
        Topmost = _config.AlwaysOnTop;

        // First-run hint: no API key configured
        if (string.IsNullOrEmpty(_config.ApiKey))
        {
            OutputTextBox.Text = "Set your API key in config.json";
            OutputTextBox.Foreground = Brushes.Gray;
        }

        Loaded += (_, _) =>
        {
            InputTextBox.Focus();
            Keyboard.Focus(InputTextBox);
        };

        SourceInitialized += Window_SourceInitialized;
        Closed += Window_Closed;
    }

    // ── Public Properties ──────────────────────────────────────────────

    public string TargetLanguage => _translationController.RequestedTargetLanguage;

    public bool IsAlwaysOnTop => Topmost;

    public bool IsToggleShortcutRegistered { get; private set; }

    public string ToggleShortcutText => ToggleWindowShortcutText;

    // ── Window Lifecycle ───────────────────────────────────────────────

    private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
            DragMove();
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        _config.WindowLeft = Left;
        _config.WindowTop = Top;
        _config.AlwaysOnTop = Topmost;
    }

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        _hwnd = new WindowInteropHelper(this).Handle;
        _hwndSource = HwndSource.FromHwnd(_hwnd);
        _hwndSource?.AddHook(WndProc);

        NativeWindowInterop.HideFromAltTab(_hwnd);
        NativeWindowInterop.ApplyWindows11RoundedCorners(_hwnd);
        IsToggleShortcutRegistered = NativeWindowInterop.RegisterToggleHotkey(_hwnd, ToggleWindowHotkeyId);
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _spinnerCts?.Cancel();

        if (_hwnd != IntPtr.Zero)
            NativeWindowInterop.UnregisterHotkey(_hwnd, ToggleWindowHotkeyId);

        IsToggleShortcutRegistered = false;
        _hwndSource?.RemoveHook(WndProc);
        _hwndSource = null;
        _hwnd = IntPtr.Zero;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == NativeWindowInterop.WmHotkey && wParam.ToInt32() == ToggleWindowHotkeyId)
        {
            ToggleVisibility();
            handled = true;
        }

        return IntPtr.Zero;
    }

    // ── Position Persistence ───────────────────────────────────────────

    private void RestoreSavedPosition()
    {
        if (_hasSavedPosition)
        {
            var screenRect = new Rect(
                SystemParameters.VirtualScreenLeft,
                SystemParameters.VirtualScreenTop,
                SystemParameters.VirtualScreenWidth,
                SystemParameters.VirtualScreenHeight);

            // Check if saved position is within any active screen
            if (_config.WindowLeft >= screenRect.Left && _config.WindowLeft < screenRect.Right &&
                _config.WindowTop >= screenRect.Top && _config.WindowTop < screenRect.Bottom)
            {
                Left = _config.WindowLeft;
                Top = _config.WindowTop;
                return;
            }
        }

        // Fallback: bottom-right corner of primary screen with 40px margin
        // Calculate in constructor (ActualWidth/Height not available yet, use known defaults)
        Left = SystemParameters.WorkArea.Right - 480 - 40;
        Top = SystemParameters.WorkArea.Bottom - 100 - 40;
    }

    // ── Auto-Grow Textboxes ───────────────────────────────────────────

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateTextBoxHeight(InputTextBox);
        TranslateButton.IsEnabled = _translationController.CanTranslate(InputTextBox.Text);

        SyncOutputWithInput();
    }

    /// <summary>Clears the output area when the input text is empty.</summary>
    private void SyncOutputWithInput()
    {
        if (string.IsNullOrWhiteSpace(InputTextBox.Text))
        {
            OutputTextBox.Text = "";
            OutputTextBox.Foreground = Brushes.Black;
            CopyButton.IsEnabled = false;
        }
    }

    private void OutputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateTextBoxHeight(OutputTextBox);
    }

    private static void UpdateTextBoxHeight(TextBox textBox)
    {
        var formattedText = new FormattedText(
            textBox.Text,
            System.Globalization.CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface(textBox.FontFamily, textBox.FontStyle, textBox.FontWeight, textBox.FontStretch),
            textBox.FontSize,
            Brushes.Black,
            VisualTreeHelper.GetDpi(textBox).PixelsPerDip)
        {
            MaxTextWidth = TextBoxWidth - TextBoxPadding
        };

        var contentHeight = formattedText.Height + 8;

        var newHeight = Math.Max(SingleLineHeight, Math.Min(contentHeight, MaxTextBoxHeight));

        if (Math.Abs(textBox.Height - newHeight) > 1)
            textBox.Height = newHeight;

        textBox.VerticalScrollBarVisibility = contentHeight > MaxTextBoxHeight
            ? ScrollBarVisibility.Visible
            : ScrollBarVisibility.Hidden;
    }

    // ── Keyboard Shortcut ──────────────────────────────────────────────

    private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
        {
            e.Handled = true;
            _ = PerformTranslationAsync();
            return;
        }

    }

    // ── Translation Flow ───────────────────────────────────────────────

    private async void TranslateButton_Click(object sender, RoutedEventArgs e)
    {
        await PerformTranslationAsync();
    }

    private async Task PerformTranslationAsync()
    {
        if (_translationController.State.IsTranslating)
            return;

        var sourceText = InputTextBox.Text;
        if (!_translationController.CanTranslate(sourceText))
            return;

        // Clear any first-run hint once the user starts translating
        if (OutputTextBox.Text == "Set your API key in config.json")
        {
            OutputTextBox.Text = "";
            OutputTextBox.Foreground = Brushes.Black;
        }

        EnterLoadingState();

        try
        {
            var state = await _translationController.TranslateAsync(sourceText);

            if (state.HasError)
            {
                ShowError(state.OutputText);
                return;
            }

            DisplayResult(state.OutputText);
        }
        catch (Exception ex)
        {
            ShowError(ex.Message);
        }
        finally
        {
            ExitLoadingState();
            TranslateButton.IsEnabled = _translationController.CanTranslate(InputTextBox.Text);
        }
    }

    // ── Display Helpers ────────────────────────────────────────────────

    private void DisplayResult(string targetText)
    {
        OutputTextBox.Foreground = Brushes.Black;
        OutputTextBox.Text = targetText;
        CopyButton.IsEnabled = true;

        try { Clipboard.SetText(targetText); }
        catch { }
    }

    private void ShowError(string message)
    {
        OutputTextBox.Foreground = Brushes.Red;
        OutputTextBox.Text = message;
        CopyButton.IsEnabled = false;
    }

    // ── Loading State ──────────────────────────────────────────────────

    private void EnterLoadingState()
    {
        TranslateButton.IsEnabled = false;
        TranslateIconView.Visibility = Visibility.Collapsed;
        SpinnerText.Visibility = Visibility.Visible;

        OutputTextBox.Opacity = 0.35;
        LoadingOverlay.Visibility = Visibility.Visible;

        _spinnerCts = new CancellationTokenSource();
        _ = AnimateSpinnerAsync(_spinnerCts.Token);
    }

    private void ExitLoadingState()
    {
        _spinnerCts?.Cancel();
        _spinnerCts = null;

        SpinnerText.Visibility = Visibility.Collapsed;
        TranslateIconView.Visibility = Visibility.Visible;

        OutputTextBox.Opacity = 1.0;
        LoadingOverlay.Visibility = Visibility.Collapsed;
    }

    private async Task AnimateSpinnerAsync(CancellationToken ct)
    {
        var frameIndex = 0;
        try
        {
            while (!ct.IsCancellationRequested)
            {
                SpinnerText.Text = SpinnerFrames[frameIndex % SpinnerFrames.Length];
                frameIndex++;
                await Task.Delay(100, ct);
            }
        }
        catch (OperationCanceledException) { }
    }

    // ── Copy Button ────────────────────────────────────────────────────

    private async void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(OutputTextBox.Text))
            return;

        try { Clipboard.SetText(OutputTextBox.Text); }
        catch { }

        CopyIconView.Visibility = Visibility.Collapsed;
        CopyCheckView.Visibility = Visibility.Visible;
        CopyButton.ToolTip = "Copied!";

        await Task.Delay(CopyFeedbackMs);

        CopyIconView.Visibility = Visibility.Visible;
        CopyCheckView.Visibility = Visibility.Collapsed;
        CopyButton.ToolTip = "Copy";
    }

    // ── Tray-Triggered Actions ─────────────────────────────────────────

    public void ToggleVisibility(bool? show = null)
    {
        bool shouldShow = show ?? !IsVisible;

        if (shouldShow)
        {
            Show();
            Activate();
            InputTextBox.Focus();
            Keyboard.Focus(InputTextBox);
        }
        else
        {
            Hide();
        }
    }

    public void SetTargetLanguage(string language)
    {
        _translationController.SetTargetLanguage(language);
        _config.LastTargetLanguage = language;
    }

    public void SetAlwaysOnTop(bool alwaysOnTop)
    {
        Topmost = alwaysOnTop;
        _config.AlwaysOnTop = alwaysOnTop;
    }
}
