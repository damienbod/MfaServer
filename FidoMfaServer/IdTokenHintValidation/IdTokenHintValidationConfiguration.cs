namespace FidoMfaServer.IdTokenHintValidation;

public class IdTokenHintValidationConfiguration
{
    public string MetadataAddress { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}
