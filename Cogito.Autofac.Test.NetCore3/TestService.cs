using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Cogito.Autofac.Test.NetCore3
{

    [RegisterAs(typeof(IHostedService))]
    public class TestService : IHostedService, IAsyncDisposable
    {

        readonly ILogger logger;
        readonly Timer timer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="logger"></param>
        public TestService(ILogger<TestService> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            timer = new Timer(TimerCallback);
        }

        void TimerCallback(object state)
        {
            logger.LogInformation("Timer triggered.");
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            timer.Change(TimeSpan.Zero, TimeSpan.FromSeconds(5));
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            timer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return timer.DisposeAsync();
        }

    }

}
