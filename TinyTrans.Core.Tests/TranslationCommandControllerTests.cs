using TinyTrans.Core;

namespace TinyTrans.Core.Tests;

public class TranslationCommandControllerTests
{
    [Fact]
    public void CanTranslate_WithWhitespaceOnlyInput_ReturnsFalse()
    {
        var controller = CreateController(TranslationResult.Success("EN", "你好"));

        Assert.False(controller.CanTranslate(" \r\n\t "));
    }

    [Fact]
    public void CanTranslate_WithNonEmptyMultilineInput_ReturnsTrue()
    {
        var controller = CreateController(TranslationResult.Success("EN", "你好"));

        Assert.True(controller.CanTranslate("first line\nsecond line"));
    }

    [Fact]
    public async Task TranslateAsync_WithWhitespaceOnlyInput_DoesNotCallProvider()
    {
        var provider = new CapturingTranslationProvider(TranslationResult.Success("EN", "你好"));
        var controller = CreateController(provider);

        var state = await controller.TranslateAsync(" \r\n\t ");

        Assert.Equal(0, provider.CallCount);
        Assert.False(state.IsTranslating);
        Assert.False(state.CanCopy);
        Assert.Equal("", state.OutputText);
    }

    [Fact]
    public async Task TranslateAsync_WithMultilineInput_TrimsBoundariesAndPreservesInternalLineBreaks()
    {
        var provider = new CapturingTranslationProvider(TranslationResult.Success("EN", "你好"));
        var controller = CreateController(provider);

        await controller.TranslateAsync("  first line\nsecond line  ");

        Assert.Equal("first line\nsecond line", provider.LastSourceText);
    }

    [Fact]
    public async Task TranslateAsync_WhileTranslationIsRunning_DoesNotStartDuplicateTranslation()
    {
        var provider = new BlockingTranslationProvider();
        var controller = CreateController(provider);

        var firstTranslation = controller.TranslateAsync("Hello");
        await provider.WaitUntilCalledAsync();

        var duplicateState = await controller.TranslateAsync("Hello again");

        Assert.True(duplicateState.IsTranslating);
        Assert.Equal(1, provider.CallCount);

        provider.Complete(TranslationResult.Success("EN", "你好"));
        await firstTranslation;
    }

    [Fact]
    public async Task TranslateAsync_OnSuccess_ExposesTranslatedTextAndEnablesCopy()
    {
        var controller = CreateController(TranslationResult.Success("EN", "你好"));

        var state = await controller.TranslateAsync("Hello");

        Assert.False(state.IsTranslating);
        Assert.False(state.HasError);
        Assert.True(state.CanCopy);
        Assert.Equal("你好", state.OutputText);
    }

    [Fact]
    public async Task TranslateAsync_OnFailure_ExposesErrorAndDisablesCopy()
    {
        var controller = CreateController(TranslationResult.Failure("provider unavailable"));

        var state = await controller.TranslateAsync("Hello");

        Assert.False(state.IsTranslating);
        Assert.True(state.HasError);
        Assert.False(state.CanCopy);
        Assert.Equal("provider unavailable", state.OutputText);
    }

    private static TranslationCommandController CreateController(TranslationResult providerResult)
    {
        return CreateController(new CapturingTranslationProvider(providerResult));
    }

    private static TranslationCommandController CreateController(ITranslationProvider provider)
    {
        return new TranslationCommandController(new TranslationOrchestrator(provider, "ZH"));
    }

    private sealed class CapturingTranslationProvider : ITranslationProvider
    {
        private readonly TranslationResult _result;

        public int CallCount { get; private set; }
        public string? LastSourceText { get; private set; }

        public CapturingTranslationProvider(TranslationResult result)
        {
            _result = result;
        }

        public Task<TranslationResult> TranslateAsync(
            string sourceText,
            string requestedTargetLanguage,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            LastSourceText = sourceText;
            return Task.FromResult(_result);
        }
    }

    private sealed class BlockingTranslationProvider : ITranslationProvider
    {
        private readonly TaskCompletionSource _called = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<TranslationResult> _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int CallCount { get; private set; }

        public Task WaitUntilCalledAsync() => _called.Task;

        public void Complete(TranslationResult result) => _completion.SetResult(result);

        public Task<TranslationResult> TranslateAsync(
            string sourceText,
            string requestedTargetLanguage,
            CancellationToken cancellationToken = default)
        {
            CallCount++;
            _called.SetResult();
            return _completion.Task;
        }
    }
}
