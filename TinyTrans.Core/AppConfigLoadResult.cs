namespace TinyTrans.Core;

public class AppConfigLoadResult
{
    public AppConfig Config { get; }
    public bool LoadedFromFile { get; }

    public AppConfigLoadResult(AppConfig config, bool loadedFromFile)
    {
        Config = config;
        LoadedFromFile = loadedFromFile;
    }
}
