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
        context.ProtocolMessage.IdTokenHint = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IkwxS2ZLRklfam5YYndXYzIyeFp4dzFzVUhIMCJ9.eyJhdWQiOiIwYWE4M2RkZi0wOWEwLTQ0ZjUtYTlmNC1iNzA0NmE4NmJlODkiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vMTg1MmIxMGYtYTAxMS00MjhiLTk4ZjktZDA5YzM3ZDQ3N2NmL3YyLjAiLCJpYXQiOjE3MTQzODUyMDAsIm5iZiI6MTcxNDM4NTIwMCwiZXhwIjoxNzE0Mzg5MTAwLCJuYW1lIjoiRGFtaWVuIEJvd2RlbiIsIm5vbmNlIjoiNjM4NDk5ODIyNzQ3MDIxMzI3LllXVTFORFl5TVdJdE5tUmxPUzAwWldVeExXSXhaR1l0TUdVMVpUWTRPVGRpTW1ZellXVTBaamhpTmpjdFpUSmtaUzAwT0Rka0xUazFNREl0WldZMU5UbGhPREl4TjJaaiIsIm9pZCI6IjBkN2Y4MGE2LTQwNzMtNGFkYS04MGQ0LWU2MWNmMDA2MjdmNyIsInByZWZlcnJlZF91c2VybmFtZSI6ImRhbWllbi5ib3dkZW5AbWVyaWxsLm9yZyIsInJoIjoiMC5BUU1BRDdGU0dCR2dpMEtZLWRDY045UjN6OTg5cUFxZ0NmVkVxZlMzQkdxR3Zva0JBWkkuIiwic3ViIjoiZGs2M0dQM3VmbF9iM3JzOGpjZDNNN0pxS1ZnYlNLYnZKY1NVd3hWZnYwWSIsInRpZCI6IjE4NTJiMTBmLWEwMTEtNDI4Yi05OGY5LWQwOWMzN2Q0NzdjZiIsInV0aSI6IlY4d2ZuX0phQlV1bExaemxSWU5vQUEiLCJ2ZXIiOiIyLjAifQ.H2DNNwXlYrrIJiBgj100PGKBHlYBieTmuHxUkNMEOrN1lD1Hu4c0kq0iJe3fp5OIONHWiu9Gq1F_tDjcZoGr58u3fWYSRRIrncLZAdUKcPP-cKIMVYO0HydJLKh_M2HMH94MvbPFDanXivl2sox9V86bGro8TshWMZJKreV-JJyg_GRUnk8SWzcAVsqNGwP2nYB-mMCWO1Vq_ItUa6HA7aFHj0mjMUExQFCP5bYcPv9TFJu3MHv0ISR-pXIepFSdwINZO0m8IcPweRU0rJppe6TleR0EGONkGr5Idt6RMvFIh4ldBJTx5cC4V52RFBICcB2MSveVTIJxifZRD9dKKA";
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
