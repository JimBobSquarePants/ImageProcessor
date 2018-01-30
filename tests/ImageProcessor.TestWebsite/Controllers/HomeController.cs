using System.Web.Mvc;

namespace ImageProcessor.TestWebsite.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/

        public ActionResult Index()
        {
            return this.View();
        }

        public ActionResult Png()
        {
            return this.View();
        }

        public ActionResult Png8()
        {
            return this.View();
        }

        public ActionResult Gif()
        {
            return this.View();
        }

        public ActionResult Bmp()
        {
            return View();
        }

        public ActionResult Tiff()
        {
            return View();
        }

        public ActionResult WebP()
        {
            return View();
        }

        public ActionResult Database()
        {
            return this.View();
        }

        public ActionResult External()
        {
            return this.View();
        }
    }
}
