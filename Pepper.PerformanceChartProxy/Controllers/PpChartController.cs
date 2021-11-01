using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using EasyCaching.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkingBeatmap = Pepper.Commons.Osu.WorkingBeatmap;

namespace Pepper.PerformanceChartProxy.Controllers
{
    [ApiController]
    [Route("/performance")]
    public class PpChartController : Controller
    {
        private readonly IEasyCachingProvider cachingProvider;
        private readonly HttpClient httpClient;
        public PpChartController(IEasyCachingProvider cachingProvider, HttpClient httpClient)
        {
            this.cachingProvider = cachingProvider;
            this.httpClient = httpClient;
        }

        public enum GameMode { Osu = 0, Taiko, Catch, Mania }
        [FromQuery(Name = "mode")] public GameMode? Mode { get; set; }

        [HttpGet]
        [Route("chart/{id:int}.png")]
        public Task<IActionResult> RenderChartPng(int id)
        {
            return RenderChart(id);
        }

        [HttpGet]
        [Route("chart/{id:int}")]
        public async Task<IActionResult> RenderChart(int id)
        {
            var cacheKey = $"beatmap_{id}";
            byte[] beatmap;
            var cacheResult = await cachingProvider.GetAsync<byte[]>(cacheKey);
            if (cacheResult.HasValue)
            {
                beatmap = cacheResult.Value;
            }
            else
            {
                try
                {
                    beatmap = await httpClient.GetByteArrayAsync($"https://osu.ppy.sh/osu/{id}");
                }
                catch (HttpRequestException httpRequestException)
                {
                    var statusCode = httpRequestException.StatusCode;
                    return StatusCode(statusCode.HasValue ? (int) statusCode.Value : StatusCodes.Status500InternalServerError);
                }
                catch
                {
                    return StatusCode(StatusCodes.Status500InternalServerError);
                }
                await cachingProvider.SetAsync(cacheKey, beatmap, TimeSpan.FromHours(12));
            }

            var workingBeatmap = WorkingBeatmap.Decode(beatmap, id);
            var url = workingBeatmap.GenerateChartUrl();

            var response = await httpClient.SendAsync(new HttpRequestMessage { RequestUri = new Uri(url) });
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return StatusCode((int) response.StatusCode);
            }
            return new FileContentResult(await response.Content.ReadAsByteArrayAsync(), "image/png");
        }
    }
}