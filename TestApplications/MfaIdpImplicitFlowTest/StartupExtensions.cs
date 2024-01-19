using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Serilog;
using System.Security.Cryptography.X509Certificates;

namespace MfaIdpImplicitFlowTest;

internal static class StartupExtensions
{
    private static IWebHostEnvironment? _env;

    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;
        _env = builder.Environment;

        services.AddHttpClient();
        services.Configure<AuthConfigurations>(configuration.GetSection("AuthConfigurations"));

        var authConfigurations = configuration.GetSection("AuthConfigurations");
        var mfaProvider = authConfigurations["MfaProvider"];

        services.AddDistributedMemoryCache();

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
            options.ClientSecret = "oidc-id_token-confidential_secret";
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

        services.AddAuthorization();

        services.AddControllersWithViews();

        return builder.Build();
    }

    private static async Task OnRedirectToIdentityProvider(RedirectContext context)
    {
        context.ProtocolMessage.IdTokenHint = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IjVCM25SeHRRN2ppOGVORGMzRnkwNUtmOTdaRSJ9.eyJhdWQiOiI1YzIwMWI2MC04OWY2LTQ3ZDgtYjJlZi05ZDlmZTJhNDI3NTEiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vN2ZmOTViMTUtZGMyMS00YmE2LWJjOTItODI0ODU2NTc4ZmMxL3YyLjAiLCJpYXQiOjE3MDU2NTg0MDIsIm5iZiI6MTcwNTY1ODQwMiwiZXhwIjoxNzA1NjYyMzAyLCJpZHAiOiJodHRwczovL3N0cy53aW5kb3dzLm5ldC9hNmJiYWI5Mi0wNTNlLTQ5MGItYmQ3ZS01Y2QwMzc2M2I3NDYvIiwibmFtZSI6IkRhbWllbiBCb3dkZW4iLCJub25jZSI6IjYzODQxMjU1NDk3NzkyNTYwOS5OVFE0WldGaU1HTXROREk0T1MwMFkyRTFMV0ppTldZdE5HSmhZV1JoTXpabFl6UTVOVFEwTjJZNU1EY3RaR00wWlMwME5tUTFMVGs0WmpjdFl6ZzRNamRsTXpVM05EWmwiLCJvaWQiOiI2OGI2MjU5Ny0wODllLTRjODAtODA0OC0yNjI5ZjE5NGY4MzAiLCJwcmVmZXJyZWRfdXNlcm5hbWUiOiJkYW1pZW4uYm93ZGVuQGlzb2x1dGlvbnMuY2giLCJyaCI6IjAuQVI4QUZWdjVmeUhjcGt1OGtvSklWbGVQd1dBYklGejJpZGhIc3UtZG4tS2tKMUdGQUNBLiIsInN1YiI6Ii1fRUQyLXlWRUZuYkpyZXVDOTNOLWU1V3hJVjlkQjhNMElpc3VZZElYcG8iLCJ0aWQiOiI3ZmY5NWIxNS1kYzIxLTRiYTYtYmM5Mi04MjQ4NTY1NzhmYzEiLCJ1dGkiOiJwQ0tqeWRGX19FaXN1MHZzVHMwVUFBIiwidmVyIjoiMi4wIn0.0szeyBecPJnJa43cU9W5R7dr5YSWOyW9B9FHtWNfnOkJ7XPh3a1ZEklUsvyEYavmkxBRrwwOsBtfyytlqEm6Gg0rtvwEJE4D5U-MxeNggeDJrVfQAzjJ0dOLx0IoNfPr12BhYf7lc-NiiVDiyk7UtnOQZ-cNj9aolc7SQG5uV68oc8JHZng8LbTEASlIGAMh5aqbqRyH-kXxpIbO3OeKWM3AZIyuYRf0IwDTn1CjfJv-4ILX122HZUPci_Fvuf7ZC1H4BwDAvrdkUHW714YdY92aa-PaStU32l5NpKt8A2dR9dPVH4IKt_PJL1DaZeYRntsAanC_q7LKZzrev_VvpQ";
    
        context.ProtocolMessage.Parameters
            .Add("claims", CreateClaimsIdTokenPayload.GenerateClaims());
    }

    public static WebApplication Configure(this WebApplication app)
    {
        IdentityModelEventSource.ShowPII = true;
        JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

        app.UseSerilogRequestLogging();

        app.UseSecurityHeaders(
            SecurityHeadersDefinitions.GetHeaderPolicyCollection(_env!.IsDevelopment()));

        if (_env!.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
        }

        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}"
        );
        return app;
    }
}
