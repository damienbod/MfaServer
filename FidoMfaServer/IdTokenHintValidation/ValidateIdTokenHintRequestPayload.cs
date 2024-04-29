using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Net.Mail;
using System.Security.Claims;

namespace FidoMfaServer.IdTokenHintValidation;

public static class ValidateIdTokenHintRequestPayload
{
    public static (bool Valid, string Reason, string Error) IsValid(ClaimsIdentity claimsIdTokenPrincipal, 
        IdTokenHintValidationConfiguration configuration, 
        string userEntraIdOid, 
        string userName)
    { 
        // oid from id_token_hint must match User OID
        var oid = claimsIdTokenPrincipal.FindFirst("oid").Value;
        if (!oid!.Equals(userEntraIdOid))
        {
            return (false, "oid parameter has an incorrect value",
                EntraIdTokenRequestConsts.ERROR_INVALID_CLIENT);
        };

        // aud must match allowed audience if for a specifc app
        if(configuration.ValidateAudience)
        {
            var aud = claimsIdTokenPrincipal.FindFirst("aud").Value;
            if (!aud!.Equals(configuration.Audience))
            {
                return (false, "client_id parameter has an incorrect value",
                    EntraIdTokenRequestConsts.ERROR_INVALID_CLIENT);
            };
        }    

        // tid must match allowed tenant
        var tid = claimsIdTokenPrincipal.FindFirst("tid").Value;
        if (!tid!.Equals(configuration.TenantId))
        {
            return (false, "tid parameter has an incorrect value",
                EntraIdTokenRequestConsts.ERROR_INVALID_CLIENT);
        };

        // preferred_username from id_token_hint
        var preferred_username = GetPreferredUserName(claimsIdTokenPrincipal);
        if (!preferred_username!.ToLower().Equals(userName.ToLower()))
        {
            return (false, "preferred_username parameter has an incorrect value",
                EntraIdTokenRequestConsts.ERROR_INVALID_CLIENT);
        };

        return (true, string.Empty, string.Empty);
    }

    public static async Task<(bool Valid, string Reason, TokenValidationResult TokenValidationResult)> ValidateTokenAndSignatureAsync(
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
                ValidateAudience = idTokenConfiguration.ValidateAudience,
                ValidAudience = idTokenConfiguration.Audience
            };

            if (testingMode)
            {
                //validationParameters.ValidateIssuerSigningKey = false;
                //validationParameters.ValidateIssuer = false;
                validationParameters.ValidateLifetime = false;
            }

            var tokenValidator = new JsonWebTokenHandler
            {
                MapInboundClaims = false
            };

            var tokenValidationResult = await tokenValidator.ValidateTokenAsync(jwtToken, validationParameters);

            if(!tokenValidationResult.IsValid)
            {
                return (tokenValidationResult.IsValid, tokenValidationResult.Exception!.Message, tokenValidationResult);
            }

            return (tokenValidationResult.IsValid, string.Empty, tokenValidationResult);
        }
        catch (Exception ex)
        {
            return (false, $"Id Token Authorization failed {ex.Message}", null);
        }
    }

    public static string GetPreferredUserName(ClaimsIdentity claimsIdentity)
    {
        var preferred_username = claimsIdentity.Claims.FirstOrDefault(t => t.Type == "preferred_username");
        return preferred_username?.Value ?? string.Empty;
    }

    public static string GetAzpacr(ClaimsIdentity claimsIdentity)
    {
        var azpacrClaim = claimsIdentity.Claims.FirstOrDefault(t => t.Type == "azpacr");
        return azpacrClaim?.Value ?? string.Empty;
    }

    public static string GetAzp(ClaimsIdentity claimsIdentity)
    {
        var azpClaim = claimsIdentity.Claims.FirstOrDefault(t => t.Type == "azp");
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
