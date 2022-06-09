using Bellrock.Capi.CapiConnector.Models;
using Microsoft.Extensions.Logging;
using System.Net.Mime;
using System.Text.Json;

namespace Bellrock.Capi.CapiConnector.Api
{
    public class CapiService : ICapiService
    {
        private readonly HttpClient httpClient;
        private readonly ILogger<CapiService> logger;

        public CapiService(HttpClient httpClient, ILogger<CapiService> logger)
        {
            this.httpClient = httpClient;
            this.logger = logger;
        }
        public async Task<CapiOrder> GetOrderAsync(Guid orderId)
        {
            using var response = await httpClient.GetAsync($"api/orders/{orderId}");

            response.EnsureSuccessStatusCode();

            if (response.Content.Headers.ContentType?.MediaType != MediaTypeNames.Application.Json)
            {
                throw new HttpRequestException("Get Order received an unexpected response from API.");
            }

            var json = await response.Content.ReadAsStringAsync();
            var order = JsonSerializer.Deserialize<CapiOrder>(json, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (order == null)
            {
                throw new JsonException($"Failed to deserialize response to Order. Json received: {json}");
            }

            return order;
        }
    }
}
