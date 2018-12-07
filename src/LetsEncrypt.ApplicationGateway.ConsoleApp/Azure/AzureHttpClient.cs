using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LetsEncrypt.ApplicationGateway.ConsoleApp.Azure
{
    class AzureHttpClient
    {
        private readonly HttpClient client;
        private readonly AzureConfiguration azureConfiguration;
        private readonly ILogger<AzureHttpClient> logger;
        private const string BaseAddress = "https://login.microsoftonline.com";

        public AzureHttpClient(HttpClient client, AzureConfiguration azureConfiguration, ILogger<AzureHttpClient> logger)
        {
            this.client = client;
            this.azureConfiguration = azureConfiguration;
            this.logger = logger;
        }

        public async Task<string> GetBearerTokenAsync()
        {
            try
            {
                //Here we are making the assumption that our HttpClient instance
                //has already had its base address set.
                this.client.BaseAddress = new Uri(BaseAddress);
                var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("client_id", this.azureConfiguration.ClientId),
                new KeyValuePair<string, string>("client_secret", this.azureConfiguration.ClientSecret),
                new KeyValuePair<string, string>("resource", "https://management.core.windows.net/")
            });

                var response = await this.client.PostAsync("/36d96d38-65a8-4e0f-b189-2decb94856dd/oauth2/token", content);
                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
            catch (HttpRequestException ex)
            {
                this.logger.LogError($"An error occured connecting to Azure auth API {ex.ToString()}");
                return null;
            }
        }
    }
}
