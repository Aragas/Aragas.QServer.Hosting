using Aragas.QServer.NetworkBus.Data;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Serilog;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Extensions.Hosting
{
    public static class HostExtensions
    {
        public static async Task RunQServerAsync(this IHost host, CancellationToken cancellationToken = default)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("loggerconfig.json", false)
                .AddJsonFile($"loggerconfig.{env}.json", true)
                .Build();
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("Starting.");

                // Recreate logger with Service Uid now that the ServiceModule is initialized.
                var serviceOptions = host.Services.GetService<IOptions<ServiceOptions>>();
                if (serviceOptions?.Value != null)
                {
                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .Enrich.WithApplicationInfo(serviceOptions.Value.Uid)
                        .CreateLogger();
                }

                await host.RunAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Fatal exception.");
                throw;
            }
            finally
            {
                Log.Information("Stopped.");
                Log.CloseAndFlush();
            }
        }
    }
}