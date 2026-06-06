using System.Text.Json;

namespace TinyTrans.Core;

public class AppConfigStore
{
    private readonly string _path;

    public AppConfigStore(string path)
    {
        _path = path;
    }

    public AppConfigLoadResult LoadOrCreate()
    {
        if (!File.Exists(_path))
        {
            var defaults = AppConfig.CreateDefaults();
            Save(defaults);
            return new AppConfigLoadResult(defaults, loadedFromFile: false);
        }

        try
        {
            var json = File.ReadAllText(_path);
            var config = JsonSerializer.Deserialize<AppConfig>(json);

            if (config == null)
                return new AppConfigLoadResult(AppConfig.CreateDefaults(), loadedFromFile: false);

            return new AppConfigLoadResult(config, loadedFromFile: true);
        }
        catch (JsonException)
        {
            return new AppConfigLoadResult(AppConfig.CreateDefaults(), loadedFromFile: false);
        }
    }

    public void Save(AppConfig config)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_path, json);
    }
}
