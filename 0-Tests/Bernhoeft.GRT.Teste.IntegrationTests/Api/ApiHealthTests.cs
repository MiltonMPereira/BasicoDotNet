using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using Xunit;

namespace Bernhoeft.GRT.Teste.IntegrationTests.Api
{
    public class ApiHealthTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ApiHealthTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Api_ShouldStart_Successfully()
        {
            // Act
            var response = await _client.GetAsync("/");

            // Assert
            response.Should().NotBeNull();
        }

        [Fact]
        public async Task Swagger_ShouldBeAccessible()
        {
            // Act
            var response = await _client.GetAsync("/swagger/index.html");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Api_Avisos_Endpoint_ShouldExist()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/avisos");

            // Assert
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }
    }
}