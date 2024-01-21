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
    "appId": "4fabcfc0-5c44-45a1-8c80-8537f0625949", // external authentication app registration
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

## Known Issues

- Only a single FIDO2/passkey can be registered per user. In a productive system, multiple key registration must be possible.
- User would need a recovery in a productive system.

## Credits, non Microsoft libraries used in this setup

https://documentation.openiddict.com/

https://github.com/passwordless-lib/fido2-net-lib


