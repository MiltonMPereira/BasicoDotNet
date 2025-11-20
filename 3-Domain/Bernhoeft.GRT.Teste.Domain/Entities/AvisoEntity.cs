namespace Bernhoeft.GRT.ContractWeb.Domain.SqlServer.ContractStore.Entities
{
    public partial class AvisoEntity
    {
        public int Id { get; private set; }
        public bool Ativo { get; set; } = true;
        public string Titulo { get; set; }
        public string Mensagem { get; set; }
        
        // Campos de auditoria - controle de criação e modificação
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
        public DateTime? DataModificacao { get; set; }
        
        /// <summary>
        /// Atualiza a data de modificação para o momento atual
        /// </summary>
        public void DefinirDataModificacao()
        {
            DataModificacao = DateTime.UtcNow;
        }
        
        /// <summary>
        /// Realiza soft delete do aviso
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
            DefinirDataModificacao();
        }
    }
}