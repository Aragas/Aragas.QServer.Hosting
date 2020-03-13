using Microsoft.Extensions.Hosting;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Aragas.QServer.Hosting
{
    public class QServerHost : IHost, IEnvironmentSetup
    {
        public static QServerHost Default => new QServerHost();

        public static IHostBuilder CreateDefaultBuilder(string[]? args = null, Guid? uid = null)
        {
            var host = Host.CreateDefaultBuilder(args ?? Array.Empty<string>())
                .UseLogging()
                .UseNATSNetworkBus()
                .UseMetricsWithDefault()
                .UseHealthChecks();

            if (uid != null)
                host.UseServiceOptions(uid.Value);

            return host;
        }

        private readonly IHost _host;

        public IServiceProvider Services => _host.Services;
        public Guid Uid { get; } = Guid.NewGuid();

        public QServerHost()
        {
            IEnvironmentSetup.SetEnvironment();

            var builder = CreateDefaultBuilder(uid: Uid);

            _host = builder.Build();
        }
        public QServerHost(IHostBuilder builder)
        {
            IEnvironmentSetup.SetEnvironment();

            builder = builder
                .UseLogging()
                .UseNATSNetworkBus()
                .UseMetricsWithDefault()
                .UseHealthChecks()
                .UseServiceOptions(Uid);

            _host = builder.Build();
        }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            return _host.StartAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            return _host.StopAsync(cancellationToken);
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
}