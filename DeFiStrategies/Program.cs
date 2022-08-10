using DeFi_Strategies.Tron.CompoundRDNT;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using ServiceStack.Text;

IServiceProvider serviceProvider = RegisterServices();
Logger logger = LogManager.GetCurrentClassLogger();
logger.Info("Application started");

await serviceProvider.GetService<AutoCompounder>().AutocompoundAsync();

if (serviceProvider != null && serviceProvider is IDisposable sp)
    sp.Dispose();

logger.Info("Execution finished. Press any key to exit.");
Console.ReadKey();

static IServiceProvider RegisterServices()
{
    var services = new ServiceCollection();

    var logsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs");
    LogManager.Configuration.Variables["mydir"] = logsPath;

    var config = new ConfigurationBuilder()
        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
        .AddJsonFile("appsettings.json").Build();

    var section = config.GetSection(nameof(CompoundingConfig));
    CompoundingConfig compoundingConfig = section.Get<CompoundingConfig>();

  

    services.AddSingleton<AutoCompounder>();
    services.AddSingleton(compoundingConfig);

    return services.BuildServiceProvider();
}