using TinyTrans.Core;

namespace TinyTrans.Core.Tests;

public class AppConfigStoreTests
{
    [Fact]
    public void LoadOrCreate_WhenFileDoesNotExist_ReturnsDefaultsAndCreatesFile()
    {
        var path = CreateTempConfigPath();
        try
        {
            var store = new AppConfigStore(path);

            var result = store.LoadOrCreate();

            Assert.False(result.LoadedFromFile);
            Assert.Equal("https://api.deepseek.com/v1/chat/completions", result.Config.Endpoint);
            Assert.Equal("deepseek-v4-flash", result.Config.Model);
            Assert.Equal("", result.Config.ApiKey);
            Assert.True(File.Exists(path));
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void LoadOrCreate_WhenFileExists_LoadsExistingConfig()
    {
        var path = CreateTempConfigPath();
        try
        {
            var saved = new AppConfig
            {
                Endpoint = "https://custom.endpoint",
                Model = "custom-model",
                ApiKey = "sk-abc123",
                WindowLeft = 100,
                WindowTop = 200,
                AlwaysOnTop = true,
                LastTargetLanguage = "ZH"
            };
            var store = new AppConfigStore(path);
            store.Save(saved);

            var result = store.LoadOrCreate();

            Assert.True(result.LoadedFromFile);
            Assert.Equal(saved.Endpoint, result.Config.Endpoint);
            Assert.Equal(saved.Model, result.Config.Model);
            Assert.Equal(saved.ApiKey, result.Config.ApiKey);
            Assert.Equal(saved.WindowLeft, result.Config.WindowLeft);
            Assert.Equal(saved.WindowTop, result.Config.WindowTop);
            Assert.Equal(saved.AlwaysOnTop, result.Config.AlwaysOnTop);
            Assert.Equal(saved.LastTargetLanguage, result.Config.LastTargetLanguage);
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void LoadOrCreate_WhenJsonIsInvalid_ReturnsDefaultsWithoutOverwritingFile()
    {
        var path = CreateTempConfigPath();
        try
        {
            File.WriteAllText(path, "not json");
            var store = new AppConfigStore(path);

            var result = store.LoadOrCreate();

            Assert.False(result.LoadedFromFile);
            Assert.Equal("https://api.deepseek.com/v1/chat/completions", result.Config.Endpoint);
            Assert.Equal("not json", File.ReadAllText(path));
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    [Fact]
    public void SaveAndLoadOrCreate_RoundTripsAllFields()
    {
        var path = CreateTempConfigPath();
        try
        {
            var original = new AppConfig
            {
                Endpoint = "https://custom.endpoint",
                Model = "custom-model",
                ApiKey = "sk-abc123",
                WindowLeft = 100,
                WindowTop = 200,
                AlwaysOnTop = true,
                LastTargetLanguage = "ZH"
            };
            var store = new AppConfigStore(path);

            store.Save(original);
            var loaded = store.LoadOrCreate().Config;

            Assert.Equal(original.Endpoint, loaded.Endpoint);
            Assert.Equal(original.Model, loaded.Model);
            Assert.Equal(original.ApiKey, loaded.ApiKey);
            Assert.Equal(original.WindowLeft, loaded.WindowLeft);
            Assert.Equal(original.WindowTop, loaded.WindowTop);
            Assert.Equal(original.AlwaysOnTop, loaded.AlwaysOnTop);
            Assert.Equal(original.LastTargetLanguage, loaded.LastTargetLanguage);
        }
        finally
        {
            DeleteIfExists(path);
        }
    }

    private static string CreateTempConfigPath()
    {
        return Path.Combine(Path.GetTempPath(), $"tinytrans-{Guid.NewGuid():N}.json");
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
            File.Delete(path);
    }
}
