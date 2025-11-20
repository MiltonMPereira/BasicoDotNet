using Bernhoeft.GRT.Core.Interfaces.Results;
using MediatR;

namespace Bernhoeft.GRT.Teste.Application.Requests.Commands.v1
{
    public class DeleteAvisoRequest : IRequest<IOperationResult<bool>>
    {
        /// <summary>
        /// ID do aviso a ser removido
        /// </summary>
        public int Id { get; set; }
    }
}