namespace TinyTrans.Core;

public class TranslationCommandController
{
    private readonly TranslationOrchestrator _orchestrator;
    private bool _isTranslating;
    private TranslationCommandState _state = TranslationCommandState.Empty();

    public TranslationCommandController(TranslationOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public TranslationCommandState State => _state;

    public string RequestedTargetLanguage => _orchestrator.RequestedTargetLanguage;

    public bool CanTranslate(string inputText) => !string.IsNullOrWhiteSpace(inputText);

    public void SetTargetLanguage(string language)
    {
        _orchestrator.SetTargetLanguage(language);
    }

    public async Task<TranslationCommandState> TranslateAsync(string inputText)
    {
        if (_isTranslating)
            return _state;

        var sourceText = inputText.Trim();
        if (string.IsNullOrEmpty(sourceText))
        {
            _state = TranslationCommandState.Empty();
            return _state;
        }

        _isTranslating = true;
        _state = TranslationCommandState.Loading(_state.OutputText);

        try
        {
            var result = await _orchestrator.TranslateAsync(sourceText);

            _state = result.IsSuccess
                ? TranslationCommandState.Success(result.TargetText)
                : TranslationCommandState.Failure(result.ErrorMessage!);

            return _state;
        }
        catch (Exception ex)
        {
            _state = TranslationCommandState.Failure(ex.Message);
            return _state;
        }
        finally
        {
            _isTranslating = false;
        }
    }
}
