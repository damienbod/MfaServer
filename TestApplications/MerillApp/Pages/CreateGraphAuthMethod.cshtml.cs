using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerillApp.Pages;

public class CreateGraphAuthMethodModel : PageModel
{
    private readonly MicrosoftGraphClient _microsoftGraphClient;

    [BindProperty]
    public CreateAuthenticationMethodDto CreateAuthenticationMethodModel { get; set; } = new CreateAuthenticationMethodDto();

    public CreateGraphAuthMethodModel(MicrosoftGraphClient microsoftGraphClient)
    {
        _microsoftGraphClient = microsoftGraphClient;
    }

    public async Task OnGetAsync()
    {
    }

    public async Task OnPostAsync()
    {
        await _microsoftGraphClient.CreateAuthenticationMethodV2(CreateAuthenticationMethodModel);
    }

}
