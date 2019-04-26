using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SharpRaven;
using SharpRaven.Data;

namespace SeCoucherMoinsBeteRssFeed.Services
{
    public class BackgroundLoaderService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public BackgroundLoaderService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    try
                    {
                        var feedLoader = scope.ServiceProvider.GetRequiredService<FeedLoader>();
                        await feedLoader.Load();
                    }
                    catch (Exception e)
                    {
                        scope.ServiceProvider.GetRequiredService<RavenClient>().Capture(new SentryEvent(e));
                    }

                    await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
                }
            }
        }
    }
}
