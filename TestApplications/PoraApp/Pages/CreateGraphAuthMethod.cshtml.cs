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
        // Using Graph SDK, old scheme blocking
        //await _microsoftGraphClient.CreateAuthenticationMethod();

        // Using HTTP, send direct
        await _microsoftGraphClient.CreateAuthenticationMethodV2();
    }
}
