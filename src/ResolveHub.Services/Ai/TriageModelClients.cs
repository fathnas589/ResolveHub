namespace ResolveHub.Services.Ai;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Anthropic;
using Anthropic.Exceptions;
using Anthropic.Models.Beta;
using Anthropic.Models.Beta.Messages;

/// <summary>Why an AI call failed, so the API can return a friendly status code.</summary>
public enum AiFailureKind { RateLimited, Declined, Unavailable }

public sealed class AiProviderException : Exception
{
    public AiProviderException(AiFailureKind kind, string message, Exception? inner = null) : base(message, inner) => Kind = kind;
    public AiFailureKind Kind { get; }
}

/// <summary>A model that returns a JSON object matching a JSON Schema.</summary>
public interface ITriageModelClient
{
    /// <summary>Display name of the provider/model, e.g. "Gemini (gemini-3.5-flash-lite)".</summary>
    string Name { get; }

    Task<string> GenerateJsonAsync(string systemPrompt, string userContent, Dictionary<string, object> jsonSchema, CancellationToken cancellationToken);
}

/// <summary>Google Gemini via the REST generateContent endpoint (works on the free tier).</summary>
public sealed class GeminiTriageClient : ITriageModelClient
{
    private readonly HttpClient _http;
    private readonly string _apiKey;
    private readonly string _model;

    public GeminiTriageClient(HttpClient http, string apiKey, string model)
    {
        _http = http;
        _apiKey = apiKey;
        _model = model;
    }

    public string Name => $"Gemini ({_model})";

    public async Task<string> GenerateJsonAsync(string systemPrompt, string userContent, Dictionary<string, object> jsonSchema, CancellationToken cancellationToken)
    {
        var body = new
        {
            systemInstruction = new { parts = new[] { new { text = systemPrompt } } },
            contents = new[] { new { role = "user", parts = new[] { new { text = userContent } } } },
            generationConfig = new
            {
                responseMimeType = "application/json",
                responseJsonSchema = jsonSchema,
            },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post,
            $"https://generativelanguage.googleapis.com/v1beta/models/{Uri.EscapeDataString(_model)}:generateContent")
        {
            Content = JsonContent.Create(body),
        };
        request.Headers.Add("x-goog-api-key", _apiKey); // header keeps the key out of URLs and logs

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex)
        {
            throw new AiProviderException(AiFailureKind.Unavailable, "Could not reach Gemini.", ex);
        }

        using (response)
        {
            string raw = await response.Content.ReadAsStringAsync(cancellationToken);
            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                throw new AiProviderException(AiFailureKind.RateLimited, "Gemini free-tier limit reached.");
            }
            if (!response.IsSuccessStatusCode)
            {
                throw new AiProviderException(AiFailureKind.Unavailable, $"Gemini returned {(int)response.StatusCode}: {Truncate(raw)}");
            }

            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;

            // A blocked prompt returns no candidates.
            if (root.TryGetProperty("promptFeedback", out var feedback) &&
                feedback.TryGetProperty("blockReason", out var blockReason))
            {
                throw new AiProviderException(AiFailureKind.Declined, $"Gemini blocked the prompt ({blockReason.GetString()}).");
            }

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                throw new AiProviderException(AiFailureKind.Unavailable, "Gemini returned no candidates.");
            }

            var candidate = candidates[0];
            string finishReason = candidate.TryGetProperty("finishReason", out var fr) ? fr.GetString() ?? "" : "";
            if (finishReason is not ("" or "STOP"))
            {
                throw new AiProviderException(
                    finishReason == "MAX_TOKENS" ? AiFailureKind.Unavailable : AiFailureKind.Declined,
                    $"Gemini stopped early ({finishReason}).");
            }

            // Concatenate the answer text, skipping any "thought" parts.
            var text = new StringBuilder();
            if (candidate.TryGetProperty("content", out var content) && content.TryGetProperty("parts", out var parts))
            {
                foreach (var part in parts.EnumerateArray())
                {
                    bool isThought = part.TryGetProperty("thought", out var t) && t.ValueKind == JsonValueKind.True;
                    if (!isThought && part.TryGetProperty("text", out var p))
                    {
                        text.Append(p.GetString());
                    }
                }
            }
            return text.ToString();
        }
    }

    private static string Truncate(string s) => s.Length <= 300 ? s : s[..300] + "…";
}

/// <summary>Anthropic Claude via the official SDK (paid API).</summary>
public sealed class ClaudeTriageClient : ITriageModelClient
{
    private readonly AnthropicClient _client;
    private readonly string _model;

    public ClaudeTriageClient(AnthropicClient client, string model)
    {
        _client = client;
        _model = model;
    }

    public string Name => $"Claude ({_model})";

    public async Task<string> GenerateJsonAsync(string systemPrompt, string userContent, Dictionary<string, object> jsonSchema, CancellationToken cancellationToken)
    {
        var schema = jsonSchema.ToDictionary(kv => kv.Key, kv => JsonSerializer.SerializeToElement(kv.Value));
        BetaMessage response;
        try
        {
            response = await _client.Beta.Messages.Create(new MessageCreateParams
            {
                Model = _model,
                MaxTokens = 2048,
                System = systemPrompt,
                Messages = [new BetaUserMessageParam { Content = userContent }],
                OutputConfig = new BetaOutputConfig
                {
                    Effort = Effort.Low, // simple classification - low effort keeps it fast
                    Format = new BetaJsonOutputFormat { Schema = schema },
                },
                // Server-side fallback: a request declined by a safety classifier is re-served by a fallback model.
                Betas = [AnthropicBeta.ServerSideFallback2026_07_01],
                Fallbacks = new Default(),
            }, cancellationToken);
        }
        catch (AnthropicRateLimitException ex)
        {
            throw new AiProviderException(AiFailureKind.RateLimited, "Claude rate limit reached.", ex);
        }
        catch (AnthropicApiException ex)
        {
            throw new AiProviderException(AiFailureKind.Unavailable, "Claude API error.", ex);
        }

        if (response.StopReason == "refusal")
        {
            throw new AiProviderException(AiFailureKind.Declined, "Claude declined the request.");
        }

        return response.Content.Select(b => b.Value).OfType<BetaTextBlock>().FirstOrDefault()?.Text ?? string.Empty;
    }
}
