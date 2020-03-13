using Aragas.QServer.NetworkBus;
using Aragas.QServer.NetworkBus.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

using Serilog;

using System;
using System.Threading.Tasks;

namespace Aragas.QServer.Hosting
{
    public static class QServerHostProgram
    {
        public static Guid Uid { get; } = Guid.NewGuid();

        public static async Task Main<TProgram>(
            Func<LoggerConfiguration, LoggerConfiguration>? loggerConfigurationFunc = null,
            Func<IHostBuilder, IHostBuilder>? hostBuilderFunc = null,
            Action<IHost>? beforeRunAction = null,
            string[]? args = null)
        {
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == null && Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == null)
            {
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", "Development");
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            }

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != null && Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == null)
            {
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            }

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == null && Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") != null)
            {
                Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT"));
            }

            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != null && Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") != null)
            {
                Environment.SetEnvironmentVariable("DOTNET_ENVIRONMENT", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
            }


            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("loggerconfig.json", false)
                .AddJsonFile($"loggerconfig.{env}.json", true)
                .Build();
            var loggerConfiguration = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.WithApplicationInfo(Uid);
            loggerConfigurationFunc?.Invoke(loggerConfiguration);
            Log.Logger = loggerConfiguration.CreateLogger();

            try
            {
                Log.Information("Starting.", typeof(TProgram).FullName);

                var hostBuilder = QServerHost.CreateDefaultBuilder(args ?? Array.Empty<string>(), Uid);
                hostBuilderFunc?.Invoke(hostBuilder);

                var host = hostBuilder.Build();

                BeforeRun(host);
                beforeRunAction?.Invoke(host);

                await host.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal exception.", typeof(TProgram).FullName);
                throw;
            }
            finally
            {
                Log.Information("Stopped.", typeof(TProgram).FullName);
                Log.CloseAndFlush();
            }
        }

        private static void BeforeRun(IHost host)
        {
            var serviceOptions = host.Services.GetRequiredService<IOptions<ServiceOptions>>().Value;
            var subscriptionStorage = host.Services.GetRequiredService<SubscriptionStorage>();

            subscriptionStorage.HandleServiceDiscoveryHandler();
            subscriptionStorage.HandleMetricsPrometheusHandler(serviceOptions.Uid);
            subscriptionStorage.HandleHealthHandler(serviceOptions.Uid);

            var lifeTime = host.Services.GetRequiredService<IHostApplicationLifetime>();
            lifeTime.ApplicationStopping.Register(() => subscriptionStorage.Dispose());
        }
    }
}