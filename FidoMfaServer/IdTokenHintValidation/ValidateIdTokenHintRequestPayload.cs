using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Security.Claims;

namespace FidoMfaServer.IdTokenHintValidation;

public static class ValidateIdTokenHintRequestPayload
{
    public static (bool Valid, string Reason, string Error) IsValid(ClaimsPrincipal claimsIdTokenPrincipal, 
        IdTokenHintValidationConfiguration configuration, 
        string userEntraIdOid, 
        string userName)
    { 
        //_idTokenHintValidationConfiguration.ClientId == request.ClientId;

        // oid from id_token_hint must match User OID
        var oid = claimsIdTokenPrincipal.FindFirst("oid").Value;
        if (!oid!.Equals(userEntraIdOid))
        {
            return (false, "oid parameter has an incorrect value",
                EntraIdTokenRequestConsts.ERROR_INVALID_CLIENT);
        };

        // aud must match allowed audience
        var aud = claimsIdTokenPrincipal.FindFirst("aud").Value;
        if (!aud!.Equals(configuration.Audience))
        {
            return (false, "client_id parameter has an incorrect value",
                EntraIdTokenRequestConsts.ERROR_INVALID_CLIENT);
        };

        // tid must match allowed tenant
        var tid = claimsIdTokenPrincipal.FindFirst("tid").Value;
        if (!tid!.Equals(configuration.TenantId))
        {
            return (false, "tid parameter has an incorrect value",
                EntraIdTokenRequestConsts.ERROR_INVALID_CLIENT);
        };

        // preferred_username from id_token_hint
        var preferred_username = claimsIdTokenPrincipal.FindFirst("preferred_username").Value;
        if (!preferred_username!.ToLower().Equals(userName.ToLower()))
        {
            return (false, "preferred_username parameter has an incorrect value",
                EntraIdTokenRequestConsts.ERROR_INVALID_CLIENT);
        };

        return (true, string.Empty, string.Empty);
    }

    public static (bool Valid, string Reason, ClaimsPrincipal ClaimsPrincipal) ValidateTokenAndSignature(
        string jwtToken,
        IdTokenHintValidationConfiguration idTokenConfiguration,
        ICollection<SecurityKey> signingKeys,
        bool testingMode)
    {
        try
        {
            var validationParameters = new TokenValidationParameters
            {
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(1),
                RequireSignedTokens = true,
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = signingKeys,
                ValidateIssuer = true,
                ValidIssuer = idTokenConfiguration.Issuer,
                ValidateAudience = true,
                ValidAudience = idTokenConfiguration.Audience
            };

            if (testingMode)
            {
                validationParameters.ValidateIssuerSigningKey = false;
            }

            ISecurityTokenValidator tokenValidator = new JwtSecurityTokenHandler();

            var claimsPrincipal = tokenValidator.ValidateToken(jwtToken, validationParameters, out var _);

            return (true, string.Empty, claimsPrincipal);
        }
        catch (Exception ex)
        {
            return (false, $"Id Token Authorization failed {ex.Message}", null);
        }
    }

    public static string GetPreferredUserName(ClaimsPrincipal claimsPrincipal)
    {
        var preferred_username = claimsPrincipal.Claims.FirstOrDefault(t => t.Type == "preferred_username");
        return preferred_username?.Value ?? string.Empty;
    }

    public static string GetAzpacr(ClaimsPrincipal claimsPrincipal)
    {
        var azpacrClaim = claimsPrincipal.Claims.FirstOrDefault(t => t.Type == "azpacr");
        return azpacrClaim?.Value ?? string.Empty;
    }

    public static string GetAzp(ClaimsPrincipal claimsPrincipal)
    {
        var azpClaim = claimsPrincipal.Claims.FirstOrDefault(t => t.Type == "azp");
        return azpClaim?.Value ?? string.Empty;
    }

    public static bool IsEmailValid(string email)
    {
        if (!MailAddress.TryCreate(email, out var mailAddress))
            return false;

        // And if you want to be more strict:
        var hostParts = mailAddress.Host.Split('.');
        if (hostParts.Length == 1)
            return false; // No dot.
        if (hostParts.Any(p => p == string.Empty))
            return false; // Double dot.
        if (hostParts[^1].Length < 2)
            return false; // TLD only one letter.

        if (mailAddress.User.Contains(' '))
            return false;
        if (mailAddress.User.Split('.').Any(p => p == string.Empty))
            return false; // Double dot or dot at end of user part.

        return true;
    }
}
