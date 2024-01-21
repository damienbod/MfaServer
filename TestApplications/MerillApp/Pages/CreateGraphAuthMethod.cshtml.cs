using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MerillApp.Pages;

public class CreateGraphAuthMethodModel : PageModel
{
    private readonly MicrosoftGraphClient _microsoftGraphClient;

    [BindProperty]
    public CreateAuthenticationMethodDto CreateAuthenticationMethodModel { get; set; } = new CreateAuthenticationMethodDto();

    [BindProperty]
    public string Result { get; set; } = string.Empty;


    public CreateGraphAuthMethodModel(MicrosoftGraphClient microsoftGraphClient)
    {
        _microsoftGraphClient = microsoftGraphClient;
    }

    public void OnGet()
    {
    }

    public async Task OnPostAsync()
    {
        Result = await _microsoftGraphClient
            .CreateAuthenticationMethodV2(CreateAuthenticationMethodModel);
    }
}
