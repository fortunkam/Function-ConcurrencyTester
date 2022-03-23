using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

[assembly: FunctionsStartup(typeof(ConcurrencyFunction.Startup))]
namespace ConcurrencyFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            //IConfigurationRoot config = new ConfigurationBuilder()
            //    .SetBasePath(Directory.GetCurrentDirectory())
            //    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            //    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //    .AddEnvironmentVariables()
            //    .Build();


            //builder.Services.AddSingleton<IConfiguration>(config);
        }

        public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
        {
            base.ConfigureAppConfiguration(builder);
        }
    }
}
