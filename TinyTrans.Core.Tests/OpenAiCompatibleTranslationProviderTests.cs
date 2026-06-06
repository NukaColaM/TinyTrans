using System.Net;
using System.Text.Json;
using TinyTrans.Core;

namespace TinyTrans.Core.Tests;

public class OpenAiCompatibleTranslationProviderTests
{
    [Fact]
    public async Task TranslateAsync_WithPlainTranslatedContent_ReturnsTextWithoutDetectedSourceLanguage()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ProviderResponse("Hello world"));
        var httpClient = new HttpClient(handler);
        var provider = CreateProvider(httpClient);

        var result = await provider.TranslateAsync("Bonjour le monde", "EN");

        Assert.True(result.IsSuccess);
        Assert.Equal("", result.DetectedSourceLanguage);
        Assert.Equal("Hello world", result.TargetText);
    }

    [Fact]
    public async Task TranslateAsync_WithProviderLabel_DoesNotParseOrStripLabel()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, ProviderResponse("[EN] Hello world"));
        var httpClient = new HttpClient(handler);
        var provider = CreateProvider(httpClient);

        var result = await provider.TranslateAsync("Bonjour le monde", "EN");

        Assert.True(result.IsSuccess);
        Assert.Equal("", result.DetectedSourceLanguage);
        Assert.Equal("[EN] Hello world", result.TargetText);
    }

    [Fact]
    public async Task TranslateAsync_WhenApiReturnsError_ReturnsFailureResultWithoutResponseBody()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError, "secret provider details");
        var httpClient = new HttpClient(handler);
        var provider = CreateProvider(httpClient);

        var result = await provider.TranslateAsync("Bonjour", "ZH");

        Assert.False(result.IsSuccess);
        Assert.Contains("500", result.ErrorMessage);
        Assert.DoesNotContain("secret provider details", result.ErrorMessage);
    }

    [Fact]
    public async Task TranslateAsync_WhenProviderContentIsMissing_ReturnsUnexpectedFormatFailure()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "{}");
        var httpClient = new HttpClient(handler);
        var provider = CreateProvider(httpClient);

        var result = await provider.TranslateAsync("Bonjour", "ZH");

        Assert.False(result.IsSuccess);
        Assert.Equal("Unexpected API response format", result.ErrorMessage);
    }

    [Fact]
    public async Task TranslateAsync_WhenProviderJsonIsInvalid_ReturnsUnexpectedFormatFailure()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, "not json");
        var httpClient = new HttpClient(handler);
        var provider = CreateProvider(httpClient);

        var result = await provider.TranslateAsync("Bonjour", "ZH");

        Assert.False(result.IsSuccess);
        Assert.Equal("Unexpected API response format", result.ErrorMessage);
    }

    [Fact]
    public async Task TranslateAsync_SendsOpenAiCompatibleRequest()
    {
        HttpMethod? capturedMethod = null;
        Uri? capturedUri = null;
        string? capturedAuthorization = null;
        string? capturedBody = null;
        var handler = new CapturingHttpMessageHandler(
            HttpStatusCode.OK,
            ProviderResponse("Hello"),
            (request, body) =>
            {
                capturedMethod = request.Method;
                capturedUri = request.RequestUri;
                capturedAuthorization = request.Headers.Authorization?.ToString();
                capturedBody = body;
            });
        var httpClient = new HttpClient(handler);
        var provider = CreateProvider(httpClient, endpoint: "https://example.test/v1/chat/completions");

        await provider.TranslateAsync("Hello", "ZH");

        Assert.Equal(HttpMethod.Post, capturedMethod);
        Assert.Equal("https://example.test/v1/chat/completions", capturedUri?.ToString());
        Assert.Equal("Bearer test-key", capturedAuthorization);
        Assert.NotNull(capturedBody);

        using var document = JsonDocument.Parse(capturedBody!);
        var root = document.RootElement;
        Assert.Equal("deepseek-v4-flash", root.GetProperty("model").GetString());
        var message = root.GetProperty("messages")[0];
        Assert.Equal("user", message.GetProperty("role").GetString());
        var content = message.GetProperty("content").GetString();
        Assert.Contains("translate to ZH", content);
        Assert.Contains("matches ZH", content);
        Assert.Contains("the other language", content);
        Assert.Contains("organize", content);
        Assert.Contains("polish", content);
        Assert.Contains("preserve the meaning", content);
        Assert.Contains("tone", content);
        Assert.Contains("line breaks", content);
        Assert.Contains("important formatting", content);
        Assert.Contains("output only the translated text", content);
        Assert.DoesNotContain("Output format", content);
        Assert.DoesNotContain("[ISO", content);
        Assert.DoesNotContain("[EN]", content);
        Assert.DoesNotContain("language label", content);
        Assert.Contains("Hello", content);
    }

    private static string ProviderResponse(string content)
    {
        return JsonSerializer.Serialize(new
        {
            choices = new[] { new { message = new { content } } }
        });
    }

    private static OpenAiCompatibleTranslationProvider CreateProvider(
        HttpClient httpClient,
        string endpoint = "https://api.deepseek.com/v1/chat/completions")
    {
        return new OpenAiCompatibleTranslationProvider(
            httpClient,
            endpoint,
            model: "deepseek-v4-flash",
            apiKey: "test-key");
    }
}

public class CapturingHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseContent;
    private readonly Action<HttpRequestMessage, string> _onRequest;

    public CapturingHttpMessageHandler(
        HttpStatusCode statusCode,
        string responseContent,
        Action<HttpRequestMessage, string> onRequest)
    {
        _statusCode = statusCode;
        _responseContent = responseContent;
        _onRequest = onRequest;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var body = await request.Content!.ReadAsStringAsync(cancellationToken);
        _onRequest(request, body);
        return new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseContent)
        };
    }
}

public class FakeHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseContent;

    public FakeHttpMessageHandler(HttpStatusCode statusCode, string responseContent)
    {
        _statusCode = statusCode;
        _responseContent = responseContent;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseContent)
        };
        return Task.FromResult(response);
    }
}
