using Microsoft.AspNetCore.Mvc.RazorPages;

namespace PoraApp.Pages;

public class CreateGraphAuthMethodModel : PageModel
{
    private readonly MicrosoftGraphClient _microsoftGraphClient;

    public CreateGraphAuthMethodModel(MicrosoftGraphClient microsoftGraphClient)
    {
        _microsoftGraphClient = microsoftGraphClient;
    }

    public async Task OnGetAsync()
    {
        // Using Graph SDK
        //await _microsoftGraphClient.CreateAuthenticationMethod();

        // Using HTTP
        await _microsoftGraphClient.CreateAuthenticationMethodV2();
    }
}
