namespace LetsEncrypt.ApplicationGateway
{
    using LetsEncrypt.ApplicationGateway.ConsoleApp.Azure;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;

    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args)
                .AddEnvironmentVariables()
                .AddEnvironmentSecrets()
                .Build();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log/lets-encrypt-application-gateway.log")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(configuration, serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();

            logger.LogInformation("Hello World!");

            //var certbotClient = serviceProvider.GetRequiredService<CertbotClient>();
            //var certificates = certbotClient.GetCertificates();

            //logger.LogInformation($"Found {certificates.Count()} certificates.");

            //foreach (var cert in certificates)
            //{
            //    var json = JsonConvert.SerializeObject(cert);
            //    logger.LogInformation(json);
            //}

            var azureHttpClient = serviceProvider.GetService<AzureHttpClient>();
            var token = azureHttpClient.GetBearerTokenAsync().Result;

            logger.LogInformation($"Access token: {token}");
        }

        private static void ConfigureServices(IConfiguration configuration, IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog())
                .AddTransient<Program>()
                .AddTransient<CertbotClient>();

            var azureConfig = new AzureConfiguration
            {
                ClientId = configuration.GetValue<string>("Azure-Client-Id"),
                ClientSecret = configuration.GetValue<string>("Azure-Client-Secret")
            };

            services.AddSingleton(azureConfig);

            services.AddHttpClient<AzureHttpClient>();
        }
    }
}
