[assembly: HostingStartup(typeof(FidoMfaServer.Areas.Identity.IdentityHostingStartup))]
namespace FidoMfaServer.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {
        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
            });
        }
    }
}
