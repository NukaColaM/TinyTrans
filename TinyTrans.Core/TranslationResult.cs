namespace TinyTrans.Core;

public class TranslationResult
{
    public string DetectedSourceLanguage { get; }
    public string TargetText { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess => ErrorMessage == null;

    private TranslationResult(string detectedSourceLanguage, string targetText, string? errorMessage)
    {
        DetectedSourceLanguage = detectedSourceLanguage;
        TargetText = targetText;
        ErrorMessage = errorMessage;
    }

    public static TranslationResult Success(string detectedSourceLanguage, string targetText)
        => new(detectedSourceLanguage, targetText, null);

    public static TranslationResult Failure(string errorMessage)
        => new("", "", errorMessage);
}
