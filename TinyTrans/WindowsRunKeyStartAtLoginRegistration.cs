using Microsoft.Win32;
using TinyTrans.Core;

namespace TinyTrans;

public class WindowsRunKeyStartAtLoginRegistration : IStartAtLoginRegistration
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public string? ReadCommand(string appName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        return key?.GetValue(appName) as string;
    }

    public void WriteCommand(string appName, string command)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true);
        key.SetValue(appName, command, RegistryValueKind.String);
    }

    public void DeleteCommand(string appName)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: true);
        key?.DeleteValue(appName, throwOnMissingValue: false);
    }
}
