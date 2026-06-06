namespace TinyTrans.Core;

public class StartAtLoginService
{
    private readonly IStartAtLoginRegistration _registration;
    private readonly string _appName;
    private readonly string _command;

    public StartAtLoginService(IStartAtLoginRegistration registration, string appName, string executablePath)
    {
        _registration = registration;
        _appName = appName;
        _command = QuoteExecutablePath(executablePath);
    }

    public bool IsEnabled()
    {
        return string.Equals(
            _registration.ReadCommand(_appName),
            _command,
            StringComparison.OrdinalIgnoreCase);
    }

    public void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            _registration.WriteCommand(_appName, _command);
            return;
        }

        _registration.DeleteCommand(_appName);
    }

    private static string QuoteExecutablePath(string executablePath)
    {
        var trimmed = executablePath.Trim().Trim('"');
        return $"\"{trimmed}\"";
    }
}
