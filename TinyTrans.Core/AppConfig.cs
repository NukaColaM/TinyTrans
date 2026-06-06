namespace TinyTrans.Core;

public class AppConfig
{
    public string Endpoint { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public double WindowLeft { get; set; }
    public double WindowTop { get; set; }
    public bool AlwaysOnTop { get; set; }
    public string LastTargetLanguage { get; set; } = string.Empty;

    public static AppConfig CreateDefaults()
    {
        return new AppConfig
        {
            Endpoint = "https://api.deepseek.com/v1/chat/completions",
            Model = "deepseek-v4-flash",
            ApiKey = "",
            LastTargetLanguage = "EN",
            AlwaysOnTop = false
        };
    }
}
