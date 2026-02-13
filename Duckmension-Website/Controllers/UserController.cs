using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Duckmension_Website.Controllers;
[Authorize(Roles = "User,Admin,Owner")]
public class UserController : Controller
{
    public IActionResult Index() => View();
}
