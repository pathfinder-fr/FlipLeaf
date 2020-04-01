using System.Diagnostics;
using FlipLeaf.Models;
using Microsoft.AspNetCore.Mvc;

namespace FlipLeaf.Controllers
{
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
