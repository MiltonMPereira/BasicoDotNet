using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Bernhoeft.GRT.Teste.ContractTests
{
    /// <summary>
    /// Testes de contrato para a API geral - endpoints de infraestrutura e saúde
    /// </summary>
    public class ApiGeneralContractTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ApiGeneralContractTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }



        #region Error Handling Contract Tests

        [Fact]
        public async Task InvalidHttpMethod_ShouldReturn405()
        {
            // Act - PATCH não é suportado na API
            var response = await _client.PatchAsync("/api/v1/avisos/1", null);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
        }

        #endregion
         

        #region Request Size Contract Tests

        [Fact]
        public async Task LargeRequest_ShouldHandleGracefully()
        {
            // Arrange - Criar um request com dados grandes (mas não excessivos)
            var largeRequest = new
            {
                titulo = new string('A', 1000), // 1KB
                mensagem = new string('B', 5000) // 5KB
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", largeRequest);

            // Assert - Deve ser processado ou rejeitado com erro apropriado
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.OK,                    // Aceito (API retorna 200)
                HttpStatusCode.BadRequest,            // Rejeitado por validação
                HttpStatusCode.RequestEntityTooLarge  // Rejeitado por tamanho
            );

            // Cleanup se criado com sucesso
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var avisoId = json.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        #endregion

        #region Character Encoding Contract Tests

        [Fact]
        public async Task Request_WithUnicodeCharacters_ShouldHandleCorrectly()
        {
            // Arrange
            var unicodeRequest = new
            {
                titulo = "Título com acentuação: ção, ã, é, í, ó, ú",
                mensagem = "Mensagem com emojis: 🚀 🎉 ✅ e caracteres especiais: @#$%^&*()"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", unicodeRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);
                var dados = jsonDocument.RootElement.GetProperty("Dados");

                dados.GetProperty("Titulo").GetString().Should().Contain("acentuação");
                dados.GetProperty("Mensagem").GetString().Should().Contain("🚀");

                // Cleanup
                var avisoId = dados.GetProperty("Id").GetInt32();
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        #endregion

    }
}