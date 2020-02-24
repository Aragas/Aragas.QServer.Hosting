using System;

namespace Microsoft.Extensions.Hosting
{
    public class QServerHost
    {
        public static IHostBuilder CreateDefaultBuilder(string[]? args = null, Guid? uid = null)
        {
            var host = Host.CreateDefaultBuilder(args ?? Array.Empty<string>())
                .UseSerilog()
                .UseNATSNetworkBus()
                .UseMetricsWithDefault()
                .UseHealthChecks();

            if (uid != null)
                host.UseServiceOptions(uid.Value);

            return host;
        }
    }
}