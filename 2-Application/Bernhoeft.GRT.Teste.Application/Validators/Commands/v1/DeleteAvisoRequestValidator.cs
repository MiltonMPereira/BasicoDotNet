using Bernhoeft.GRT.Teste.Application.Requests.Commands.v1;
using FluentValidation;

namespace Bernhoeft.GRT.Teste.Application.Validators.Commands.v1
{
    public class DeleteAvisoRequestValidator : AbstractValidator<DeleteAvisoRequest>
    {
        public DeleteAvisoRequestValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0)
                .WithMessage("O ID do aviso deve ser maior que zero.");
        }
    }
}