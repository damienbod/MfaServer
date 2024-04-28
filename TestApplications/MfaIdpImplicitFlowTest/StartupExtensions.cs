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
        context.ProtocolMessage.IdTokenHint = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6InEtMjNmYWxldlpoaEQzaG05Q1Fia1A1TVF5VSJ9.eyJhdWQiOiIwYWE4M2RkZi0wOWEwLTQ0ZjUtYTlmNC1iNzA0NmE4NmJlODkiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vMTg1MmIxMGYtYTAxMS00MjhiLTk4ZjktZDA5YzM3ZDQ3N2NmL3YyLjAiLCJpYXQiOjE3MTQyOTQ2MjksIm5iZiI6MTcxNDI5NDYyOSwiZXhwIjoxNzE0Mjk4NTI5LCJuYW1lIjoiRGFtaWVuIEJvd2RlbiIsIm5vbmNlIjoiNjM4NDk4OTE3MjgwNjczMjc5LlltVTFPREJtWkdJdE5tRmlNQzAwTm1JNExUaGhaV010WldFNFpUQTNORFF6WVRnd01ESmpZV0ptWWpFdE1ERTNPQzAwTUdFeUxUbG1NV1F0TlRjd1pUQmlaakJsTWpFMCIsIm9pZCI6IjBkN2Y4MGE2LTQwNzMtNGFkYS04MGQ0LWU2MWNmMDA2MjdmNyIsInByZWZlcnJlZF91c2VybmFtZSI6ImRhbWllbi5ib3dkZW5AbWVyaWxsLm9yZyIsInJoIjoiMC5BUU1BRDdGU0dCR2dpMEtZLWRDY045UjN6OTg5cUFxZ0NmVkVxZlMzQkdxR3Zva0JBWkkuIiwic3ViIjoiZGs2M0dQM3VmbF9iM3JzOGpjZDNNN0pxS1ZnYlNLYnZKY1NVd3hWZnYwWSIsInRpZCI6IjE4NTJiMTBmLWEwMTEtNDI4Yi05OGY5LWQwOWMzN2Q0NzdjZiIsInV0aSI6IkJ2alplVEtNV1VhODdsUTlQTE42QUEiLCJ2ZXIiOiIyLjAifQ.kEx9j9i_ufSXEuWoLsAOBtjbUqaX4kyD-pj33p4rnLeqI1GIGY6i_a0mvy_VmqzzzslJZHiSyEE_nyXvNvGiiEliEWqJgIuwU6-6Zu1HqmorAKtiYdipI2wH66JMmbgi2yPGPi9K2zzM--197dKj1LWPA_SP8J60qGcnabmeB9nFkNfEf8_4qnLjHWO_qHdu_83PzH4D968AfZDQejnLlP50m1cJTcG6ORXPaLVJ2xxWUWfmN0AoERDo-gCJs9BngnUfHDnRg8CIGKcMO2tEVUYa0bOupj0cte4AsCCb05Ys73zfZuhdwXRWgR9iisVywAAf7JH5kyGLzoRNzbnYhg";
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
