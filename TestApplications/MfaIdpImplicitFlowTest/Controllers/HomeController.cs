using MfaIdpImplicitFlowTest.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace MfaIdpImplicitFlowTest.Controllers;

[Authorize]
public class HomeController : Controller
{
    public async Task<IActionResult> Index()
    {
        return View();
    }

    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}