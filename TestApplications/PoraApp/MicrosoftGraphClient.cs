using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;

namespace PoraApp;

public class MicrosoftGraphClient
{
    private readonly GraphServiceClient _graphServiceClient;

    /// <summary>
    /// Used directly in the confidential UI application
    /// Same App registration is used to add the graph permissions
    /// </summary>
    /// <param name="graphServiceClient"></param>
    public MicrosoftGraphClient(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    /// <summary>
    ///https://graph.microsoft.com/beta/policies/authenticationMethodsPolicy/authenticationMethodConfigurations
    ///{
    ///  "@odata.type": "#microsoft.graph.externalAuthenticationMethodConfiguration",
    ///  "displayName": "ILoveAuthN",
    ///  "appId": "600b719b-3766-4dc5-95a6-3c4a8dc31885",
    ///  "openIdConnectSetting": {
    ///    "clientId": "06a011bd-ec92-4404-80fb-db6d5ada8ee2",
    ///    "discoveryUrl": "https//iloveauthn.com/.well-known/openid-configuration"
    ///  }
    ///}
    /// </summary>
    /// <returns></returns>
    public async Task<AuthenticationMethodConfiguration?> CreateAuthenticationMethod()
    {
        var additionalData = new Dictionary<string, object>
        {
            { "displayName", "FIDO2-passkeys-MFA-t1" },
            { "appId", "c5684f52-769e-471a-b7b5-9b9a94af97d4" },
            { "openIdConnectSetting", System.Text.Json.JsonSerializer.Serialize(new openIdConnectSetting()) }
        };
        var user = await _graphServiceClient.AuthenticationMethodsPolicy
            .AuthenticationMethodConfigurations.PostAsync(new AuthenticationMethodConfiguration
            {
                AdditionalData = additionalData
            }, b => b.Options.WithScopes("Policy.ReadWrite.AuthenticationMethod"));

        return user;
    }
}

public class openIdConnectSetting
{
    public string clientId { get; set; } = "oidc-implicit-mfa-confidential";
    public string discoveryUrl { get; set; } = "https//iloveauthn.com/.well-known/openid-configuration";
}
