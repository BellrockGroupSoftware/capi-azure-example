using Bellrock.Capi.CapiConnector.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bellrock.Capi.CapiConnector.Api
{
    public class CapiApiKeySecretAuthService : ICapiAuthenticationService
    {
        private readonly HttpClient httpClient;
        private readonly IMemoryCache memoryCache;

        private readonly string capiKey;
        private readonly string capiSecretKey;

        public CapiApiKeySecretAuthService(HttpClient httpClient, IMemoryCache memoryCache, IConfiguration configuration)
        {
            this.httpClient = httpClient;
            this.memoryCache = memoryCache;

            this.capiKey = configuration["capi-api-key"];
            this.capiSecretKey = configuration["capi-api-secret"];
        }

        public async Task<CapiAuthToken> GetTokenAsync()
        {
            if (!memoryCache.TryGetValue("CapiToken", out CapiAuthToken token))
            {
                token = await RefreshTokenAsync();
                memoryCache.Set("CapiToken", token, DateTime.UtcNow.AddMilliseconds(1));
            }

            return token;
        }

        public async Task<CapiAuthToken> RefreshTokenAsync()
        {
            var saltResponse = await httpClient.GetAsync("api/tokens/salt");

            // Throw exception if not a success status code
            saltResponse.EnsureSuccessStatusCode();

            var salt = await saltResponse.Content.ReadAsStringAsync();
            var saltAndSecretBytes = Encoding.ASCII.GetBytes(this.capiSecretKey + salt.Replace("\"", ""));
            var sha256Hash = SHA256.Create().ComputeHash(saltAndSecretBytes);

            var body = new { apiKeyId = this.capiKey, authToken = Convert.ToBase64String(sha256Hash) };
            var accTokenResponse = await httpClient.PostAsync("api/tokens/obtain", JsonContent.Create(body));

            // Throw exception if not a success status code
            accTokenResponse.EnsureSuccessStatusCode();

            var token = await accTokenResponse.Content.ReadAsStringAsync();

            return new CapiAuthToken()
            {
                Token = token.Replace("\"", "")
            };
        }
    }
}
