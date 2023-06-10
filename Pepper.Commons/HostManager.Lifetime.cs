namespace Pepper.Commons
{
    public partial class HostManager
    {
        public async Task Run()
        {
            var _ = Start();
            await Task.Delay(-1);
        }

        private async Task Start()
        {
            if (host == null)
            {
                await PrepareHost();
            }

            await host!.StartAsync();
        }

        public async Task Reload()
        {
            var t = host?.StopAsync();
            if (t is not null)
            {
                await t.ConfigureAwait(false);
            }

            host = null;
            await Start();
        }
    }
}