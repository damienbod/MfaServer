using Microsoft.AspNetCore.Mvc;

namespace MfaIdpImplicitFlowTest.Controllers;

[Route("[controller]")]
public class StatusController : Controller
{
    [Route("test")]
    public IActionResult Test()
    {
        return View();
    }
}
