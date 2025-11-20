using Bernhoeft.GRT.ContractWeb.Domain.SqlServer.ContractStore.Interfaces.Repositories;
using Bernhoeft.GRT.Core.EntityFramework.Domain.Interfaces;
using Bernhoeft.GRT.Core.Interfaces.Results;
using Bernhoeft.GRT.Core.Models;
using Bernhoeft.GRT.Teste.Application.Requests.Commands.v1;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Bernhoeft.GRT.Teste.Application.Handlers.Commands.v1
{
    public class DeleteAvisoHandler : IRequestHandler<DeleteAvisoRequest, IOperationResult<bool>>
    {
        private readonly IServiceProvider _serviceProvider;
        
        private IContext _context => _serviceProvider.GetRequiredService<IContext>();
        private IAvisoRepository _avisoRepository => _serviceProvider.GetRequiredService<IAvisoRepository>();

        public DeleteAvisoHandler(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

        public async Task<IOperationResult<bool>> Handle(DeleteAvisoRequest request, CancellationToken cancellationToken)
        {
            var aviso = await _avisoRepository.ObterAvisoPorIdAsync(request.Id, cancellationToken: cancellationToken);
            
            if (aviso is null)
                return OperationResult<bool>.ReturnNotFound();

            // Soft Delete - apenas marca como inativo
            aviso.Desativar();

            _avisoRepository.Update(aviso);
            await _context.SaveChangesAsync(cancellationToken);

            return OperationResult<bool>.ReturnOk(true);
        }
    }
}