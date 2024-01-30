# FIDO2/passkeys MFA Server

_Microsoft Entra ID: external authentication methods_

[![.NET](https://github.com/damienbod/MfaServer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/damienbod/MfaServer/actions/workflows/dotnet.yml)  [![Deploy app to Entra ID Web App](https://github.com/damienbod/MfaServer/actions/workflows/azure-webapps-dotnet-core.yml/badge.svg)](https://github.com/damienbod/MfaServer/actions/workflows/azure-webapps-dotnet-core.yml)

The FIDO2/passkeys MFA Server is implemented using an ASP.NET Core Web application. The application uses ASP.NET Core Identity to store and persist the users in an Azure SQL database. The MFA server uses passwordless-lib fido2-net-lib to implement the FIDO2 authentication. OpenIddict is used to implement the OpenID Connect flow.

![flow](https://github.com/damienbod/MfaServer/blob/main/images/me-id_external-authn-flows_01.png)

## Setup Microsoft Entra ID

See the Microsoft Entra ID: external authentication methods documentation.

### Microsoft Graph

Microsoft Graph is used to create the Microsoft Entra ID: external authentication method.

Requires the delegated **Policy.ReadWrite.AuthenticationMethod** permission 

POST 

https://graph.microsoft.com/beta/policies/authenticationMethodsPolicy/authenticationMethodConfigurations

```
{
    "@odata.type": "#microsoft.graph.externalAuthenticationMethodConfiguration",
    "displayName": "--name-of-provider--", // Displayed in login
    "state": "enabled"
    "appId": "--app-registration-clientId--", // external authentication app registration, see docs
    "openIdConnectSetting": {
        "clientId": "--your-client_id-from-external-provider--",
        "discoveryUrl": "--your-external-provider-url--/.well-known/openid-configuration"
    },
    "includeTarget": { // switch this if only specific users are required
        "targetType": "group",
        "id": "all_users"
    }
}
```

The https://developer.microsoft.com/en-us/graph/graph-explorer can be used to run this.


## Setup FIDO2/passkeys MFA Server 

_(OpenIddict, fido2-net-lib, ASP.NET Core Identity)_

### Core Setup OpenIddict

The Microsoft Entra ID external authentication provider requires an OpenID connect server to interact with. OpenIddict is used to implement this. Any OpenID Connect implementation can be used, as long as you can customize the claims returned in the id_token. ASP.NET Core Identity is used to persist the users to the database.

See [OpenIddict](https://documentation.openiddict.com/guides/getting-started/creating-your-own-server-instance.html)

OpenID Connect implicit flow is used and in OpenIddict, this can be configured in the Worker class. An id_token_hint is used to send the user data from Microsoft Entra ID. 

Here is an example of a possible OpenIddict client setup:

```csharp
await manager.CreateAsync(new OpenIddictApplicationDescriptor
{
    ClientId = "oidc-implicit-mfa",
    ConsentType = ConsentTypes.Implicit,
    DisplayName = "OIDC Implicit Flow for MFA",
    DisplayNames =
    {
        [CultureInfo.GetCultureInfo("fr-FR")] = "Application cliente MVC"
    },
    RedirectUris =
    {
        new Uri("https://login.microsoftonline.com/common/federation/externalauthprovider")
    },
    Permissions =
    {
        Permissions.Endpoints.Authorization,
        Permissions.Endpoints.Revocation,
        Permissions.GrantTypes.Implicit,
        Permissions.ResponseTypes.IdToken,
        Permissions.Scopes.Email,
        Permissions.Scopes.Profile,
        Permissions.Scopes.Roles
    }
});
```

Microsoft Entra ID uses the **redirect_uri**: 

https://login.microsoftonline.com/common/federation/externalauthprovider

### Setup Fido2/passkeys

The FIDO2/passkeys authentication was implement using the [fido2-net-lib](https://github.com/passwordless-lib/fido2-net-lib) nuget package.

This was implemented using the [AspNetCoreIdentityFido2Passwordless](https://github.com/damienbod/AspNetCoreIdentityFido2Mfa/tree/main/AspNetCoreIdentityFido2Passwordless) implementation. You need to replace all the ASP.NET Core Identity Razor Pages and add the WebAuthn js scripts from the wwwroot.

The Fido2 appsettings configuration must be changed to match the server deployment.

```
"Fido2": {
    // This must match the deployment domain
    "ServerDomain": "fidomfaserver.azurewebsites.net",
    "ServerName": "FidoMfaServer",
    "Origins": [ "https://fidomfaserver.azurewebsites.net" ],
    "TimestampDriftTolerance": 300000,
    "MDSAccessKey": null
},
```

### Setup OpenIddict Implicit flow for ME-ID external authn

The default Implicit flow client handling needs to be adapted for the Microsoft Entra ID external authentication flow. This is implemented in the **AuthorizationController**. This server is only used for this purpose, if implementing this on an existing OpenID Connect server, you would need to leave the default for the other flows. 

The code implements the validation like in the Microsoft Entra ID documentation. It is important to validation the **id_token_hint** (including the signature) and to create the returned id_token with the extra claims and changed claims required by Microsoft Entra ID external authentication methods.

```csharp
case ConsentTypes.Implicit:
case ConsentTypes.External when authorizations.Any():
case ConsentTypes.Explicit when authorizations.Any() && !request.HasPrompt(Prompts.Consent):
    var principal = await _signInManager.CreateUserPrincipalAsync(user);

    // Note: in this sample, the granted scopes match the requested scope
    // but you may want to allow the user to uncheck specific scopes.
    // For that, simply restrict the list of scopes before calling SetScopes.
    principal.SetScopes(request.GetScopes());
    principal.SetResources(await _scopeManager.ListResourcesAsync(principal.GetScopes()).ToListAsync());

    // Automatically create a permanent authorization to avoid requiring explicit consent
    // for future authorization or token requests containing the same scopes.
    var authorization = authorizations.LastOrDefault();
    if (authorization == null)
    {
        authorization = await _authorizationManager.CreateAsync(
            principal: principal,
            subject: await _userManager.GetUserIdAsync(user),
            client: await _applicationManager.GetIdAsync(application),
            type: AuthorizationTypes.Permanent,
            scopes: principal.GetScopes());
    }

    principal.SetAuthorizationId(await _authorizationManager.GetIdAsync(authorization));

    //get well known endpoints and validate access token sent in the assertion
    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
        _idTokenHintValidationConfiguration.MetadataAddress,
        new OpenIdConnectConfigurationRetriever());

    var wellKnownEndpoints = await configurationManager.GetConfigurationAsync();

    var idTokenHintValidationResult = ValidateIdTokenHintRequestPayload.ValidateTokenAndSignature(
        request.IdTokenHint,
        _idTokenHintValidationConfiguration,
        wellKnownEndpoints.SigningKeys,
        _testingMode);

    if (!idTokenHintValidationResult.Valid)
    {
        return UnauthorizedValidationParametersFailed(idTokenHintValidationResult.Reason,
            "id_token_hint validation failed");
    }

    var requestedClaims = System.Text.Json.JsonSerializer.Deserialize<claims>(request.Claims);

    //principal.AddClaim("acr", "possessionorinherence");
    principal.AddClaim("acr", "fido");
    var sub = idTokenHintValidationResult.ClaimsPrincipal
        .Claims.First(d => d.Type == "sub");

    principal.RemoveClaims("sub");
    principal.AddClaim(sub.Type, sub.Value);

    var claims = principal.Claims.ToList();
    claims.Add(new Claim("amr", "[\"fido\"]", JsonClaimValueTypes.JsonArray));

    ClaimsPrincipal cp = new();
    cp.AddIdentity(new ClaimsIdentity(claims, principal.Identity.AuthenticationType));

    foreach (var claim in cp.Claims)
    {
        claim.SetDestinations(GetDestinations(claim, cp));
    }

    var (Valid, Reason, Error) = ValidateIdTokenHintRequestPayload
        .IsValid(idTokenHintValidationResult.ClaimsPrincipal, 
        _idTokenHintValidationConfiguration,
        user.EntraIdOid,
        user.UserName);

    if (!Valid)
    {
        return UnauthorizedValidationParametersFailed(Reason, Error);
    }

    return SignIn(cp, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);

```

The appsettings need to match your Microsoft Entra ID tenant and the used App registration

```
"IdTokenHintValidationConfiguration": {
    "MetadataAddress": "https://login.microsoftonline.com/--your-tenant-id--/v2.0/.well-known/openid-configuration",
    "Issuer": "https://login.microsoftonline.com/--your-tenant-id--/v2.0",
    // client_id from the app we allow, i.e. MerillApp App Registration
    // We can enable or disable this validation if app aud are to be accepted.
    "Audience": "--your-client-id.app-using-the-mfa--",
    // If this is true, Audience (App registration client_id) is validated.
    "ValidateAudience": "False", 
    "TenantId": "--your-tenant-id--"
},
```

The **IdTokenHintValidation** FidoMfaServer project in the  implements the different flow validations.

## Known Issues

- Only a single FIDO2/passkeys key can be registered per user. In a productive system, multiple key registration must be possible.
- User would need a recovery in a productive system.
- Need to add a SCIM import of users
- Register page should only allowed validated Microsoft Entra ID users and use the OID from the id_token

## Credits, non Microsoft projects used in this setup

https://documentation.openiddict.com/

https://github.com/passwordless-lib/fido2-net-lib

https://github.com/damienbod/AspNetCoreIdentityFido2Mfa


