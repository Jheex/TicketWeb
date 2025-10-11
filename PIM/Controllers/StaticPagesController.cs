using Microsoft.AspNetCore.Mvc;

namespace PIM.Controllers
{
    public class StaticPagesController : Controller
    {
        public IActionResult FAQ()
        {
            return View(); // procura Views/StaticPages/FAQ.cshtml
        }
        
        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Terms()
        {
            return View();
        }

        public IActionResult Support()
        {
            return View();
        }
    }
}