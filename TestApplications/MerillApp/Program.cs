using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace MerillApp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddOptions();

        builder.Services.AddScoped<MicrosoftGraphClient>();

        builder.Services.AddDistributedMemoryCache();

        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(options =>
            {
                builder.Configuration.Bind("AzureAd", options);
                options.UsePkce = true;
                options.Events = new OpenIdConnectEvents
                {
                    OnTokenResponseReceived = async context =>
                    {
                        var idToken = context.TokenEndpointResponse.IdToken;
                    }
                };
            })
            .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(graphBaseUrl: "https://graph.microsoft.com/beta", new List<string> {
                "Policy.ReadWrite.AuthenticationMethod" })
            .AddDistributedTokenCaches();

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });
        builder.Services.AddRazorPages()
            .AddMicrosoftIdentityUI();

        var app = builder.Build();

        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();
        app.MapControllers();

        app.Run();
    }
}
