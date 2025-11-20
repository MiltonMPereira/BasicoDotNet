using Bernhoeft.GRT.Teste.Application.Requests.Commands.v1;
using FluentValidation;

namespace Bernhoeft.GRT.Teste.Application.Validators.Commands.v1
{
    public class UpdateAvisoRequestBodyValidator : AbstractValidator<UpdateAvisoRequestBody>
    {
        public UpdateAvisoRequestBodyValidator()
        {
            RuleFor(x => x.Mensagem)
                .NotEmpty()
                .WithMessage("A mensagem é obrigatória.")
                .MaximumLength(1000)
                .WithMessage("A mensagem deve ter no máximo 1000 caracteres.");
        }
    }
}