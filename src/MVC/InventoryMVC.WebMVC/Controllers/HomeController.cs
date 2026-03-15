using Microsoft.AspNetCore.Mvc;

namespace InventoryMVC.WebMVC.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();
}
