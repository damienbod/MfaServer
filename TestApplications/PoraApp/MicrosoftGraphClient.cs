using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

namespace MerillApp;

public class MicrosoftGraphClient
{
    private readonly GraphServiceClient _graphServiceClient;
    private readonly IHttpClientFactory _clientFactory;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;

    public MicrosoftGraphClient(GraphServiceClient graphServiceClient, 
        IHttpClientFactory clientFactory,
       ITokenAcquisition tokenAcquisition,
       IConfiguration configuration)
    {
        _graphServiceClient = graphServiceClient;
        _clientFactory = clientFactory;
        _tokenAcquisition = tokenAcquisition;
        _configuration = configuration;
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
    public async Task<AuthenticationMethodConfiguration?> CreateAuthenticationMethod()
    {
        var oidcSetting = System.Text.Json.JsonSerializer.Serialize(new openIdConnectSetting());
        var additionalData = new Dictionary<string, object>
        {
            { "displayName", "FIDO2-passkeys-MFA-b8dcfa58960c.ngrok.app" },
            { "appId", "c5684f52-769e-471a-b7b5-9b9a94af97d4" },
            { "openIdConnectSetting", oidcSetting }
        };
        var created = await _graphServiceClient.AuthenticationMethodsPolicy
            .AuthenticationMethodConfigurations.PostAsync(new AuthenticationMethodConfiguration
            {
                AdditionalData = additionalData, 
                OdataType = "#microsoft.graph.externalAuthenticationMethodConfiguration",
            }, b => b.Options.WithScopes("Policy.ReadWrite.AuthenticationMethod"));

        return created;
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
    public async Task<string> CreateAuthenticationMethodV2()
    {
        try
        {
            var client = _clientFactory.CreateClient();
            var baseAddress = "https://graph.microsoft.com/beta";

            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new List<string> { "Policy.ReadWrite.AuthenticationMethod" });

            client.BaseAddress = new Uri(baseAddress);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var json = System.Text.Json.JsonSerializer.Serialize(new CreateAuthenticationMethodMethod());
            var response = await client.PostAsJsonAsync("https://graph.microsoft.com/beta/policies/authenticationMethodsPolicy/authenticationMethodConfigurations",
                new CreateAuthenticationMethodMethod());

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = $"Graph API user name response: {responseContent}";

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

public class CreateAuthenticationMethodMethod
{
    [JsonPropertyName("@odata.type")]
    public string OdataType { get; set; } = "#microsoft.graph.externalAuthenticationMethodConfiguration";
    public string displayName { get; set; } = "FIDO2-passkeys-MFA-b8dcfa58960c.ngrok.app";
    public string appId { get; set; } = "c5684f52-769e-471a-b7b5-9b9a94af97d4";
    public openIdConnectSetting openIdConnectSetting { get; set; } = new openIdConnectSetting();
}

public class openIdConnectSetting
{
    public string clientId { get; set; } = "oidc-implicit-mfa-confidential";
    public string discoveryUrl { get; set; } = "https://b8dcfa58960c.ngrok.app/.well-known/openid-configuration";
}
