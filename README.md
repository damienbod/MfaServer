# FIDO MFA Server

_Microsoft Entra ID: external authentication methods_

[![.NET](https://github.com/damienbod/MfaServer/actions/workflows/dotnet.yml/badge.svg)](https://github.com/damienbod/MfaServer/actions/workflows/dotnet.yml)  [![Deploy app to Entra ID Web App](https://github.com/damienbod/MfaServer/actions/workflows/azure-webapps-dotnet-core.yml/badge.svg)](https://github.com/damienbod/MfaServer/actions/workflows/azure-webapps-dotnet-core.yml)

The MFA Server is implemented using an ASP.NET Core Web application. The application uses ASP.NET Core Identity to store and persist the users in an Azure SQL database. The MFA server uses passwordless-lib fido2-net-lib to implement the FIDO2 authentication. OpenIddict is used to implement the OpenID Connect flow.

![flow](https://github.com/damienbod/MfaServer/blob/main/images/me-id_external-authn-flows_01.png)

## Setup Microsoft Entra ID

See the Microsoft Entra ID: external authentication methods documentation.

## Setup MFA Server (OpenIddict, fido2-net-lib, ASP.NET Core Identity)

## Known Issues

- Only a single FIDO2/passkey can be registered per user. In a productive system, multiple key registration must be possible.
- User would need a recovery in a productive system.

## Credits, non Microsoft libraries used in this setup

https://documentation.openiddict.com/

https://github.com/passwordless-lib/fido2-net-lib


