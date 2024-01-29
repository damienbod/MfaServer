using FidoMfaServer.IdTokenHintValidation;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace FidoMfaServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IdTokenHintValidationConfiguration _idTokenHintValidationConfiguration;
    private readonly bool _testingMode;

    public LoginModel(IOptions<IdTokenHintValidationConfiguration> idTokenHintValidationConfiguration)
    {
        _idTokenHintValidationConfiguration = idTokenHintValidationConfiguration.Value;
    }

    public async Task OnGetAsync([FromQuery] string? returnUrl)
    {
        string id_token_hint = null;
        if (returnUrl != null)
        {
            var queryParams = QueryHelpers.ParseQuery(returnUrl);
            StringValues stringValues;
            var exists = queryParams.TryGetValue("id_token_hint", out stringValues);
            if (exists)
            {
                id_token_hint = stringValues.FirstOrDefault();
            }
        }

        //get well known endpoints and validate access token sent in the assertion
        var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            _idTokenHintValidationConfiguration.MetadataAddress,
            new OpenIdConnectConfigurationRetriever());

        var wellKnownEndpoints = await configurationManager.GetConfigurationAsync();

        var idTokenHintValidationResult = ValidateIdTokenHintRequestPayload.ValidateTokenAndSignature(
            id_token_hint,
            _idTokenHintValidationConfiguration,
            wellKnownEndpoints.SigningKeys,
            false);

        if (!idTokenHintValidationResult.Valid)
        {
            // throw 401
        }

    }

    public void OnPost()
    {
    }
}
