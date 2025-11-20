using System.Globalization;

namespace Bernhoeft.GRT.Teste.UnitTests
{
    /// <summary>
    /// Configurações globais para os testes unitários
    /// </summary>
    public static class TestConfiguration
    {
        static TestConfiguration()
        {
            // Configurar cultura para testes
            CultureInfo.CurrentCulture = new CultureInfo("pt-BR");
            CultureInfo.CurrentUICulture = new CultureInfo("pt-BR");
        }

        public static void Initialize()
        {
            // Método para forçar a inicialização estática
        }
    }

    /// <summary>
    /// Classe para executar configurações uma única vez durante os testes
    /// </summary>
    public class TestStartup
    {
        public TestStartup()
        {
            TestConfiguration.Initialize();
        }
    }
}