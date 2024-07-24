using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Bintainer.WebApp.Services
{
    public class DigikeyService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string? _clientId;
        private readonly string? _clientSecret;
        private readonly string? _tokenEndpoint = "https://sandbox-api.digikey.com/v1/oauth2/token";
        private readonly string? _apiEndpointBase = "https://sandbox-api.digikey.com/products/v4/";

        public DigikeyService(HttpClient httpClient, IMemoryCache cache, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _cache = cache;
            _clientId = configuration["Digikey:ClientId"];
            _clientSecret = configuration["Digikey:ClientSecret"];
        }

        public async Task<string?> GetTokenAsync()
        {
            if (_cache.TryGetValue("DigikeyToken", out string? token))
            {
                return token;
            }

            var requestBody = new Dictionary<string, string?>
            {
                { "client_id", _clientId },
                { "grant_type", "client_credentials" },
                { "client_secret", _clientSecret }
            };

            var requestContent = new FormUrlEncodedContent(requestBody);

            var request = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint);
            request.Content = requestContent;
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent);

            if (tokenResponse == null)
            {
                throw new InvalidOperationException("Failed to retrieve token.");
            }

            _cache.Set("DigikeyToken", tokenResponse.access_token, TimeSpan.FromSeconds(tokenResponse.expires_in - 60));

            return tokenResponse.access_token;
        }

        public async Task<string> GetProductDetailsAsync(string keyword)
        {
            var token = await GetTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Get, $"{_apiEndpointBase}search/{keyword}/productdetails");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("X-DIGIKEY-Client-Id", _clientId);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
                
        public List<Parameter> ExtractParameters(string productDetails) 
        {
            JObject jsonObject = JObject.Parse(productDetails);
            List<Parameter> parameters = new List<Parameter>();
            JArray parameterArray = (JArray)jsonObject["Product"]["Parameters"];
            foreach (var item in parameterArray)
            {
                parameters.Add(new Parameter
                {
                    ParameterText = item["ParameterText"].ToString(),
                    ValueText = item["ValueText"].ToString()
                });
            }
            return parameters;
        }

        private class TokenResponse
        {
            public string? access_token { get; set; }
            public int expires_in { get; set; }
            public string? token_type { get; set; }
        }

        public class Parameter
        {
            public string ParameterText { get; set; }
            public string ValueText { get; set; }
        }


    }
}
