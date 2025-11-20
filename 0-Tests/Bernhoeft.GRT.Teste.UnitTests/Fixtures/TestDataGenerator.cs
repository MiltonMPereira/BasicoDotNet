using Bernhoeft.GRT.Teste.Application.Requests.Commands.v1;
using Bernhoeft.GRT.Teste.Application.Requests.Queries.v1;
using Bernhoeft.GRT.ContractWeb.Domain.SqlServer.ContractStore.Entities;

namespace Bernhoeft.GRT.Teste.UnitTests.Fixtures
{
    public static class TestDataGenerator
    {
        private static readonly Random _random = new();

        public static CreateAvisoRequest CreateValidCreateAvisoRequest()
        {
            return new CreateAvisoRequest
            {
                Titulo = $"Título Teste {_random.Next(1, 1000)}",
                Mensagem = $"Mensagem de teste {_random.Next(1, 1000)}"
            };
        }

        public static GetAvisosRequest CreateValidGetAvisosRequest()
        {
            return new GetAvisosRequest();
        }

        public static AvisoEntity CreateValidAvisoEntity()
        {
            return new AvisoEntity
            {
                Titulo = $"Título {_random.Next(1, 1000)}",
                Mensagem = $"Mensagem {_random.Next(1, 1000)}",
                Ativo = _random.Next(0, 2) == 1,
                DataCriacao = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                DataModificacao = _random.Next(0, 2) == 1 ? DateTime.UtcNow.AddDays(-_random.Next(1, 10)) : null
            };
        }

        public static List<AvisoEntity> CreateValidAvisoEntityList(int count = 5)
        {
            var avisos = new List<AvisoEntity>();
            for (int i = 0; i < count; i++)
            {
                avisos.Add(new AvisoEntity
                {
                    Titulo = $"Título {i + 1}",
                    Mensagem = $"Mensagem {i + 1}",
                    Ativo = i % 2 == 0, // Alterna entre true/false
                    DataCriacao = DateTime.UtcNow.AddDays(-i),
                    DataModificacao = i % 3 == 0 ? DateTime.UtcNow.AddDays(-i + 1) : null
                });
            }
            return avisos;
        }

        public static string GenerateRandomString(int maxLength = 50)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 ";
            var length = Math.Min(maxLength, _random.Next(5, maxLength));
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[_random.Next(s.Length)]).ToArray());
        }

        public static DateTime GenerateRandomDate()
        {
            return DateTime.UtcNow.AddDays(-_random.Next(1, 365));
        }
    }
}