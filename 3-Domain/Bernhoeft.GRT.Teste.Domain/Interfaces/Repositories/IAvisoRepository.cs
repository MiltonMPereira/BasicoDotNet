using Bernhoeft.GRT.ContractWeb.Domain.SqlServer.ContractStore.Entities;
using Bernhoeft.GRT.Core.EntityFramework.Domain.Interfaces;
using Bernhoeft.GRT.Core.Enums;

namespace Bernhoeft.GRT.ContractWeb.Domain.SqlServer.ContractStore.Interfaces.Repositories
{
    public interface IAvisoRepository : IRepository<AvisoEntity>
    {
        /// <summary>
        /// Retorna todos os avisos ativos
        /// </summary>
        Task<List<AvisoEntity>> ObterTodosAvisosAsync(TrackingBehavior tracking = TrackingBehavior.Default, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Retorna um aviso ativo por ID
        /// </summary>
        Task<AvisoEntity> ObterAvisoPorIdAsync(int id, TrackingBehavior tracking = TrackingBehavior.Default, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Verifica se existe um aviso ativo com o ID especificado
        /// </summary>
        Task<bool> ExisteAvisoAtivoAsync(int id, CancellationToken cancellationToken = default);
    }
}