using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pepper.Commons.Maimai;
using Pepper.Frontends.Maimai.Services;
using Xunit;

namespace Pepper.Frontends.Maimai.Test.DataResolutionTest
{
    public class DataFixture : IAsyncLifetime
    {
        public readonly MaimaiDataDbContext MaimaiDataDbContext;
        public readonly MaimaiDataService MaimaiDataService;
        private readonly ServiceProvider serviceProvider;
        private static readonly HttpClient httpClient = new();
        public DataFixture()
        {
            var serviceCollection = new ServiceCollection();
            var s = Environment.GetEnvironmentVariable("MARIADB_CONNECTION_STRING_MAIMAI");
            MaimaiDataDbContext = new MaimaiDataDbContext(
                new DbContextOptionsBuilder<MaimaiDataDbContext>()
                    .UseMySql(s, ServerVersion.AutoDetect(s))
                    .Options
            );

            serviceProvider = serviceCollection.BuildServiceProvider();
            MaimaiDataService = new MaimaiDataService(serviceProvider);
        }


        public async Task InitializeAsync()
        {
            await MaimaiDataService.Load(MaimaiDataDbContext, httpClient, CancellationToken.None);
        }

        public async Task DisposeAsync()
        {
            await serviceProvider.DisposeAsync();
            MaimaiDataService.Dispose();
            await MaimaiDataDbContext.DisposeAsync();
        }
    }
}