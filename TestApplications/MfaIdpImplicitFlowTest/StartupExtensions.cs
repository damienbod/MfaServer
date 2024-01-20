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
        context.ProtocolMessage.IdTokenHint = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IjVCM25SeHRRN2ppOGVORGMzRnkwNUtmOTdaRSJ9.eyJhdWQiOiIwYWE4M2RkZi0wOWEwLTQ0ZjUtYTlmNC1iNzA0NmE4NmJlODkiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vMTg1MmIxMGYtYTAxMS00MjhiLTk4ZjktZDA5YzM3ZDQ3N2NmL3YyLjAiLCJpYXQiOjE3MDU3MzY0NzAsIm5iZiI6MTcwNTczNjQ3MCwiZXhwIjoxNzA1NzQwMzcwLCJuYW1lIjoiRGFtaWVuIEJvd2RlbiIsIm5vbmNlIjoiNjM4NDEzMzM1NjI1MTg5ODIxLk9UTTNZMlV4TldRdFpHSTVZeTAwTW1JMkxUZzNZalF0TjJOa1ltSTFOREpqWlRsbU1UZG1NakkzWWpRdFlXRXdOUzAwTkdVMUxXRTRNamN0WXpVek1HUTJNakUxWkRjeSIsIm9pZCI6IjBkN2Y4MGE2LTQwNzMtNGFkYS04MGQ0LWU2MWNmMDA2MjdmNyIsInByZWZlcnJlZF91c2VybmFtZSI6ImRhbWllbi5ib3dkZW5AbWVyaWxsLm9yZyIsInJoIjoiMC5BUU1BRDdGU0dCR2dpMEtZLWRDY045UjN6OTg5cUFxZ0NmVkVxZlMzQkdxR3Zva0JBWkkuIiwic3ViIjoiZGs2M0dQM3VmbF9iM3JzOGpjZDNNN0pxS1ZnYlNLYnZKY1NVd3hWZnYwWSIsInRpZCI6IjE4NTJiMTBmLWEwMTEtNDI4Yi05OGY5LWQwOWMzN2Q0NzdjZiIsInV0aSI6IlhCeVdhNWF5MTBTalQ1MmVOREpMQUEiLCJ2ZXIiOiIyLjAifQ.K8MM3lDKNz5WsJf1SlzQaSxAnQJvYl8WRKcPSMD1vH1GJYE6_XPNUL6iT4n7Y3N9JzrpUhAD_buVLWeoYFxEV6c7HQlCignva4zpXd-nJlupJ1S7WDGaCEbjJjsp9WQyJVcmdHs9SlxN3i6z2wyO6Z8H6ZM_0FvZaCbiRFqziURgkPiibPy_iIkurusBeZK48wViOSDojzHm1P9K6ad0J8Zt9XaEyTGGdMeZEcDWGO4FdFzaoccGOJDvaBK452QRUrost2iKpq246b81GIi8agi8BiOO8eWzbFfyzbpzGixEI9hASc50Rjd3SDoOETqoW1DHwQyQ_S6b6oXIM2qXGQ";
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
