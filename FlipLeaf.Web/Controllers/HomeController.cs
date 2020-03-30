using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PathfinderFr.FlipLeaf.Models;

namespace PathfinderFr.FlipLeaf.Areas.Root.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        [Route("_site/error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
