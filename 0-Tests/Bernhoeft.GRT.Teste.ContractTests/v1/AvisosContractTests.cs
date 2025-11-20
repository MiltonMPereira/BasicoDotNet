using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Bernhoeft.GRT.Teste.ContractTests.v1
{
    /// <summary>
    /// Testes de contrato para a API de Avisos v1
    /// Validam estrutura de request/response, códigos HTTP e comportamentos esperados
    /// </summary>
    public class AvisosContractTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;
        private readonly JsonSerializerOptions _jsonOptions;

        public AvisosContractTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        #region POST /api/v1/avisos - Contract Tests

        [Fact]
        public async Task CreateAviso_WithValidRequest_ShouldReturn200WithCorrectSchema()
        {
            // Arrange
            var request = new
            {
                titulo = "Teste Contract",
                mensagem = "Mensagem de teste para contrato"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", request);

            // Assert - HTTP Status
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert - Content Type
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            // Assert - Response Schema
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            // Verifica estrutura do response
            root.TryGetProperty("Dados", out var dados).Should().BeTrue();
            dados.TryGetProperty("Id", out var id).Should().BeTrue();
            id.GetInt32().Should().BeGreaterThan(0);

            dados.TryGetProperty("Titulo", out var titulo).Should().BeTrue();
            titulo.GetString().Should().Be(request.titulo);

            dados.TryGetProperty("Mensagem", out var mensagem).Should().BeTrue();
            mensagem.GetString().Should().Be(request.mensagem);

            dados.TryGetProperty("DataCriacao", out var dataCriacao).Should().BeTrue();
            dataCriacao.GetDateTime().Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromHours(5));

            // Cleanup
            var avisoId = id.GetInt32();
            await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
        }

        [Fact]
        public async Task CreateAviso_WithMissingTitulo_ShouldReturn400WithValidationErrors()
        {
            // Arrange
            var request = new
            {
                mensagem = "Mensagem sem título"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", request);

            // Assert - HTTP Status
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Assert - Content Type
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            // Assert - Validation Error Schema
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            root.TryGetProperty("Mensagens", out var mensagens).Should().BeTrue();
            mensagens.GetArrayLength().Should().BeGreaterThan(0);
            
            var errorMessages = mensagens.EnumerateArray()
                .Select(m => m.GetString().ToLowerInvariant())
                .ToList();
            
            errorMessages.Should().Contain(msg => msg.Contains("título") || msg.Contains("titulo"));
        }

        [Fact]
        public async Task CreateAviso_WithMissingMensagem_ShouldReturn400WithValidationErrors()
        {
            // Arrange
            var request = new
            {
                titulo = "Título sem mensagem"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/avisos", request);

            // Assert - HTTP Status
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Assert - Validation Error Schema
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            root.TryGetProperty("Mensagens", out var mensagens).Should().BeTrue();
            
            var errorMessages = mensagens.EnumerateArray()
                .Select(m => m.GetString().ToLowerInvariant())
                .ToList();
            
            errorMessages.Should().Contain(msg => msg.Contains("mensagem"));
        }

        [Fact]
        public async Task CreateAviso_WithEmptyBody_ShouldReturn400Or415()
        {
            // Act
            var response = await _client.PostAsync("/api/v1/avisos", null);

            // Assert - API pode retornar 400 ou 415 dependendo da implementação
            response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.UnsupportedMediaType);
        }

        #endregion

        #region GET /api/v1/avisos - Contract Tests

        [Fact]
        public async Task GetAvisos_ShouldReturnCorrectSchema()
        {
            // Arrange - Criar um aviso primeiro
            var createRequest = new
            {
                titulo = "Aviso para GET",
                mensagem = "Mensagem para teste GET"
            };
            var createResponse = await _client.PostAsJsonAsync("/api/v1/avisos", createRequest);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createContent);
            var avisoId = createJson.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();

            try
            {
                // Act
                var response = await _client.GetAsync("/api/v1/avisos");

                // Assert - HTTP Status
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Content Type
                response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

                // Assert - Response Schema
                var content = await response.Content.ReadAsStringAsync();
                content.Should().NotBeNullOrEmpty();

                var jsonDocument = JsonDocument.Parse(content);
                var root = jsonDocument.RootElement;

                root.TryGetProperty("Dados", out var dados).Should().BeTrue();
                dados.ValueKind.Should().Be(JsonValueKind.Array);
                dados.GetArrayLength().Should().BeGreaterThan(0);

                var firstItem = dados[0];
                
                firstItem.TryGetProperty("Id", out var id).Should().BeTrue();
                id.ValueKind.Should().Be(JsonValueKind.Number);

                firstItem.TryGetProperty("Titulo", out var titulo).Should().BeTrue();
                titulo.ValueKind.Should().Be(JsonValueKind.String);

                firstItem.TryGetProperty("Mensagem", out var mensagem).Should().BeTrue();
                mensagem.ValueKind.Should().Be(JsonValueKind.String);

                firstItem.TryGetProperty("DataCriacao", out var dataCriacao).Should().BeTrue();
                dataCriacao.ValueKind.Should().Be(JsonValueKind.String);

                firstItem.TryGetProperty("Ativo", out var ativo).Should().BeTrue();
                ativo.ValueKind.Should().Be(JsonValueKind.True);
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        #endregion

        #region GET /api/v1/avisos/{id} - Contract Tests

        [Fact]
        public async Task GetAviso_WithExistingId_ShouldReturn200WithCorrectSchema()
        {
            // Arrange - Criar um aviso primeiro
            var createRequest = new
            {
                titulo = "Aviso para GET por ID",
                mensagem = "Mensagem para teste GET por ID"
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/v1/avisos", createRequest);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createContent);
            var avisoId = createJson.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();

            try
            {
                // Act
                var response = await _client.GetAsync($"/api/v1/avisos/{avisoId}");

                // Assert - HTTP Status
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Content Type
                response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

                // Assert - Response Schema
                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);
                var root = jsonDocument.RootElement;

                root.TryGetProperty("Dados", out var dados).Should().BeTrue();
                
                dados.TryGetProperty("Id", out var id).Should().BeTrue();
                id.GetInt32().Should().Be(avisoId);

                dados.TryGetProperty("Titulo", out var titulo).Should().BeTrue();
                titulo.GetString().Should().Be(createRequest.titulo);

                dados.TryGetProperty("Mensagem", out var mensagem).Should().BeTrue();
                mensagem.GetString().Should().Be(createRequest.mensagem);
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        [Fact]
        public async Task GetAviso_WithNonExistentId_ShouldReturn404()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/avisos/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetAviso_WithInvalidId_ShouldReturn400()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/avisos/invalid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region PUT /api/v1/avisos/{id} - Contract Tests

        [Fact]
        public async Task UpdateAviso_WithValidRequest_ShouldReturn200WithCorrectSchema()
        {
            // Arrange - Criar um aviso primeiro
            var createRequest = new
            {
                titulo = "Aviso para Update",
                mensagem = "Mensagem original"
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/v1/avisos", createRequest);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createContent);
            var avisoId = createJson.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();

            var updateRequest = new
            {
                mensagem = "Mensagem atualizada"
            };

            try
            {
                // Act
                var response = await _client.PutAsJsonAsync($"/api/v1/avisos/{avisoId}", updateRequest);

                // Assert - HTTP Status
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                // Assert - Content Type
                response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

                // Assert - Response Schema
                var content = await response.Content.ReadAsStringAsync();
                var jsonDocument = JsonDocument.Parse(content);
                var root = jsonDocument.RootElement;

                root.TryGetProperty("Dados", out var dados).Should().BeTrue();
                
                dados.TryGetProperty("Id", out var id).Should().BeTrue();
                id.GetInt32().Should().Be(avisoId);

                dados.TryGetProperty("Mensagem", out var mensagem).Should().BeTrue();
                mensagem.GetString().Should().Be(updateRequest.mensagem);
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        [Fact]
        public async Task UpdateAviso_WithNonExistentId_ShouldReturn404()
        {
            // Arrange
            var updateRequest = new
            {
                mensagem = "Mensagem para ID inexistente"
            };

            // Act
            var response = await _client.PutAsJsonAsync("/api/v1/avisos/99999", updateRequest);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region DELETE /api/v1/avisos/{id} - Contract Tests

        [Fact]
        public async Task DeleteAviso_WithExistingId_ShouldReturn200()
        {
            // Arrange - Criar um aviso primeiro
            var createRequest = new
            {
                titulo = "Aviso para Delete",
                mensagem = "Mensagem para delete"
            };
            
            var createResponse = await _client.PostAsJsonAsync("/api/v1/avisos", createRequest);
            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createContent);
            var avisoId = createJson.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();

            // Act
            var response = await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");

            // Assert - HTTP Status
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Assert - Content Type
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

            // Assert - Response Schema
            var content = await response.Content.ReadAsStringAsync();
            var jsonDocument = JsonDocument.Parse(content);
            var root = jsonDocument.RootElement;

            root.TryGetProperty("Dados", out var dados).Should().BeTrue();
            dados.GetBoolean().Should().BeTrue();
        }

        [Fact]
        public async Task DeleteAviso_WithNonExistentId_ShouldReturn404()
        {
            // Act
            var response = await _client.DeleteAsync("/api/v1/avisos/99999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task DeleteAviso_WithInvalidId_ShouldReturn400()
        {
            // Act
            var response = await _client.DeleteAsync("/api/v1/avisos/invalid");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        #endregion

        #region Content-Type and Headers Contract Tests

        [Fact]
        public async Task AllEndpoints_ShouldAcceptJsonContentType()
        {
            // Arrange
            var request = new
            {
                titulo = "Teste Content-Type",
                mensagem = "Teste de cabeçalhos"
            };

            using var jsonContent = JsonContent.Create(request);
            jsonContent.Headers.ContentType!.MediaType.Should().Be("application/json");

            // Act
            var response = await _client.PostAsync("/api/v1/avisos", jsonContent);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);

            // Cleanup
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var avisoId = json.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        [Fact]
        public async Task AllEndpoints_ShouldReturnJsonContentType()
        {
            // Act
            var response = await _client.GetAsync("/api/v1/avisos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        }

        #endregion

        #region API Version Contract Tests

        [Fact]
        public async Task ApiEndpoints_ShouldAcceptV1Version()
        {
            // Act & Assert - todos os endpoints devem responder na v1
            var getResponse = await _client.GetAsync("/api/v1/avisos");
            getResponse.StatusCode.Should().NotBe(HttpStatusCode.NotFound);

            var postRequest = new { titulo = "Teste V1", mensagem = "Teste versão" };
            var postResponse = await _client.PostAsJsonAsync("/api/v1/avisos", postRequest);
            postResponse.StatusCode.Should().NotBe(HttpStatusCode.NotFound);

            // Cleanup
            if (postResponse.StatusCode == HttpStatusCode.OK)
            {
                var content = await postResponse.Content.ReadAsStringAsync();
                var json = JsonDocument.Parse(content);
                var avisoId = json.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        [Fact]
        public async Task ApiEndpoints_WithoutVersion_ShouldReturn404()
        {
            // Act
            var response = await _client.GetAsync("/api/avisos");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion
    }
}