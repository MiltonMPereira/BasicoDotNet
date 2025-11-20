using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace Bernhoeft.GRT.Teste.ContractTests.Workflows
{
    /// <summary>
    /// Testes de contrato para fluxos de trabalho (workflows) da API
    /// Testa cenários end-to-end e integrações entre endpoints
    /// </summary>
    public class WorkflowContractTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public WorkflowContractTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory;
            _client = _factory.CreateClient();
        }

        #region CRUD Workflow Tests

        [Fact]
        public async Task CompleteAvisoCRUD_ShouldWorkAsExpected()
        {
            // Step 1: CREATE
            var createRequest = new
            {
                titulo = "Aviso Workflow Test",
                mensagem = "Mensagem inicial do workflow"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/v1/avisos", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createContent);
            var avisoId = createJson.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();

            // Step 2: READ (Get by ID)
            var getResponse = await _client.GetAsync($"/api/v1/avisos/{avisoId}");
            getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var getContent = await getResponse.Content.ReadAsStringAsync();
            var getJson = JsonDocument.Parse(getContent);
            var avisoData = getJson.RootElement.GetProperty("Dados");

            avisoData.GetProperty("Id").GetInt32().Should().Be(avisoId);
            avisoData.GetProperty("Titulo").GetString().Should().Be(createRequest.titulo);
            avisoData.GetProperty("Mensagem").GetString().Should().Be(createRequest.mensagem);

            // Step 3: UPDATE
            var updateRequest = new
            {
                mensagem = "Mensagem atualizada no workflow"
            };

            var updateResponse = await _client.PutAsJsonAsync($"/api/v1/avisos/{avisoId}", updateRequest);
            updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var updateContent = await updateResponse.Content.ReadAsStringAsync();
            var updateJson = JsonDocument.Parse(updateContent);
            var updatedData = updateJson.RootElement.GetProperty("Dados");

            updatedData.GetProperty("Mensagem").GetString().Should().Be(updateRequest.mensagem);

            // Step 4: Verify UPDATE (Read again)
            var getUpdatedResponse = await _client.GetAsync($"/api/v1/avisos/{avisoId}");
            getUpdatedResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var getUpdatedContent = await getUpdatedResponse.Content.ReadAsStringAsync();
            var getUpdatedJson = JsonDocument.Parse(getUpdatedContent);
            var updatedAvisoData = getUpdatedJson.RootElement.GetProperty("Dados");

            updatedAvisoData.GetProperty("Mensagem").GetString().Should().Be(updateRequest.mensagem);

            // Step 5: DELETE
            var deleteResponse = await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var deleteContent = await deleteResponse.Content.ReadAsStringAsync();
            var deleteJson = JsonDocument.Parse(deleteContent);
            deleteJson.RootElement.GetProperty("Dados").GetBoolean().Should().BeTrue();

            // Step 6: Verify DELETE (Should return 404)
            var getDeletedResponse = await _client.GetAsync($"/api/v1/avisos/{avisoId}");
            getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        #endregion

        #region Error Recovery Workflow Tests

        [Fact]
        public async Task FailedCreate_ThenSuccessfulCreate_ShouldWork()
        {
            // Step 1: Try invalid create
            var invalidRequest = new
            {
                titulo = "", // Invalid
                mensagem = "Mensagem válida"
            };

            var invalidResponse = await _client.PostAsJsonAsync("/api/v1/avisos", invalidRequest);
            invalidResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            // Step 2: Try valid create with same client
            var validRequest = new
            {
                titulo = "Título válido após erro",
                mensagem = "Mensagem válida após erro"
            };

            var validResponse = await _client.PostAsJsonAsync("/api/v1/avisos", validRequest);
            validResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            // Step 3: Verify the valid one was created
            var content = await validResponse.Content.ReadAsStringAsync();
            var json = JsonDocument.Parse(content);
            var avisoId = json.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();

            // Cleanup
            await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
        }

        [Fact]
        public async Task UpdateNonExistent_ThenCreateAndUpdate_ShouldWork()
        {
            // Step 1: Try to update non-existent
            var updateRequest = new
            {
                mensagem = "Tentativa de update em ID inexistente"
            };

            var updateNonExistentResponse = await _client.PutAsJsonAsync("/api/v1/avisos/99999", updateRequest);
            updateNonExistentResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

            // Step 2: Create a real aviso
            var createRequest = new
            {
                titulo = "Aviso para update real",
                mensagem = "Mensagem original"
            };

            var createResponse = await _client.PostAsJsonAsync("/api/v1/avisos", createRequest);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var createContent = await createResponse.Content.ReadAsStringAsync();
            var createJson = JsonDocument.Parse(createContent);
            var avisoId = createJson.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();

            try
            {
                // Step 3: Update the real aviso
                var realUpdateRequest = new
                {
                    mensagem = "Mensagem atualizada para real"
                };

                var realUpdateResponse = await _client.PutAsJsonAsync($"/api/v1/avisos/{avisoId}", realUpdateRequest);
                realUpdateResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var updateContent = await realUpdateResponse.Content.ReadAsStringAsync();
                var updateJson = JsonDocument.Parse(updateContent);
                var updatedData = updateJson.RootElement.GetProperty("Dados");

                updatedData.GetProperty("Mensagem").GetString().Should().Be(realUpdateRequest.mensagem);
            }
            finally
            {
                // Cleanup
                await _client.DeleteAsync($"/api/v1/avisos/{avisoId}");
            }
        }

        #endregion

        #region Concurrent Operations Workflow Tests

        [Fact]
        public async Task ConcurrentCreateOperations_ShouldAllSucceed()
        {
            // Arrange - Multiple create requests
            var requests = Enumerable.Range(1, 5).Select(i => new
            {
                titulo = $"Concurrent Aviso {i}",
                mensagem = $"Mensagem concurrent {i}"
            }).ToArray();

            // Act - Send all requests concurrently
            var tasks = requests.Select(req => 
                _client.PostAsJsonAsync("/api/v1/avisos", req)
            ).ToArray();

            var responses = await Task.WhenAll(tasks);

            try
            {
                // Assert - All should succeed
                var createdIds = new List<int>();

                for (int i = 0; i < responses.Length; i++)
                {
                    responses[i].StatusCode.Should().Be(HttpStatusCode.OK);

                    var content = await responses[i].Content.ReadAsStringAsync();
                    var json = JsonDocument.Parse(content);
                    var data = json.RootElement.GetProperty("Dados");

                    var avisoId = data.GetProperty("Id").GetInt32();
                    createdIds.Add(avisoId);

                    data.GetProperty("Titulo").GetString().Should().Be(requests[i].titulo);
                    data.GetProperty("Mensagem").GetString().Should().Be(requests[i].mensagem);
                }

                // Verify all have unique IDs
                createdIds.Should().OnlyHaveUniqueItems();

                // Cleanup
                foreach (var id in createdIds)
                {
                    await _client.DeleteAsync($"/api/v1/avisos/{id}");
                }
            }
            finally
            {
                // Dispose responses
                foreach (var response in responses)
                {
                    response.Dispose();
                }
            }
        }

        #endregion

        #region State Consistency Workflow Tests

        [Fact]
        public async Task CreateUpdateDelete_ShouldMaintainConsistentState()
        {
            // Track all avisos we create for cleanup
            var createdIds = new List<int>();

            try
            {
                // Create multiple avisos
                for (int i = 1; i <= 3; i++)
                {
                    var createRequest = new
                    {
                        titulo = $"State Test Aviso {i}",
                        mensagem = $"Initial message {i}"
                    };

                    var createResponse = await _client.PostAsJsonAsync("/api/v1/avisos", createRequest);
                    createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                    var createContent = await createResponse.Content.ReadAsStringAsync();
                    var createJson = JsonDocument.Parse(createContent);
                    var avisoId = createJson.RootElement.GetProperty("Dados").GetProperty("Id").GetInt32();
                    createdIds.Add(avisoId);
                }

                // Update some of them
                for (int i = 0; i < 2; i++)
                {
                    var updateRequest = new
                    {
                        mensagem = $"Updated message {i + 1}"
                    };

                    var updateResponse = await _client.PutAsJsonAsync($"/api/v1/avisos/{createdIds[i]}", updateRequest);
                    updateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
                }

                // Verify state consistency
                var listResponse = await _client.GetAsync("/api/v1/avisos");
                listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var listContent = await listResponse.Content.ReadAsStringAsync();
                var listJson = JsonDocument.Parse(listContent);
                var avisos = listJson.RootElement.GetProperty("Dados");

                var ourAvisos = avisos.EnumerateArray()
                    .Where(a => createdIds.Contains(a.GetProperty("Id").GetInt32()))
                    .ToList();

                ourAvisos.Should().HaveCount(3);

                // Verify updates were applied
                var updatedAvisos = ourAvisos
                    .Where(a => createdIds.Take(2).Contains(a.GetProperty("Id").GetInt32()))
                    .ToList();

                foreach (var aviso in updatedAvisos)
                {
                    aviso.GetProperty("Mensagem").GetString().Should().StartWith("Updated message");
                }

                // Delete one and verify it's gone
                var deleteResponse = await _client.DeleteAsync($"/api/v1/avisos/{createdIds[0]}");
                deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

                var getDeletedResponse = await _client.GetAsync($"/api/v1/avisos/{createdIds[0]}");
                getDeletedResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);

                // Remove from tracking since it's deleted
                createdIds.RemoveAt(0);
            }
            finally
            {
                // Cleanup remaining avisos
                foreach (var id in createdIds)
                {
                    await _client.DeleteAsync($"/api/v1/avisos/{id}");
                }
            }
        }

        #endregion
    }
}