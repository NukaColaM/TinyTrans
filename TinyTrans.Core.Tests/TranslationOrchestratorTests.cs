using TinyTrans.Core;

namespace TinyTrans.Core.Tests;

public class TranslationOrchestratorTests
{
    [Fact]
    public async Task TranslateAsync_WhenDetectedSourceMatchesRequestedTarget_ReturnsSwappedEffectiveTarget()
    {
        var translationProvider = new StubTranslationProvider(
            TranslationResult.Success(detectedSourceLanguage: "EN", targetText: "你好"));
        var orchestrator = new TranslationOrchestrator(translationProvider, initialTargetLanguage: "EN");

        var result = await orchestrator.TranslateAsync("Hello");

        Assert.True(result.IsSuccess);
        Assert.Equal("EN", result.RequestedTargetLanguage);
        Assert.Equal("EN", result.DetectedSourceLanguage);
        Assert.Equal("ZH", result.EffectiveTargetLanguage);
        Assert.Equal("你好", result.TargetText);
    }

    [Fact]
    public async Task TranslateAsync_WhenDetectedSourceDiffersFromRequestedTarget_KeepsRequestedTargetAsEffectiveTarget()
    {
        var translationProvider = new StubTranslationProvider(
            TranslationResult.Success(detectedSourceLanguage: "EN", targetText: "你好"));
        var orchestrator = new TranslationOrchestrator(translationProvider, initialTargetLanguage: "ZH");

        var result = await orchestrator.TranslateAsync("Hello");

        Assert.True(result.IsSuccess);
        Assert.Equal("ZH", result.RequestedTargetLanguage);
        Assert.Equal("EN", result.DetectedSourceLanguage);
        Assert.Equal("ZH", result.EffectiveTargetLanguage);
        Assert.Equal("你好", result.TargetText);
    }

    [Fact]
    public async Task TranslateAsync_WhenDetectedSourceIsUnknown_ReturnsUnknownEffectiveTarget()
    {
        var translationProvider = new StubTranslationProvider(
            TranslationResult.Success(detectedSourceLanguage: "", targetText: "你好"));
        var orchestrator = new TranslationOrchestrator(translationProvider, initialTargetLanguage: "ZH");

        var result = await orchestrator.TranslateAsync("Hello");

        Assert.True(result.IsSuccess);
        Assert.Equal("ZH", result.RequestedTargetLanguage);
        Assert.Equal("", result.DetectedSourceLanguage);
        Assert.Equal("", result.EffectiveTargetLanguage);
        Assert.Equal("你好", result.TargetText);
    }

    [Fact]
    public async Task TranslateAsync_WhenProviderFails_ReturnsControlledFailureWithRequestedTarget()
    {
        var translationProvider = new StubTranslationProvider(
            TranslationResult.Failure("provider unavailable"));
        var orchestrator = new TranslationOrchestrator(translationProvider, initialTargetLanguage: "ZH");

        var result = await orchestrator.TranslateAsync("Hello");

        Assert.False(result.IsSuccess);
        Assert.Equal("ZH", result.RequestedTargetLanguage);
        Assert.Equal("provider unavailable", result.ErrorMessage);
        Assert.Equal("", result.DetectedSourceLanguage);
        Assert.Equal("", result.EffectiveTargetLanguage);
        Assert.Equal("", result.TargetText);
    }

    private sealed class StubTranslationProvider : ITranslationProvider
    {
        private readonly TranslationResult _result;

        public StubTranslationProvider(TranslationResult result)
        {
            _result = result;
        }

        public Task<TranslationResult> TranslateAsync(
            string sourceText,
            string targetLanguage,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }
}
