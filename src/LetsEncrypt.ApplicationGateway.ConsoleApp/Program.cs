namespace LetsEncrypt.ApplicationGateway
{
    using Newtonsoft.Json;
    using System;
    using System.Diagnostics;
    using System.Linq;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("log/lets-encrypt-application-gateway.log")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var logger = serviceProvider.GetService<ILogger<Program>>();

            logger.LogInformation("Hello World!");

            var certbotClient = serviceProvider.GetRequiredService<CertbotClient>();
            var certificates = certbotClient.GetCertificates();

            logger.LogInformation($"Found {certificates.Count()} certificates.");

            foreach (var cert in certificates)
            {
                var json = JsonConvert.SerializeObject(cert);
                logger.LogInformation(json);
            }
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(configure => configure.AddSerilog())
                .AddTransient<Program>()
                .AddTransient<CertbotClient>();
        }
    }
}
