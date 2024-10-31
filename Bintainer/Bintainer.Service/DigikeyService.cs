using Bintainer.Model.DTO;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Text.Json;

namespace Bintainer.Service
{
    public class DigikeyService
    {
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private readonly string? _clientId;
        private readonly string? _clientSecret;
        private readonly string? _grantType;
        private readonly string? _tokenEndpoint = "https://api.digikey.com/v1/oauth2/token";
        private readonly string? _apiEndpointBase = "https://api.digikey.com/products/v4/";

        public DigikeyService(HttpClient httpClient, IMemoryCache cache, IOptions<DigikeySettings> settings)
        {
            _httpClient = httpClient;
            _cache = cache;
            _clientId = settings.Value.ClientId;
            _grantType = settings.Value.GrantType;
            _clientSecret = settings.Value.ClientSecret;
        }

        public async Task<string?> GetTokenAsync()
        {
            if (_cache.TryGetValue("DigikeyToken", out string? token))
            {
                return token;
            }

            if (string.IsNullOrEmpty(_clientId) || string.IsNullOrEmpty(_clientSecret) || string.IsNullOrEmpty(_grantType))
            {
                throw new InvalidOperationException("Client ID, Client Secret, or Grant Type is not configured.");
            }

            var requestBody = new Dictionary<string, string?>
            {
                { "client_id", _clientId },
                { "grant_type", _grantType },
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

            if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.access_token))
            {
                throw new InvalidOperationException("Failed to retrieve a valid token.");
            }

            // Cache the token, reducing expiration time by a minute for safety
            _cache.Set("DigikeyToken", tokenResponse.access_token, TimeSpan.FromSeconds(tokenResponse.expires_in - 60));

            return tokenResponse.access_token;
        }

        public async Task<string> GetProductDetailsAsync(string keyword)
        {
            var token = await GetTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                throw new InvalidOperationException("Invalid or missing token.");
            }

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

            string? detailedDescription = (string?)jsonObject["Product"]?["Description"]?["DetailedDescription"];
            if (!string.IsNullOrEmpty(detailedDescription))
            {
                parameters.Add(new Parameter { Category = "DetailedDescription", Name = "DetailedDescription", Value = detailedDescription });
            }

            string? photoUrl = (string?)jsonObject["Product"]?["PhotoUrl"];
            if (!string.IsNullOrEmpty(photoUrl))
            {
                parameters.Add(new Parameter { Category = "PhotoUrl", Name = "PhotoUrl", Value = photoUrl });
            }

            JArray? parameterArray = (JArray?)jsonObject["Product"]?["Parameters"];
            if (parameterArray != null)
            {
                foreach (var item in parameterArray)
                {
                    parameters.Add(new Parameter
                    {
                        Category = "Parameter",
                        Name = item["ParameterText"]?.ToString() ?? string.Empty,
                        Value = item["ValueText"]?.ToString() ?? string.Empty
                    });
                }
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
            public string Category { get; set; }
            public string Name { get; set; }
            public string Value { get; set; }
        }
    }
}
