namespace TinyTrans.Core;

public interface ITranslationProvider
{
    Task<TranslationResult> TranslateAsync(
        string sourceText,
        string requestedTargetLanguage,
        CancellationToken cancellationToken = default);
}
