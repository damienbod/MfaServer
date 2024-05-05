namespace MerillApp;

public class CreateAuthenticationMethodDto
{
    public string DisplayName { get; set; } = "MFA server using FIDO2";
    public string AppRegistrationId { get; set; } = "c5684f52-769e-471a-b7b5-9b9a94af97d4";
    public string ClientId { get; set; } = "oidc-implicit-mfa";
    public string DiscoveryUrl { get; set; } = "https://fidomfaserver.azurewebsites.net/.well-known/openid-configuration";
}