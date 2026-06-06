using System.Text.Json;
using System.Text.Json.Nodes;

namespace TinyTrans.Core;

public class OpenAiCompatibleTranslationProvider : ITranslationProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _endpoint;
    private readonly string _model;
    private readonly string _apiKey;

    public OpenAiCompatibleTranslationProvider(HttpClient httpClient, string endpoint, string model, string apiKey)
    {
        _httpClient = httpClient;
        _endpoint = endpoint;
        _model = model;
        _apiKey = apiKey;
    }

    public async Task<TranslationResult> TranslateAsync(
        string sourceText,
        string requestedTargetLanguage,
        CancellationToken cancellationToken = default)
    {
        var prompt = $"Detect the source language internally and translate to {requestedTargetLanguage}. If the detected source language matches {requestedTargetLanguage}, translate to the other language instead (only EN and ZH are available). Before translating, organize and polish the input; preserve the meaning, tone, useful line breaks, and important formatting. Please output only the translated text, with no prefixes, brackets, explanations, or metadata.";

        var requestBody = new
        {
            model = _model,
            messages = new[]
            {
                new { role = "user", content = prompt + "\n\n" + sourceText }
            }
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, _endpoint)
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody), System.Text.Encoding.UTF8, "application/json")
        };
        httpRequest.Headers.Add("Authorization", $"Bearer {_apiKey}");

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        }
        catch (Exception ex)
        {
            return TranslationResult.Failure(ex.Message);
        }

        if (!response.IsSuccessStatusCode)
        {
            return TranslationResult.Failure($"API returned {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var rawResponse = await response.Content.ReadAsStringAsync(cancellationToken);
        string? content;
        try
        {
            var parsed = JsonNode.Parse(rawResponse);
            content = parsed?["choices"]?[0]?["message"]?["content"]?.GetValue<string>();
        }
        catch (JsonException)
        {
            return TranslationResult.Failure("Unexpected API response format");
        }
        catch (InvalidOperationException)
        {
            return TranslationResult.Failure("Unexpected API response format");
        }

        if (content == null)
        {
            return TranslationResult.Failure("Unexpected API response format");
        }

        return TranslationResult.Success(detectedSourceLanguage: "", targetText: content.Trim());
    }
}
