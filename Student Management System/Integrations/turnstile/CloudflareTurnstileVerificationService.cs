using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Student_Management_System.Configs;
using Student_Management_System.Services.Interfaces;

namespace Student_Management_System.Integrations.turnstile;

public class CloudflareTurnstileVerificationService : ITurnstileVerificationService
{
    private const string StudentSearchAction = "student_search";

    private readonly HttpClient _httpClient;
    private readonly ILogger<CloudflareTurnstileVerificationService> _logger;
    private readonly TurnstileOptions _options;

    public CloudflareTurnstileVerificationService(
        HttpClient httpClient,
        IOptions<TurnstileOptions> options,
        ILogger<CloudflareTurnstileVerificationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<bool> VerifyStudentSearchAsync(
        string token,
        string? remoteIp,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.SecretKey)
            || string.IsNullOrWhiteSpace(token)
            || token.Length > 2048)
        {
            _logger.LogWarning("Turnstile verification rejected because its configuration or token is missing.");
            return false;
        }

        var request = new Dictionary<string, string>
        {
            ["secret"] = _options.SecretKey,
            ["response"] = token
        };

        if (!string.IsNullOrWhiteSpace(remoteIp))
        {
            request["remoteip"] = remoteIp;
        }

        try
        {
            using var response = await _httpClient.PostAsync(
                "turnstile/v0/siteverify",
                new FormUrlEncodedContent(request),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Turnstile Siteverify returned HTTP status {StatusCode}.",
                    response.StatusCode);
                return false;
            }

            var verification = await response.Content.ReadFromJsonAsync<TurnstileVerificationResponse>(
                cancellationToken: cancellationToken);

            var hostnameMatches = string.IsNullOrWhiteSpace(_options.ExpectedHostname)
                || string.Equals(
                    verification?.Hostname,
                    _options.ExpectedHostname,
                    StringComparison.OrdinalIgnoreCase);

            var valid = verification?.Success == true
                && hostnameMatches
                && string.Equals(
                    verification.Action,
                    StudentSearchAction,
                    StringComparison.Ordinal);

            if (!valid)
            {
                _logger.LogWarning(
                    "Turnstile verification failed. Hostname: {Hostname}; Action: {Action}; Errors: {Errors}",
                    verification?.Hostname,
                    verification?.Action,
                    string.Join(',', verification?.ErrorCodes ?? []));
            }

            return valid;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exception, "Turnstile Siteverify request failed.");
            return false;
        }
    }

    private sealed record TurnstileVerificationResponse(
        [property: JsonPropertyName("success")] bool Success,
        [property: JsonPropertyName("hostname")] string? Hostname,
        [property: JsonPropertyName("action")] string? Action,
        [property: JsonPropertyName("error-codes")] string[]? ErrorCodes);
}
