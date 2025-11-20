namespace Bernhoeft.GRT.Teste.Application.Requests.Commands.v1
{
    /// <summary>
    /// Body request para atualização de aviso (usado no endpoint PUT)
    /// </summary>
    public class UpdateAvisoRequestBody
    {
        /// <summary>
        /// Nova mensagem do aviso (apenas mensagem pode ser editada)
        /// </summary>
        public string Mensagem { get; set; }
    }
}