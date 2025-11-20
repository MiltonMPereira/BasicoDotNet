using Bernhoeft.GRT.Teste.Application.Requests.Commands.v1;
using Bernhoeft.GRT.Teste.Application.Validators.Commands.v1;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace Bernhoeft.GRT.Teste.UnitTests.Validators.Commands.v1
{
    public class CreateAvisoRequestValidatorTests
    {
        private readonly CreateAvisoRequestValidator _validator;

        public CreateAvisoRequestValidatorTests()
        {
            _validator = new CreateAvisoRequestValidator();
        }

        [Fact]
        public void Validator_ValidRequest_ShouldNotHaveValidationErrors()
        {
            // Arrange
            var request = new CreateAvisoRequest
            {
                Titulo = "Título Válido",
                Mensagem = "Mensagem válida para o aviso"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validator_EmptyTitulo_ShouldHaveValidationError(string titulo)
        {
            // Arrange
            var request = new CreateAvisoRequest
            {
                Titulo = titulo,
                Mensagem = "Mensagem válida"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Titulo)
                  .WithErrorMessage("O título é obrigatório.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Validator_EmptyMensagem_ShouldHaveValidationError(string mensagem)
        {
            // Arrange
            var request = new CreateAvisoRequest
            {
                Titulo = "Título válido",
                Mensagem = mensagem
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Mensagem)
                  .WithErrorMessage("A mensagem é obrigatória.");
        }

        [Fact]
        public void Validator_TituloExceedsMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var longTitle = new string('A', 201); // 201 caracteres
            var request = new CreateAvisoRequest
            {
                Titulo = longTitle,
                Mensagem = "Mensagem válida"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Titulo)
                  .WithErrorMessage("O título deve ter no máximo 200 caracteres.");
        }

        [Fact]
        public void Validator_MensagemExceedsMaxLength_ShouldHaveValidationError()
        {
            // Arrange
            var longMessage = new string('A', 1001); // 1001 caracteres
            var request = new CreateAvisoRequest
            {
                Titulo = "Título válido",
                Mensagem = longMessage
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Mensagem)
                  .WithErrorMessage("A mensagem deve ter no máximo 1000 caracteres.");
        }

        [Fact]
        public void Validator_TituloAtMaxLength_ShouldNotHaveValidationError()
        {
            // Arrange
            var titleAtMaxLength = new string('A', 200); // Exatamente 200 caracteres
            var request = new CreateAvisoRequest
            {
                Titulo = titleAtMaxLength,
                Mensagem = "Mensagem válida"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Titulo);
        }

        [Fact]
        public void Validator_MensagemAtMaxLength_ShouldNotHaveValidationError()
        {
            // Arrange
            var messageAtMaxLength = new string('A', 1000); // Exatamente 1000 caracteres
            var request = new CreateAvisoRequest
            {
                Titulo = "Título válido",
                Mensagem = messageAtMaxLength
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Mensagem);
        }

        [Fact]
        public void Validator_BothFieldsInvalid_ShouldHaveMultipleValidationErrors()
        {
            // Arrange
            var request = new CreateAvisoRequest
            {
                Titulo = "", // Inválido
                Mensagem = "" // Inválido
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Titulo);
            result.ShouldHaveValidationErrorFor(x => x.Mensagem);
            result.Errors.Should().HaveCount(2);
        }

        [Fact]
        public void Validator_TituloOneCharacter_ShouldBeValid()
        {
            // Arrange
            var request = new CreateAvisoRequest
            {
                Titulo = "A",
                Mensagem = "Mensagem válida"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Titulo);
        }

        [Fact]
        public void Validator_MensagemOneCharacter_ShouldBeValid()
        {
            // Arrange
            var request = new CreateAvisoRequest
            {
                Titulo = "Título válido",
                Mensagem = "A"
            };

            // Act
            var result = _validator.TestValidate(request);

            // Assert
            result.ShouldNotHaveValidationErrorFor(x => x.Mensagem);
        }
    }
}