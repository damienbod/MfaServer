using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;

namespace MfaIdpImplicitFlowTest;

public class CreateDelegatedTestIdTokenPayloadModel
{
    public string Version { get; set; } = "2.0";
    public string Sub { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PreferredUsername { get; set; } = string.Empty;
    public string Oid { get; set; } = string.Empty;
    public string Tid { get; set; } = string.Empty;

    public X509Certificate2? SigningCredentials { get; set; }
}
