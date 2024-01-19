using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace MfaIdpImplicitFlowTest;

public static class CreateIdTokenHintPayload
{
    public static string GenerateJwtTokenAsync(CreateIdTokenHintPayloadModel payload)
    {
        SigningCredentials signingCredentials = new X509SigningCredentials(payload.SigningCredentials);

        var alg = signingCredentials.Algorithm;

        //{
        //  "alg": "RS256",
        //  "kid": "....",
        //  "typ": "jwt",
        //}

        var subject = new ClaimsIdentity(new[] {
            new Claim("ver", payload.Version),
            new Claim("iss", payload.Issuer),
            new Claim("sub", payload.Sub),
            new Claim("aud", payload.Audience),
            new Claim("name", payload.Name),
            new Claim("preferred_username", payload.PreferredUsername),
            new Claim("oid", payload.Oid),
            new Claim("tid", payload.Tid)

            //new Claim("act", $"{{ \"sub\": \"{payload.OriginalClientId}\" }}", JsonClaimValueTypes.Json )
        });


        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = subject,
            Expires = DateTime.UtcNow.AddHours(1),
            IssuedAt = DateTime.UtcNow,
            Issuer = payload.Issuer,
            Audience = payload.Audience,
            SigningCredentials = signingCredentials,
            TokenType = "jwt"
        };

        tokenDescriptor.AdditionalHeaderClaims ??= new Dictionary<string, object>();

        if (!tokenDescriptor.AdditionalHeaderClaims.ContainsKey("alg"))
        {
            tokenDescriptor.AdditionalHeaderClaims.Add("alg", alg);
        }

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
