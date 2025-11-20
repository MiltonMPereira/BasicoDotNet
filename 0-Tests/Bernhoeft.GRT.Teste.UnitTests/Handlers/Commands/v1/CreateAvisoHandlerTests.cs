using Bernhoeft.GRT.Teste.Application.Requests.Commands.v1;
using FluentAssertions;
using Xunit;

namespace Bernhoeft.GRT.Teste.UnitTests.Handlers.Commands.v1
{
    public class CreateAvisoHandlerTests
    {
        [Fact]
        public void CreateAvisoRequest_ValidData_ShouldHaveCorrectProperties()
        {
            // Arrange & Act
            var request = new CreateAvisoRequest
            {
                Titulo = "Teste Título",
                Mensagem = "Teste Mensagem"
            };

            // Assert
            request.Titulo.Should().Be("Teste Título");
            request.Mensagem.Should().Be("Teste Mensagem");
        }

        [Theory]
        [InlineData("Título 1", "Mensagem 1")]
        [InlineData("Título 2", "Mensagem 2")]
        [InlineData("", "")]
        [InlineData(null, null)]
        public void CreateAvisoRequest_DifferentValues_ShouldSetCorrectly(string titulo, string mensagem)
        {
            // Arrange & Act
            var request = new CreateAvisoRequest
            {
                Titulo = titulo,
                Mensagem = mensagem
            };

            // Assert
            request.Titulo.Should().Be(titulo);
            request.Mensagem.Should().Be(mensagem);
        }
    }
}