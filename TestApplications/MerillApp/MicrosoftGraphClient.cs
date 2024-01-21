using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;

namespace MerillApp;

public class MicrosoftGraphClient
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ITokenAcquisition _tokenAcquisition;

    public MicrosoftGraphClient(IHttpClientFactory clientFactory,
       ITokenAcquisition tokenAcquisition)
    {
        _clientFactory = clientFactory;
        _tokenAcquisition = tokenAcquisition;
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
    public async Task<string> CreateAuthenticationMethodV2(CreateAuthenticationMethodDto model)
    {
        var createAuthenticationMethodModel = new CreateAuthenticationMethodModel
        {
            openIdConnectSetting = new openIdConnectSetting
            {
                discoveryUrl = model.DiscoveryUrl,
                clientId = model.ClientId,
            },
            displayName = model.DisplayName,
            appId = model.AppRegistrationId
        };

        try
        {
            var client = _clientFactory.CreateClient();
            var baseAddress = "https://graph.microsoft.com/beta";

            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new List<string> { "Policy.ReadWrite.AuthenticationMethod" });

            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.PostAsJsonAsync("https://graph.microsoft.com/beta/policies/authenticationMethodsPolicy/authenticationMethodConfigurations",
                createAuthenticationMethodModel);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = $"Microsoft Graph response: {responseContent}";

                return data;
            }

            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Exception {e}");
        }
    }
}

public class CreateAuthenticationMethodModel
{
    [JsonPropertyName("@odata.type")]
    public string OdataType { get; set; } = "#microsoft.graph.externalAuthenticationMethodConfiguration";
    public string displayName { get; set; } = "FIDO2-passkeys-MFA";
    public string state { get; set; } = "enabled";
    public string appId { get; set; } = "c5684f52-769e-471a-b7b5-9b9a94af97d4";
    public openIdConnectSetting openIdConnectSetting { get; set; } = new openIdConnectSetting();
    public includeTarget includeTarget { get; set; } = new includeTarget(); 
}

public class openIdConnectSetting
{
    public string clientId { get; set; } = "oidc-implicit-mfa-confidential";
    public string discoveryUrl { get; set; } = "https://fidomfaserver.azurewebsites.net/.well-known/openid-configuration";
}

public class includeTarget
{
    public string clientId { get; set; } = "group";
    public string id { get; set; } = "all_users";
}

