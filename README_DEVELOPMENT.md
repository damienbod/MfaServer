# FIDO2/passkeys MFA Server

[![.NET](https://github.com/damienbod/MfaServer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/damienbod/MfaServer/actions/workflows/dotnet.yml)  [![Deploy app to Entra ID Web App](https://github.com/damienbod/MfaServer/actions/workflows/azure-webapps-dotnet-core.yml/badge.svg)](https://github.com/damienbod/MfaServer/actions/workflows/azure-webapps-dotnet-core.yml)

## Database

```
Add-Migration "init_identity_new" 
```

```
Update-Database
```

## MFA Server using FIDO

Update the app.settings to deploy. The ServerDomain MUST match the deployed domain.

```
  "Fido2": {
    "ServerDomain": "localhost",
    "ServerName": "FidoMfaServer",
    "Origins": [ "https://localhost:44318" ],
    "TimestampDriftTolerance": 300000,
    "MDSAccessKey": null
  },
  "IdTokenHintValidationConfiguration": {
    "MetadataAddress": "https://login.microsoftonline.com/7ff95b15-dc21-4ba6-bc92-824856578fc1/v2.0/.well-known/openid-configuration",
    "Issuer": "https://login.microsoftonline.com/7ff95b15-dc21-4ba6-bc92-824856578fc1/v2.0",
    "Audience": "5c201b60-89f6-47d8-b2ef-9d9fe2a42751",
    "TenantId": "7ff95b15-dc21-4ba6-bc92-824856578fc1"
  },
  "TestMode": "false",
```

## Microsoft Graph 

POST 

https://graph.microsoft.com/beta/policies/authenticationMethodsPolicy/authenticationMethodConfigurations

Or PATCH 

https://graph.microsoft.com/beta/policies/authenticationMethodsPolicy/authenticationMethodConfigurations/7e3543fd-2045-459b-afa6-4a542d349c07

```
{
    "@odata.type": "#microsoft.graph.externalAuthenticationMethodConfiguration",
    "displayName": "FIDO2-passkeys-MFA",
    "state": "enabled"
    "appId": "4fabcfc0-5c44-45a1-8c80-8537f0625949", // external authentication app registration
    "openIdConnectSetting": {
        "clientId": "oidc-implicit-mfa-confidential",
        "discoveryUrl": "https://fidomfaserver.azurewebsites.net/.well-known/openid-configuration"
    },
    "includeTarget": { // switch this if only specific users are required
        "targetType": "group",
        "id": "all_users"
    }
}
```

## MFA Testing

The **MerillApp** is used to login to the Microsoft Entra ID application. The app can be used to get an id_token for testing or to test on the deployed tenant.
_
```csharp
.AddMicrosoftIdentityWebApp(options =>
{
    builder.Configuration.Bind("AzureAd", options);
    options.UsePkce = true;
    options.Events = new OpenIdConnectEvents();

    options.Events.OnTokenResponseReceived = async context =>
    {
        var idToken = context.TokenEndpointResponse.IdToken;
    };
},
```

### MfaIdpImplicitFlowTest application

The **MfaIdpImplicitFlowTest** is used to test the MFA server. This is what Microsoft Entra ID external MFA would request when using an external MFA server.

You can use this application to test the MFA server without ME-ID.

OIDC Implicit flow is used. Use the id_token:

```csharp
services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.SignInScheme = "Cookies";
    options.Authority = mfaProvider;
    options.RequireHttpsMetadata = true;
    options.ClientId = "oidc-implicit-mfa-confidential";
    options.ResponseType = "id_token";
    options.UsePkce = false;
    options.GetClaimsFromUserInfoEndpoint = false;
    options.Scope.Add("email");

    options.SaveTokens = true;

    options.Events = new OpenIdConnectEvents
    {
        OnRedirectToIdentityProvider = OnRedirectToIdentityProvider,
    };
});
```

The OnRedirectToIdentityProvider adds the payload as ME-ID would send:

```csharp
private static async Task OnRedirectToIdentityProvider(RedirectContext context)
{
    context.ProtocolMessage.IdTokenHint = "eyJ... your-entra-id-id_token-goes-here";

    context.ProtocolMessage.Parameters
        .Add("claims", CreateClaimsIdTokenPayload.GenerateClaims());
}
```

CreateClaimsIdTokenPayload.GenerateClaims() creates the following payload (from docs):

```json
{
    "id_token": {
        "acr": {
            "essential": true,
            "values":["possessionorinherence"]
        },
        "amr": {
            "essential": true,
        }
    }
}
```

## Links

https://learn.microsoft.com/en-gb/entra/identity/authentication/concept-authentication-external-method-provider

https://learn.microsoft.com/en-gb/entra/identity/authentication/how-to-authentication-external-method-manage

https://techcommunity.microsoft.com/t5/microsoft-entra-blog/public-preview-external-authentication-methods-in-microsoft/ba-p/4078808

https://documentation.openiddict.com/

https://github.com/passwordless-lib/fido2-net-lib

https://mysignins.microsoft.com/

https://developer.microsoft.com/en-us/graph/graph-explorer

https://github.com/damienbod/AspNetCoreIdentityFido2Mfa