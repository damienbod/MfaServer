using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Logging;
using Serilog;

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
        context.ProtocolMessage.IdTokenHint = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IkwxS2ZLRklfam5YYndXYzIyeFp4dzFzVUhIMCJ9.eyJhdWQiOiIwYWE4M2RkZi0wOWEwLTQ0ZjUtYTlmNC1iNzA0NmE4NmJlODkiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vMTg1MmIxMGYtYTAxMS00MjhiLTk4ZjktZDA5YzM3ZDQ3N2NmL3YyLjAiLCJpYXQiOjE3MTQzOTY5NDUsIm5iZiI6MTcxNDM5Njk0NSwiZXhwIjoxNzE0NDAwODQ1LCJuYW1lIjoiRGFtaWVuIEJvd2RlbiIsIm5vbmNlIjoiNjM4NDk5OTM5NjE5MTI2MzQwLk5EVm1ZVGRtTmpFdFpUWm1aUzAwWlRJMExUZ3hNREF0TjJRd1kyUXlOV1U1WTJGaVpURXpOVEJrWlRNdE5qaGtNeTAwWm1ZeExUa3dOVGd0T0Rnd05UaGlZelprTURCaiIsIm9pZCI6IjBkN2Y4MGE2LTQwNzMtNGFkYS04MGQ0LWU2MWNmMDA2MjdmNyIsInByZWZlcnJlZF91c2VybmFtZSI6ImRhbWllbi5ib3dkZW5AbWVyaWxsLm9yZyIsInJoIjoiMC5BUU1BRDdGU0dCR2dpMEtZLWRDY045UjN6OTg5cUFxZ0NmVkVxZlMzQkdxR3Zva0JBWkkuIiwic3ViIjoiZGs2M0dQM3VmbF9iM3JzOGpjZDNNN0pxS1ZnYlNLYnZKY1NVd3hWZnYwWSIsInRpZCI6IjE4NTJiMTBmLWEwMTEtNDI4Yi05OGY5LWQwOWMzN2Q0NzdjZiIsInV0aSI6Ikx5M3A5TzZ6Z0UySmR3cTkxU3FBQVEiLCJ2ZXIiOiIyLjAifQ.iunxydt45CQv149mLq3Fw6nyoWATfrIsdjZGZJdPU76bWPmkFjSOc5oaXaBBBlpp9l8c6Vi9TQ2BY31IWoKhHKVTyDOaE84pvIooK9PTgYWtO242gb3CCxRkJGU5GkSJfs2A-yl_ZtNfTpDpI_tsGFgU9ufnhrQjoZH0FNk69W3ULEOFeGuxaWz2sgIpSwayUbXHh8r5XD-yPjsPcB17yb-lVObGfMN684pgEfm6ZqMaYrDAXFQbn9gWJY7lvwY0PBNGWUGpcnnWErFpMkVhr9gJCdeYpN5gM0GJfI8vfF_FCUr5o6ENwSNnWew565WJThKVkj3VK1-rQa8L6Lnbsg";
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
