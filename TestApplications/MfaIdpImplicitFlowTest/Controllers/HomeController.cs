using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MfaIdpImplicitFlowTest.Models;

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