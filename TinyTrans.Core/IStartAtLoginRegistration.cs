namespace TinyTrans.Core;

public interface IStartAtLoginRegistration
{
    string? ReadCommand(string appName);
    void WriteCommand(string appName, string command);
    void DeleteCommand(string appName);
}
