using FidoMfaServer.IdTokenHintValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Primitives;
using FidoMfaServer.Data;
using Microsoft.AspNetCore.Identity;

namespace FidoMfaServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IdTokenHintValidationConfiguration _idTokenHintValidationConfiguration;
    private readonly bool _testingMode;
    private readonly ApplicationDbContext _applicationDbContext;

    [BindProperty(SupportsGet = true)]
    public string? UserName { get; set; }

    public LoginModel(IOptions<IdTokenHintValidationConfiguration> idTokenHintValidationConfiguration,
         ApplicationDbContext applicationDbContext)
    {
        _idTokenHintValidationConfiguration = idTokenHintValidationConfiguration.Value;
        _applicationDbContext = applicationDbContext;
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

        var oid = idTokenHintValidationResult
            .ClaimsPrincipal.Claims.FirstOrDefault(o => o.Type == "oid");

        if(oid == null)
        {
            // throw 401
        }

        var user = _applicationDbContext.Users.FirstOrDefault(u => u.EntraIdOid == oid.Value);

        if (user == null)
        {
            // throw 401
        }

        UserName = user.UserName;
    }

    public void OnPost()
    {
    }
}
