namespace TinyTrans.Core;

public class TranslationCommandState
{
    public bool IsTranslating { get; }
    public string OutputText { get; }
    public bool CanCopy { get; }
    public bool HasError { get; }

    private TranslationCommandState(bool isTranslating, string outputText, bool canCopy, bool hasError)
    {
        IsTranslating = isTranslating;
        OutputText = outputText;
        CanCopy = canCopy;
        HasError = hasError;
    }

    public static TranslationCommandState Empty()
        => new(isTranslating: false, outputText: "", canCopy: false, hasError: false);

    public static TranslationCommandState Loading(string outputText)
        => new(isTranslating: true, outputText, canCopy: false, hasError: false);

    public static TranslationCommandState Success(string translatedText)
        => new(isTranslating: false, translatedText, canCopy: true, hasError: false);

    public static TranslationCommandState Failure(string errorMessage)
        => new(isTranslating: false, errorMessage, canCopy: false, hasError: true);
}
