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
        context.ProtocolMessage.IdTokenHint = "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IjVCM25SeHRRN2ppOGVORGMzRnkwNUtmOTdaRSJ9.eyJhdWQiOiJjZjBiMjMyZC1iOGIzLTRlZGMtYmU1ZS0xOWE0YTg5ZTE1YjgiLCJpc3MiOiJodHRwczovL2xvZ2luLm1pY3Jvc29mdG9ubGluZS5jb20vMDgxN2M2NTUtYTg1My00ZDhmLTk3MjMtM2EzMzNiNWI5MjM1L3YyLjAiLCJpYXQiOjE3MDU2Njc2NTUsIm5iZiI6MTcwNTY2NzY1NSwiZXhwIjoxNzA1NjcxNTU1LCJuYW1lIjoiRGFtaWVuIEJvd2RlbiIsIm5vbmNlIjoiNjM4NDEyNjQ3NTI4MjQ3Mzk0Lll6azBOVEUwTlRVdFpURmtZaTAwT1dFeUxUaGlNVFl0WXpZd01USXpaamRrWkRrNU1tTmhZVGxsWldFdE1ETXhNUzAwWldGakxUZ3hOV0l0TXpnMk1tWTRZekZtT1dRNCIsIm9pZCI6ImY0MzE3MDBjLTZkODEtNDViOC04ZjhkLTU4MWE4YWE3Y2RhNiIsInByZWZlcnJlZF91c2VybmFtZSI6ImRhbWllbi5ib3dkZW5AZWxhcG9yYS5jb20iLCJyaCI6IjAuQVVJQVZjWVhDRk9vajAyWEl6b3pPMXVTTlMwakM4LXp1TnhPdmw0WnBLaWVGYmhDQUhFLiIsInN1YiI6IjFmN3dkMVpQRzI2YVctSFE5MHJ2cmZpSndEWXotQXpXLVNNbjVJNW5DZGMiLCJ0aWQiOiIwODE3YzY1NS1hODUzLTRkOGYtOTcyMy0zYTMzM2I1YjkyMzUiLCJ1dGkiOiJzelRDU3lpR0JVU1p4TFFTc1JrS0FBIiwidmVyIjoiMi4wIn0.omPKFejCkoVkuqA9-moiz0oqCZVKzxrdgeoSxEPnl22szD7USMoJo8IYkjvtSayG6CEpKgbebP3y_LfRCM6HB1BAuK4h_f1eLU0I3ljUBDQWTkgrbuFx5BQSFLmdfiq9hGo8vc9oatqrUOkxm51lfqt_Bm68W9mkfl42_5PGisxL_aoHtz9kcx8DUsKe-vwRdq2oxzTIt2HGQQIjypwVLWseKUfu1kBjbvLMwbuGaM5ydVSLc_CNkoQxfCx0wxdHyDf1o-yqV6aeXvt4jovOIQjNs6b60oiVWKqxZJK-cRc2Lz_R4PhyFGez8nHRCH37O4fynyfF-j5IO9UAxlVHrw";
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
