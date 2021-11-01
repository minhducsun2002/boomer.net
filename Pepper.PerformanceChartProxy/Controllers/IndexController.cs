using System.Threading.Tasks;
using EasyCaching.Core;
using Microsoft.AspNetCore.Mvc;

namespace Pepper.PerformanceChartProxy.Controllers
{
    [ApiController]
    [Route("/")]
    public class IndexController : Controller
    {
        private readonly IEasyCachingProvider cachingProvider;
        public IndexController(IEasyCachingProvider cachingProvider)
        {
            this.cachingProvider = cachingProvider;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return Content($"The server is running with {await cachingProvider.GetCountAsync()} beatmap(s) cached.");
        }
    }
}