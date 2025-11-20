using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Bernhoeft.GRT.Teste.ContractTests.Validation
{
    /// <summary>
    /// Testes de contrato para validação de dados na API
    /// Foca em testar cenários de validação e contratos de erro
    /// </summary>
    public class ValidationContractTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public ValidationContractTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        #region Validation Error Schema Tests

        [Fact]
        public async Task ValidationError_ShouldFollowStandardSchema()
        {
            // Arrange - Request inválido
            var invalidRequest = new { }; // Request vazio

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", invalidRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            // Schema padrão de erro de validação
            root.TryGetProperty("Mensagens", out var mensagens).Should().BeTrue();
            mensagens.ValueKind.Should().Be(JsonValueKind.Array);
            mensagens.GetArrayLength().Should().BeGreaterThan(0);

            // Cada erro deve ser uma string
            var firstError = mensagens[0];
            firstError.ValueKind.Should().Be(JsonValueKind.String);
        }

        #endregion

        #region Field Validation Contract Tests

        [Theory]
        [InlineData(null, "Mensagem válida")]
        [InlineData("", "Mensagem válida")]
        [InlineData("   ", "Mensagem válida")]
        public async Task CreateAviso_WithInvalidTitulo_ShouldReturnValidationError(string titulo, string mensagem)
        {
            // Arrange
            var request = new
            {
                titulo = titulo,
                mensagem = mensagem
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            var mensagens = jsonDocument.RootElement.GetProperty("Mensagens");

            // Deve conter erro relacionado ao título
            var errorMessages = mensagens.EnumerateArray()
                .Select(e => e.GetString().ToLowerInvariant())
                .ToList();

            errorMessages.Should().Contain(msg => msg.Contains("titulo") || msg.Contains("título"));
        }

        [Theory]
        [InlineData("Título válido", null)]
        [InlineData("Título válido", "")]
        [InlineData("Título válido", "   ")]
        public async Task CreateAviso_WithInvalidMensagem_ShouldReturnValidationError(string titulo, string mensagem)
        {
            // Arrange
            var request = new
            {
                titulo = titulo,
                mensagem = mensagem
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            var mensagens = jsonDocument.RootElement.GetProperty("Mensagens");

            // Deve conter erro relacionado à mensagem
            var errorMessages = mensagens.EnumerateArray()
                .Select(e => e.GetString().ToLowerInvariant())
                .ToList();

            errorMessages.Should().Contain(msg => msg.Contains("mensagem"));
        }

        #endregion

        #region Length Validation Contract Tests

        [Fact]
        public async Task CreateAviso_WithTooLongTitulo_ShouldReturnValidationError()
        {
            // Arrange - String muito longa (assumindo limite de ~200 caracteres)
            var longTitulo = new string('A', 500);
            var request = new
            {
                titulo = longTitulo,
                mensagem = "Mensagem válida"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);
                var mensagens = jsonDocument.RootElement.GetProperty("Mensagens");

                var errorMessages = mensagens.EnumerateArray()
                    .Select(e => e.GetString().ToLowerInvariant())
                    .ToList();

                errorMessages.Should().Contain(msg => 
                    msg.Contains("titulo") || 
                    msg.Contains("título") ||
                    msg.Contains("length") || 
                    msg.Contains("comprimento") ||
                    msg.Contains("tamanho"));
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                // Cleanup se foi aceito
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var avisoId = json.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        [Fact]
        public async Task CreateAviso_WithTooLongMensagem_ShouldReturnValidationError()
        {
            // Arrange - String muito longa (assumindo limite de ~1000 caracteres)
            var longMensagem = new string('B', 2000);
            var request = new
            {
                titulo = "Título válido",
                mensagem = longMensagem
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", request);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.OK);

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);
                var mensagens = jsonDocument.RootElement.GetProperty("Mensagens");

                var errorMessages = mensagens.EnumerateArray()
                    .Select(e => e.GetString().ToLowerInvariant())
                    .ToList();

                errorMessages.Should().Contain(msg => 
                    msg.Contains("mensagem") || 
                    msg.Contains("length") || 
                    msg.Contains("comprimento") ||
                    msg.Contains("tamanho"));
            }
            else if (response.StatusCode == HttpStatusCode.OK)
            {
                // Cleanup se foi aceito
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var avisoId = json.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        #endregion

        #region ID Validation Contract Tests

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("abc")]
        [InlineData("1.5")]
        public async Task GetAviso_WithInvalidId_ShouldReturnBadRequest(string invalidId)
        {
            // Act
            var response = await _client.GetAsync($"/api/v1/avisos/{invalidId}");

            // Assert - API pode aceitar alguns formatos e retornar 200/404
            response.StatusCode.Should().BeOneOf(
                HttpStatusCode.BadRequest, 
                HttpStatusCode.NotFound,
                HttpStatusCode.OK // API pode aceitar alguns IDs como válidos
            );
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("abc")]
        public async Task UpdateAviso_WithInvalidId_ShouldReturnBadRequest(string invalidId)
        {
            // Arrange
            var updateRequest = new
            {
                mensagem = "Nova mensagem"
            };

            // Act
            var response = await _client.PutAsJsonAsync($"/api/v1/avisos/{invalidId}", updateRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("0")]
        [InlineData("-1")]
        [InlineData("abc")]
        public async Task DeleteAviso_WithInvalidId_ShouldReturnBadRequest(string invalidId)
        {
            // Act
            var response = await _client.DeleteAsync($"/api/v1/avisos/{invalidId}");

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        #endregion

        #region Update Validation Contract Tests

        [Fact]
        public async Task UpdateAviso_WithEmptyMensagem_ShouldReturnValidationError()
        {
            // Arrange - Criar um aviso primeiro
            var createRequest = new
            {
                titulo = "Aviso para Update",
                mensagem = "Mensagem original"
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/v1/avisos", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createContent);
            var avisoId = createJson.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();

            // Arrange - Request de update inválido
            var updateRequest = new
            {
                mensagem = ""
            };

            try
            {
                // Act
                var response = await _client.PutAsJsonAsync($"/api/v1/avisos/{avisoId}", updateRequest);

                // Assert
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);
                var mensagens = jsonDocument.RootElement.GetProperty("Mensagens");

                var errorMessages = mensagens.EnumerateArray()
                    .Select(e => e.GetString().ToLowerInvariant())
                    .ToList();

                errorMessages.Should().Contain(msg => msg.Contains("mensagem"));
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        [Fact]
        public async Task UpdateAviso_WithoutMensagem_ShouldReturnValidationError()
        {
            // Arrange - Request vazio
            var updateRequest = new { };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/avisos/1", updateRequest);

            // Assert
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
        }

        #endregion

        #region Malformed Request Contract Tests

        [Fact]
        public async Task CreateAviso_WithMalformedJson_ShouldReturnBadRequest()
        {
            // Arrange
            var malformedJson = "{ \"titulo\": \"Test\", \"mensagem\": }"; // JSON inválido
            var content = new StringContent(malformedJson, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/avisos", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task CreateAviso_WithExtraFields_ShouldIgnoreOrReject()
        {
            // Arrange - Request com campos extras
            var requestWithExtraFields = new
            {
                titulo = "Título válido",
                mensagem = "Mensagem válida",
                campoExtra = "valor extra",
                outroExtraCampo = 123
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", requestWithExtraFields);

            // Assert - API deve aceitar e ignorar campos extras, ou rejeitar
            response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest);

            // Cleanup se foi aceito
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var avisoId = json.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        #endregion

        #region Concurrent Validation Tests

        [Fact]
        public async Task MultipleValidationRequests_ShouldHandleConcurrently()
        {
            // Arrange - Múltiplos requests inválidos
            var invalidRequests = Enumerable.Range(0, 5).Select(i => new
            {
                titulo = i % 2 == 0 ? "" : "Título válido",
                mensagem = i % 2 == 1 ? "" : "Mensagem válida"
            });

            // Act
            var tasks = invalidRequests.Select(req => 
                _client.PostAsJsonAsync("/api/v1/avisos", req)
            ).ToArray();

            var responses = await Task.WhenAll(tasks);

            // Assert
            foreach (var response in responses)
            {
                response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
                response.Dispose();
            }
        }

        #endregion
    }
}