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

The Microsoft Entra ID external authentication provider requires an OpenID connect server to interact with. OpenIddict is used to implement this. Any OpenID Connect can be used, as long as you can customize the claims returned in the id_token. ASP.NET Core Identity is used to persist the users to the database.

See [OpenIddict](https://documentation.openiddict.com/guides/getting-started/creating-your-own-server-instance.html)

OpenID Connect implicit flow is used and in OpenIddict, this can be configured in the Worker class. An id_token_hint is used to send the user data from Microsoft Entra ID. 

Here is an example of a possible OpenIddict client setup

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
    PostLogoutRedirectUris =
    {
        new Uri("https://localhost:5001/signout-callback-oidc")
    },
    RedirectUris =
    {
        new Uri("https://localhost:5001/signin-oidc"), // local dev
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

### Setup Fido2/passkeys

[AspNetCoreIdentityFido2Passwordless](https://github.com/damienbod/AspNetCoreIdentityFido2Mfa/tree/main/AspNetCoreIdentityFido2Passwordless)

### Setup OpenIddict Implicit flow for ME-ID external authn

## Known Issues

- Only a single FIDO2/passkey can be registered per user. In a productive system, multiple key registration must be possible.
- User would need a recovery in a productive system.

## Credits, non Microsoft projects used in this setup

https://documentation.openiddict.com/

https://github.com/passwordless-lib/fido2-net-lib

https://github.com/damienbod/AspNetCoreIdentityFido2Mfa


