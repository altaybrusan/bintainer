using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Bintainer.Model.DTO;
using Bintainer.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Moq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
namespace Bintainer.Test.Service
{
    public class MockHttpMessageHandler : DelegatingHandler
    {
        private readonly Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> _sendAsyncFunc;

        public MockHttpMessageHandler(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> sendAsyncFunc)
        {
            _sendAsyncFunc = sendAsyncFunc;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return _sendAsyncFunc(request, cancellationToken);
        }
    }


    [TestFixture]
    public class DigikeyServiceTests
    {
        private HttpClient _httpClient;
        private Mock<IMemoryCache> _mockCache;
        private Mock<IOptions<DigikeySettings>> _mockSettings;
        private DigikeyService _digikeyService;
        private Mock<HttpMessageHandler> _mockHttpMessageHandler;

        [SetUp]
        public void Setup()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);

            _mockCache = new Mock<IMemoryCache>();
            _mockSettings = new Mock<IOptions<DigikeySettings>>();

            _mockSettings.Setup(s => s.Value).Returns(new DigikeySettings
            {
                ClientId = "test-client-id",
                ClientSecret = "test-client-secret",
                GrantType = "client_credentials"
            });

            // Set up the cache mock to avoid NullReferenceException
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(mc => mc.CreateEntry(It.IsAny<object>())).Returns(mockCacheEntry.Object);

            _digikeyService = new DigikeyService(_httpClient, _mockCache.Object, _mockSettings.Object);
        }

        [Test]
        public async Task GetTokenAsync_ShouldReturnToken_WhenTokenIsNotCached()
        {
            // Arrange
            var tokenResponse = new { access_token = "test-token", expires_in = 3600, token_type = "Bearer" };
            var content = new StringContent(JsonSerializer.Serialize(tokenResponse), Encoding.UTF8, "application/json");

            var handler = new MockHttpMessageHandler((request, cancellationToken) =>
                Task.FromResult(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = content
                }));

            _httpClient = new HttpClient(handler);
            _digikeyService = new DigikeyService(_httpClient, _mockCache.Object, _mockSettings.Object);

            // Act
            var token = await _digikeyService.GetTokenAsync();

            // Assert
            Assert.That(token, Is.EqualTo("test-token"));
        }

        [Test]
        public async Task GetTokenAsync_ShouldReturnCachedToken_WhenTokenIsCached()
        {
            // Arrange
            string cachedToken = "cached-token";
            object tokenObject = cachedToken;

            _mockCache.Setup(c => c.TryGetValue("DigikeyToken", out tokenObject))
                      .Returns(true);

            _digikeyService = new DigikeyService(_httpClient, _mockCache.Object, _mockSettings.Object);

            // Act
            var token = await _digikeyService.GetTokenAsync();

            // Assert
            Assert.That(token, Is.EqualTo("cached-token"));
        }

        [Test]
        public async Task GetProductDetailsAsync_ShouldReturnProductDetails()
        {
            // Arrange
            string testToken = "test-token";
            string testKeyword = "test-keyword";
            string expectedResponse = "{\"Product\": {\"Parameters\": [], \"Description\": {\"DetailedDescription\": \"Test description\"}, \"PhotoUrl\": \"http://example.com/image.jpg\"}}";

            object cachedToken = testToken;  // Ensure the type matches IMemoryCache usage

            // Setup the cache to return the test token
            _mockCache.Setup(c => c.TryGetValue("DigikeyToken", out cachedToken))
                      .Returns(true);

            var responseMessage = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(expectedResponse)
            };

            var handler = new MockHttpMessageHandler((request, cancellationToken) =>
                Task.FromResult(responseMessage));

            _httpClient = new HttpClient(handler);
            _digikeyService = new DigikeyService(_httpClient, _mockCache.Object, _mockSettings.Object);

            // Act
            var result = await _digikeyService.GetProductDetailsAsync(testKeyword);

            // Assert
            Assert.AreEqual(expectedResponse, result);
        }

        [Test]
        public void ExtractParameters_ShouldReturnParameters_WhenProductDetailsProvided()
        {
            // Arrange
            string productDetails = @"{
            ""Product"": {
                ""Description"": { ""DetailedDescription"": ""Test description"" },
                ""PhotoUrl"": ""http://example.com/image.jpg"",
                ""Parameters"": [
                    { ""ParameterText"": ""Test Parameter 1"", ""ValueText"": ""Value 1"" },
                    { ""ParameterText"": ""Test Parameter 2"", ""ValueText"": ""Value 2"" }
                ]
            }
        }";

            _digikeyService = new DigikeyService(_httpClient, _mockCache.Object, _mockSettings.Object);

            // Act
            var parameters = _digikeyService.ExtractParameters(productDetails);

            // Assert
            Assert.That(parameters.Count, Is.EqualTo(4));
            Assert.That(parameters[0].Name, Is.EqualTo("DetailedDescription"));
            Assert.That(parameters[0].Value, Is.EqualTo("Test description"));
            Assert.That(parameters[1].Name, Is.EqualTo("PhotoUrl"));
            Assert.That(parameters[1].Value, Is.EqualTo("http://example.com/image.jpg"));
            Assert.That(parameters[2].Name, Is.EqualTo("Test Parameter 1"));
            Assert.That(parameters[2].Value, Is.EqualTo("Value 1"));
        }
    }

}
