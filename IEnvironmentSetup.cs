using System;

namespace Aragas.QServer.Hosting
{
    internal interface IEnvironmentSetup
    {
        protected static void SetEnvironment()
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
        }
    }
}