using Bernhoeft.GRT.Core.Interfaces.Results;
using Bernhoeft.GRT.Teste.Application.Responses.Commands.v1;
using MediatR;

namespace Bernhoeft.GRT.Teste.Application.Requests.Commands.v1
{
    public class CreateAvisoRequest : IRequest<IOperationResult<CreateAvisoResponse>>
    {
        /// <summary>
        /// Título do aviso
        /// </summary>
        public string Titulo { get; set; }
        
        /// <summary>
        /// Mensagem do aviso
        /// </summary>
        public string Mensagem { get; set; }
    }
}