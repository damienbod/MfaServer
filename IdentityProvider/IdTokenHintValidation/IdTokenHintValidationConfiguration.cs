﻿using System.Text.Json.Serialization;

namespace IdentityProvider.IdTokenHintValidation;

public class IdTokenHintValidationConfiguration
{
    // assertion parameter token validation
    [JsonPropertyName("AccessTokenMetadataAddress")]
    public string AccessTokenMetadataAddress { get; set; } = string.Empty;
    [JsonPropertyName("AccessTokenAuthority")]
    public string AccessTokenAuthority { get; set; } = string.Empty;
    [JsonPropertyName("AccessTokenAudience")]
    public string AccessTokenAudience { get; set; } = string.Empty;

    // request parameters
    [JsonPropertyName("Audience")]
    public string Audience { get; set; } = string.Empty;
    [JsonPropertyName("ClientSecret")]
    public string ClientSecret { get; set; } = string.Empty;
    [JsonPropertyName("ClientId")]
    public string ClientId { get; set; } = string.Empty;
    [JsonPropertyName("ScopeForNewAccessToken")]
    public string ScopeForNewAccessToken { get; set; } = string.Empty;

    // new token claims
    [JsonPropertyName("AudienceForNewAccessToken")]
    public string AudienceForNewAccessToken { get; set; } = string.Empty;

    [JsonPropertyName("IssuerForNewAccessToken")]
    public string IssuerForNewAccessToken { get; set; } = string.Empty;
}
