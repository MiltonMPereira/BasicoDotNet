using Bernhoeft.GRT.Teste.Application.Responses.Queries.v1;
using Bernhoeft.GRT.ContractWeb.Domain.SqlServer.ContractStore.Entities;
using FluentAssertions;
using Xunit;

namespace Bernhoeft.GRT.Teste.UnitTests.Responses.Queries.v1
{
    public class GetAvisosResponseTests
    {
        [Fact]
        public void ImplicitOperator_FromAvisoEntity_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new AvisoEntity
            {
                Titulo = "Título Teste",
                Mensagem = "Mensagem Teste",
                Ativo = true,
                DataCriacao = DateTime.UtcNow.AddDays(-1),
                DataModificacao = DateTime.UtcNow
            };

            // Act
            GetAvisosResponse response = entity; // Teste do operador implícito

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(entity.Id);
            response.Titulo.Should().Be(entity.Titulo);
            response.Mensagem.Should().Be(entity.Mensagem);
            response.Ativo.Should().Be(entity.Ativo);
            response.DataCriacao.Should().Be(entity.DataCriacao);
            response.DataModificacao.Should().Be(entity.DataModificacao);
        }

        [Fact]
        public void ImplicitOperator_FromAvisoEntityWithNullDataModificacao_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new AvisoEntity
            {
                Titulo = "Outro Título",
                Mensagem = "Outra Mensagem",
                Ativo = false,
                DataCriacao = DateTime.UtcNow.AddDays(-5),
                DataModificacao = null
            };

            // Act
            GetAvisosResponse response = entity;

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(entity.Id);
            response.Titulo.Should().Be(entity.Titulo);
            response.Mensagem.Should().Be(entity.Mensagem);
            response.Ativo.Should().Be(entity.Ativo);
            response.DataCriacao.Should().Be(entity.DataCriacao);
            response.DataModificacao.Should().BeNull();
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void ImplicitOperator_WithEmptyOrNullTitulo_ShouldMapCorrectly(string titulo)
        {
            // Arrange
            var entity = new AvisoEntity
            {
                Titulo = titulo,
                Mensagem = "Mensagem válida",
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            // Act
            GetAvisosResponse response = entity;

            // Assert
            response.Should().NotBeNull();
            response.Titulo.Should().Be(titulo);
            response.Mensagem.Should().Be("Mensagem válida");
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("   ")]
        public void ImplicitOperator_WithEmptyOrNullMensagem_ShouldMapCorrectly(string mensagem)
        {
            // Arrange
            var entity = new AvisoEntity
            {
                Titulo = "Título válido",
                Mensagem = mensagem,
                Ativo = false,
                DataCriacao = DateTime.UtcNow
            };

            // Act
            GetAvisosResponse response = entity;

            // Assert
            response.Should().NotBeNull();
            response.Titulo.Should().Be("Título válido");
            response.Mensagem.Should().Be(mensagem);
        }

        [Fact]
        public void ImplicitOperator_WithMinimalEntity_ShouldMapCorrectly()
        {
            // Arrange
            var entity = new AvisoEntity
            {
                Titulo = null,
                Mensagem = null,
                Ativo = false,
                DataCriacao = default,
                DataModificacao = null
            };

            // Act
            GetAvisosResponse response = entity;

            // Assert
            response.Should().NotBeNull();
            response.Id.Should().Be(0);
            response.Titulo.Should().BeNull();
            response.Mensagem.Should().BeNull();
            response.Ativo.Should().BeFalse();
            response.DataCriacao.Should().Be(default);
            response.DataModificacao.Should().BeNull();
        }
    }
}