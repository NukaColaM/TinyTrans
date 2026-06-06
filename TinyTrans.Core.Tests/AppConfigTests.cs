using TinyTrans.Core;

namespace TinyTrans.Core.Tests;

public class AppConfigTests
{
    [Fact]
    public void CreateDefaults_ReturnsConfigWithDeepseekDefaults()
    {
        var config = AppConfig.CreateDefaults();

        Assert.Equal("https://api.deepseek.com/v1/chat/completions", config.Endpoint);
        Assert.Equal("deepseek-v4-flash", config.Model);
        Assert.Equal("", config.ApiKey);
        Assert.Equal("EN", config.LastTargetLanguage);
        Assert.False(config.AlwaysOnTop);
    }
}
