# MFA Server

## Database

```
Add-Migration "init_identity_new" 
```

```
Update-Database
```

MFA Test UI

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