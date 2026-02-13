using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Duckmension_Website.Controllers;

[Authorize(Roles = "Owner")]
public class OwnerController : Controller
{
    public IActionResult Index() => View();
}
