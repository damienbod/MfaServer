using FidoMfaServer.Data;
using FidoMfaServer.IdTokenHintValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace FidoMfaServer.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly IdTokenHintValidationConfiguration _idTokenHintValidationConfiguration;
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly bool _testingMode;

    [BindProperty(SupportsGet = true)]
    public string? UserName { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Name { get; set; }

    public LoginModel(IOptions<IdTokenHintValidationConfiguration> idTokenHintValidationConfiguration,
        IConfiguration configuration, ApplicationDbContext applicationDbContext)
    {
        _idTokenHintValidationConfiguration = idTokenHintValidationConfiguration.Value;
        _applicationDbContext = applicationDbContext;

        _testingMode = configuration.GetValue<bool>("TestMode");
    }

    public async Task OnGetAsync([FromQuery] string? returnUrl)
    {
        string id_token_hint = null;
        if (returnUrl != null)
        {
            var queryParams = QueryHelpers.ParseQuery(returnUrl);
            var exists = queryParams.TryGetValue("id_token_hint", out StringValues stringValues);
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

        // For prod deployments, the aud, iss, tid, exp claims MUST be validated as well as the signature.
        var idTokenHintValidationResult = await ValidateIdTokenHintRequestPayload.ValidateTokenAndSignatureAsync(
            id_token_hint,
            _idTokenHintValidationConfiguration,
            wellKnownEndpoints.SigningKeys,
            _testingMode);

        if (!idTokenHintValidationResult.Valid)
        {
            throw new UnauthorizedAccessException($"invalid id_token: {idTokenHintValidationResult.TokenValidationResult}");
        }

        var oid = ValidateIdTokenHintRequestPayload.GetOid(
            idTokenHintValidationResult.TokenValidationResult.ClaimsIdentity);


        Name = idTokenHintValidationResult.TokenValidationResult.ClaimsIdentity
           .Claims.FirstOrDefault(n => n.Type == "name")!.Value;

        if (oid == null)
        {
            throw new UnauthorizedAccessException("invalid id_token, missing oid");
        }

        // Validate tid if the MFA server should only accept users from allowed specific tenants
        // !!! If the tid is not validated, the users are open to phishing attacks. !!!
        //var tid = idTokenHintValidationResult.TokenValidationResult.ClaimsIdentity
        //  .Claims.FirstOrDefault(o => o.Type == "tid");
        // validate tid claim ...

        var user = _applicationDbContext.Users.FirstOrDefault(u => u.EntraIdOid == oid);

        if (user == null)
        {
            throw new UnauthorizedAccessException("no user found");
        }

        UserName = user.UserName;
    }

    public void OnPost()
    {
    }
}
