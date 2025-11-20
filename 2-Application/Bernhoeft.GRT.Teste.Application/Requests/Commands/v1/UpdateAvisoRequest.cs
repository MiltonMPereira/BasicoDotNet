using Bernhoeft.GRT.Core.Interfaces.Results;
using Bernhoeft.GRT.Teste.Application.Responses.Commands.v1;
using MediatR;

namespace Bernhoeft.GRT.Teste.Application.Requests.Commands.v1
{
    public class UpdateAvisoRequest : IRequest<IOperationResult<UpdateAvisoResponse>>
    {
        /// <summary>
        /// ID do aviso a ser editado
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nova mensagem do aviso (apenas mensagem pode ser editada)
        /// </summary>
        public string Mensagem { get; set; }
    }
}