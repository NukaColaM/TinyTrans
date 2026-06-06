namespace TinyTrans.Core;

public class TranslationOutcome
{
    public string RequestedTargetLanguage { get; }
    public string DetectedSourceLanguage { get; }
    public string EffectiveTargetLanguage { get; }
    public string TargetText { get; }
    public string? ErrorMessage { get; }
    public bool IsSuccess => ErrorMessage == null;

    private TranslationOutcome(
        string requestedTargetLanguage,
        string detectedSourceLanguage,
        string effectiveTargetLanguage,
        string targetText,
        string? errorMessage)
    {
        RequestedTargetLanguage = requestedTargetLanguage;
        DetectedSourceLanguage = detectedSourceLanguage;
        EffectiveTargetLanguage = effectiveTargetLanguage;
        TargetText = targetText;
        ErrorMessage = errorMessage;
    }

    public static TranslationOutcome Success(
        string requestedTargetLanguage,
        string detectedSourceLanguage,
        string targetText)
    {
        return new TranslationOutcome(
            requestedTargetLanguage,
            detectedSourceLanguage,
            ResolveEffectiveTarget(requestedTargetLanguage, detectedSourceLanguage),
            targetText,
            null);
    }

    public static TranslationOutcome Failure(string requestedTargetLanguage, string errorMessage)
        => new(requestedTargetLanguage, "", "", "", errorMessage);

    private static string ResolveEffectiveTarget(string requestedTargetLanguage, string detectedSourceLanguage)
    {
        if (string.IsNullOrEmpty(detectedSourceLanguage))
            return "";

        if (detectedSourceLanguage == requestedTargetLanguage)
            return requestedTargetLanguage == "EN" ? "ZH" : "EN";

        return requestedTargetLanguage;
    }
}
