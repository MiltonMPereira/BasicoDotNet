using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Bernhoeft.GRT.Teste.IntegrationTests.Controllers.v1
{
    public class AvisosControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public AvisosControllerTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Get_Avisos_ShouldReturnOk()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/avisos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Post_Aviso_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            var aviso = new
            {
                Titulo = "Título Teste",
                Mensagem = "Mensagem de teste"
            };

            var json = JsonSerializer.Serialize(aviso);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/avisos", content);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Created);
        }

        [Fact]
        public async Task Post_Aviso_WithEmptyTitle_ShouldReturnBadRequest()
        {
            // Arrange
            var avisoInvalido = new
            {
                Titulo = "",
                Mensagem = "Mensagem válida"
            };

            var json = JsonSerializer.Serialize(avisoInvalido);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/avisos", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Post_Aviso_WithEmptyMessage_ShouldReturnBadRequest()
        {
            // Arrange
            var avisoInvalido = new
            {
                Titulo = "Título válido",
                Mensagem = ""
            };

            var json = JsonSerializer.Serialize(avisoInvalido);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/avisos", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Get_Aviso_ById_ShouldReturnResponse()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/avisos/1");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
        }
    }
}