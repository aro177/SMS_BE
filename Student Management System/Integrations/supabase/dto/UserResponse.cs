using System.Text.Json.Serialization;

namespace Student_Management_System.Integrations.supabase.dto
{
    public record UserResponse(
    [property: JsonPropertyName("id")]
    Guid Id,

    [property: JsonPropertyName("email")]
    string? Email,

    [property: JsonPropertyName("phone")]
    string? Phone,

    [property: JsonPropertyName("user_metadata")]
    Dictionary<string, object>? UserMetadata,

    [property: JsonPropertyName("app_metadata")]
    Dictionary<string, object>? AppMetadata,

    [property: JsonPropertyName("banned_until")]
    DateTimeOffset? BannedUntil,

    [property: JsonPropertyName("created_at")]
    DateTimeOffset? CreatedAt,

    [property: JsonPropertyName("last_sign_in_at")]
    DateTimeOffset? LastSignInAt
);
}
