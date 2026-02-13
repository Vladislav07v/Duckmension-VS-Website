using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Duckmension_Website.Controllers;
[Authorize(Roles = "Admin,Owner")]
public class AdminController : Controller
{
    public IActionResult Index() => View();
}
