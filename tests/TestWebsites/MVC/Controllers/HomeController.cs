using Microsoft.AspNet.Mvc;

namespace MVC.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Bmp()
        {
            return View();
        }

        public IActionResult Gif()
        {
            return View();
        }

        public IActionResult Png()
        {
            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}