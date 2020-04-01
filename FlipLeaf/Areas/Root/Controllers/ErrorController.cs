using System.Diagnostics;
using FlipLeaf.Areas.Root.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlipLeaf.Areas.Root.Controllers
{
    [Area("Root")]
    public class ErrorController : Controller
    {
        [Route("_site/error")]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
