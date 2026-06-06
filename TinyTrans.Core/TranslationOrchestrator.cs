namespace TinyTrans.Core;

public class TranslationOrchestrator
{
    private readonly ITranslationProvider _translationProvider;
    private string _requestedTargetLanguage;

    public string RequestedTargetLanguage => _requestedTargetLanguage;

    public TranslationOrchestrator(ITranslationProvider translationProvider, string initialTargetLanguage)
    {
        _translationProvider = translationProvider;
        _requestedTargetLanguage = ValidateLanguage(initialTargetLanguage);
    }

    public void SetTargetLanguage(string language)
    {
        _requestedTargetLanguage = ValidateLanguage(language);
    }

    public async Task<TranslationOutcome> TranslateAsync(string sourceText)
    {
        var providerResult = await _translationProvider.TranslateAsync(sourceText, _requestedTargetLanguage);

        if (!providerResult.IsSuccess)
            return TranslationOutcome.Failure(_requestedTargetLanguage, providerResult.ErrorMessage!);

        return TranslationOutcome.Success(
            _requestedTargetLanguage,
            providerResult.DetectedSourceLanguage,
            providerResult.TargetText);
    }

    private static string ValidateLanguage(string language)
    {
        if (language is not ("EN" or "ZH"))
            throw new ArgumentException($"Unsupported language: {language}", nameof(language));

        return language;
    }
}
