using Bernhoeft.GRT.ContractWeb.Domain.SqlServer.ContractStore.Entities;
using Bernhoeft.GRT.Teste.UnitTests.Fixtures;
using FluentAssertions;
using Xunit;

namespace Bernhoeft.GRT.Teste.UnitTests.Entities
{
    public class AvisoEntityTests
    {
        [Fact]
        public void AvisoEntity_DefaultValues_ShouldBeSetCorrectly()
        {
            // Act
            var aviso = new AvisoEntity();

            // Assert
            aviso.Id.Should().Be(0);
            aviso.Titulo.Should().BeNull();
            aviso.Mensagem.Should().BeNull();
            aviso.Ativo.Should().BeTrue();
            aviso.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
            aviso.DataModificacao.Should().BeNull();
        }

        [Fact]
        public void AvisoEntity_SetProperties_ShouldWorkCorrectly()
        {
            // Arrange
            var dataAtual = DateTime.UtcNow;
            var aviso = new AvisoEntity();

            // Act
            aviso.Titulo = "Título de Teste";
            aviso.Mensagem = "Mensagem de teste";
            aviso.Ativo = false;
            aviso.DataCriacao = dataAtual;
            aviso.DataModificacao = dataAtual.AddMinutes(5);

            // Assert
            aviso.Titulo.Should().Be("Título de Teste");
            aviso.Mensagem.Should().Be("Mensagem de teste");
            aviso.Ativo.Should().BeFalse();
            aviso.DataCriacao.Should().Be(dataAtual);
            aviso.DataModificacao.Should().Be(dataAtual.AddMinutes(5));
        }

        [Fact]
        public void Desativar_ShouldSetAtivoToFalseAndSetDataModificacao()
        {
            // Arrange
            var aviso = new AvisoEntity();
            aviso.Ativo = true;
            var beforeCall = DateTime.UtcNow;

            // Act
            aviso.Desativar();
            var afterCall = DateTime.UtcNow;

            // Assert
            aviso.Ativo.Should().BeFalse();
            aviso.DataModificacao.Should().NotBeNull();
            aviso.DataModificacao.Value.Should().BeOnOrAfter(beforeCall);
            aviso.DataModificacao.Value.Should().BeOnOrBefore(afterCall);
        }

        [Fact]
        public void DefinirDataModificacao_ShouldSetCurrentDateTime()
        {
            // Arrange
            var aviso = new AvisoEntity();
            var beforeExecution = DateTime.UtcNow;

            // Act
            aviso.DefinirDataModificacao();
            var afterExecution = DateTime.UtcNow;

            // Assert
            aviso.DataModificacao.Should().NotBeNull();
            aviso.DataModificacao.Value.Should().BeOnOrAfter(beforeExecution);
            aviso.DataModificacao.Value.Should().BeOnOrBefore(afterExecution);
        }

        [Fact]
        public void DefinirDataModificacao_CalledMultipleTimes_ShouldUpdateToLatestTime()
        {
            // Arrange
            var aviso = new AvisoEntity();
            
            // Act
            aviso.DefinirDataModificacao();
            var firstUpdate = aviso.DataModificacao;
            
            Thread.Sleep(10); // Pequeno delay para garantir diferença de tempo
            
            aviso.DefinirDataModificacao();
            var secondUpdate = aviso.DataModificacao;

            // Assert
            secondUpdate.Should().NotBeNull();
            firstUpdate.Should().NotBeNull();
            secondUpdate.Value.Should().BeAfter(firstUpdate.Value);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void AvisoEntity_EmptyOrNullTitulo_ShouldBeAllowed(string titulo)
        {
            // Arrange & Act
            var aviso = new AvisoEntity
            {
                Titulo = titulo,
                Mensagem = "Mensagem válida"
            };

            // Assert
            aviso.Titulo.Should().Be(titulo);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public void AvisoEntity_EmptyOrNullMensagem_ShouldBeAllowed(string mensagem)
        {
            // Arrange & Act
            var aviso = new AvisoEntity
            {
                Titulo = "Título válido",
                Mensagem = mensagem
            };

            // Assert
            aviso.Mensagem.Should().Be(mensagem);
        }

        [Fact]
        public void AvisoEntity_LongStrings_ShouldBeAccepted()
        {
            // Arrange
            var longTitle = new string('A', 500);
            var longMessage = new string('B', 2000);

            // Act
            var aviso = new AvisoEntity
            {
                Titulo = longTitle,
                Mensagem = longMessage
            };

            // Assert
            aviso.Titulo.Should().Be(longTitle);
            aviso.Mensagem.Should().Be(longMessage);
        }

        [Fact]
        public void AvisoEntity_DataCriacao_ShouldAllowPastDates()
        {
            // Arrange
            var pastDate = DateTime.UtcNow.AddDays(-30);

            // Act
            var aviso = new AvisoEntity
            {
                DataCriacao = pastDate
            };

            // Assert
            aviso.DataCriacao.Should().Be(pastDate);
        }

        [Fact]
        public void AvisoEntity_DataModificacao_ShouldAllowNullValue()
        {
            // Arrange & Act
            var aviso = new AvisoEntity
            {
                DataModificacao = null
            };

            // Assert
            aviso.DataModificacao.Should().BeNull();
        }
    }
}