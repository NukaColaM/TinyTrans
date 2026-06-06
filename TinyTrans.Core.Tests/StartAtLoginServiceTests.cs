using TinyTrans.Core;

namespace TinyTrans.Core.Tests;

public class StartAtLoginServiceTests
{
    [Fact]
    public void IsEnabled_WhenRegistrationIsMissing_ReturnsFalse()
    {
        var registration = new InMemoryStartAtLoginRegistration();
        var service = new StartAtLoginService(registration, "TinyTrans", @"C:\Apps\TinyTrans\TinyTrans.exe");

        Assert.False(service.IsEnabled());
    }

    [Fact]
    public void Enable_WritesQuotedCommandForCurrentUserRegistration()
    {
        var registration = new InMemoryStartAtLoginRegistration();
        var service = new StartAtLoginService(registration, "TinyTrans", @"C:\Apps\TinyTrans\TinyTrans.exe");

        service.SetEnabled(true);

        Assert.True(service.IsEnabled());
        Assert.Equal("\"C:\\Apps\\TinyTrans\\TinyTrans.exe\"", registration.ReadCommand("TinyTrans"));
    }

    [Fact]
    public void Disable_RemovesCurrentUserRegistration()
    {
        var registration = new InMemoryStartAtLoginRegistration();
        var service = new StartAtLoginService(registration, "TinyTrans", @"C:\Apps\TinyTrans\TinyTrans.exe");
        service.SetEnabled(true);

        service.SetEnabled(false);

        Assert.False(service.IsEnabled());
        Assert.Null(registration.ReadCommand("TinyTrans"));
    }

    [Fact]
    public void IsEnabled_WhenRegisteredCommandDiffers_ReturnsFalse()
    {
        var registration = new InMemoryStartAtLoginRegistration();
        registration.WriteCommand("TinyTrans", "\"C:\\Other\\TinyTrans.exe\"");
        var service = new StartAtLoginService(registration, "TinyTrans", @"C:\Apps\TinyTrans\TinyTrans.exe");

        Assert.False(service.IsEnabled());
    }

    private sealed class InMemoryStartAtLoginRegistration : IStartAtLoginRegistration
    {
        private readonly Dictionary<string, string> _commands = new(StringComparer.OrdinalIgnoreCase);

        public string? ReadCommand(string appName)
        {
            return _commands.TryGetValue(appName, out var command) ? command : null;
        }

        public void WriteCommand(string appName, string command)
        {
            _commands[appName] = command;
        }

        public void DeleteCommand(string appName)
        {
            _commands.Remove(appName);
        }
    }
}
