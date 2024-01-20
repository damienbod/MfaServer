# FIDO2/passkeys MFA Server

[![.NET](https://github.com/damienbod/MfaServer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/damienbod/MfaServer/actions/workflows/dotnet.yml) [![Deploy to Entra ID](https://github.com/damienbod/MfaServer/actions/workflows/azure-webapps-dotnet-core.yml/badge.svg)](https://github.com/damienbod/MfaServer/actions/workflows/azure-webapps-dotnet-core.yml)

## Database

```
Add-Migration "init_identity_new" 
```

```
Update-Database
```

## MFA Server using FIDO

Update the app.settings to deploy

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

```
{
    "@odata.type": "#microsoft.graph.externalAuthenticationMethodConfiguration",
    "displayName": "FIDO2-passkeys-MFA",
    "appId": "4fabcfc0-5c44-45a1-8c80-8537f0625949", // remove external authentication app registration
    "openIdConnectSetting": {
        "clientId": "oidc-implicit-mfa-confidential",
        "discoveryUrl": "https://fidomfaserver.azurewebsites.net//.well-known/openid-configuration"
    },
    "includeTarget": { // switch this if only specific users are required
        "targetType": "group",
        "id": "all_users"
    }
}
```

## MFA Test UI

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

Use the id_token:

```csharp
private static async Task OnRedirectToIdentityProvider(RedirectContext context)
{
    context.ProtocolMessage.IdTokenHint = "eyJ... your-entra-id-id_token-goes-here";

    context.ProtocolMessage.Parameters
        .Add("claims", CreateClaimsIdTokenPayload.GenerateClaims());
}
```


## Links

https://documentation.openiddict.com/

https://mysignins.microsoft.com/