namespace IdentityProvider.IdTokenHintValidation;

public class IdTokenHintValidationConfiguration
{

    public string IdTokenHintMetadataAddress { get; set; } = string.Empty;
    public string IdTokenAuthority { get; set; } = string.Empty;
    public string IdTokenAudience { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;

}
