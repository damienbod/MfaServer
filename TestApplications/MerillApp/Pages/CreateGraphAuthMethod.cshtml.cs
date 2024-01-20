using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerillApp.Pages;

public class CreateGraphAuthMethodModel : PageModel
{
    private readonly MicrosoftGraphClient _microsoftGraphClient;

    public CreateGraphAuthMethodModel(MicrosoftGraphClient microsoftGraphClient)
    {
        _microsoftGraphClient = microsoftGraphClient;
    }

    public async Task OnGetAsync()
    {
        await _microsoftGraphClient.CreateAuthenticationMethodV2();
    }
}
