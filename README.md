# API de Avisos

API em .NET 9 para gerenciamento de avisos, utilizando Clean Architecture e Entity Framework (in-memory). Proposta de solução para o desafio proposto abaixo.

---

## 📝 Desafio Técnico

### 📌 Objetivo
Modificar a API existente para suportar novas funcionalidades relacionadas à avisos.

### 🧩 Tarefas Implementadas

**Endpoints obrigatórios:**
- `GET /avisos/{id}`: Retornar um aviso específico com base no ID
- `POST /avisos`: Criar um novo aviso
- `PUT /avisos/{id}`: Edita um aviso com base no ID
- `DELETE /avisos/{id}`: Remove um aviso (soft delete)

**Regras de negócio:**
- Controle de auditoria: DataCriacao e DataModificacao
- Validações com FluentValidation 
- Apenas campo mensagem pode ser editado
- Soft delete
- Filtros para retornar apenas avisos ativos

---

## ⚙️ Tecnologias

- .NET 9
- Entity Framework Core (In-Memory)
- FluentValidation
- MediatR (CQRS)
- Swagger/OpenAPI
- xUnit + FluentAssertions

---

## 🏛️ Arquitetura Implementada

### Clean Architecture - 4 Camadas
- **Domain**: Entidades e interfaces
- **Application**: Handlers, requests/responses, validadores
- **Infrastructure**: Repositórios e mapeamento EF Core
- **Presentation**: Controllers da API

### Padrões Aplicados
- **CQRS**: Separação de Commands e Queries com MediatR
- **Repository Pattern**: Abstração da camada de dados
- **Soft Delete**: Preservação de dados históricos
- **FluentValidation**: Validações centralizadas e testáveis
- **Implicit Operators**: Conversão automática Entity → Response

---

## 📊 Modelo de Dados

### AvisoEntity
```csharp
public class AvisoEntity
{
    public int Id { get; private set; }              // Chave primária
    public bool Ativo { get; set; } = true;         // Soft Delete
    public string Titulo { get; set; }              // Título do aviso
    public string Mensagem { get; set; }            // Conteúdo do aviso
    public DateTime DataCriacao { get; set; }       // Auditoria - Criação
    public DateTime? DataModificacao { get; set; }  // Auditoria - Modificação
    
    public void DefinirDataModificacao() => DataModificacao = DateTime.UtcNow;
    public void Desativar() { Ativo = false; DefinirDataModificacao(); }
}
```

### Decisões de Design
- **ID com setter privado**: Garante que apenas o EF Core pode definir o ID
- **Métodos de negócio**: Encapsula regras de auditoria e soft delete

---

## 📚 Documentação da API

### Endpoints

#### Lista Avisos
```http
GET /api/v1/avisos
```
- Retorna a lista de avisos ativos.
- **Success Response**: `200 OK` com lista de avisos.
- **Error Response**: `204 No Content` se não houver avisos.

#### Buscar Aviso por ID
```http
GET /api/v1/avisos/{id}
```
- Retorna um aviso específico pelo ID.
- **URL Params**: `id=[inteiro]` (obrigatório)
- **Success Response**: `200 OK` com dados do aviso.
- **Error Response**: `404 Not Found` se aviso não existir.

#### Criar Novo Aviso
```http
POST /api/v1/avisos
```
- Cria um novo aviso.
- **Body**: JSON com `titulo` e `mensagem`.
- **Success Response**: `200 OK` com dados do aviso criado.
- **Error Response**: `400 Bad Request` se validação falhar.

#### Atualizar Aviso
```http
PUT /api/v1/avisos/{id}
```
- Atualiza uma aviso existente (apenas mensagem).
- **URL Params**: `id=[inteiro]` (obrigatório)
- **Body**: JSON com `mensagem`.
- **Success Response**: `200 OK` com dados do aviso atualizado.
- **Error Response**: `404 Not Found` se aviso não existir.

#### Remover Aviso
```http
DELETE /api/v1/avisos/{id}
```
- Remove (soft delete) um aviso.
- **URL Params**: `id=[inteiro]` (obrigatório)
- **Success Response**: `200 OK` se remoção for bem-sucedida.
- **Error Response**: `404 Not Found` se aviso não existir.

---

## ✅ Validações Implementadas

### CreateAvisoRequest
- Título obrigatório 
- Título máximo 200 caracteres
- Mensagem obrigatória 
- Mensagem máximo 1000 caracteres

### UpdateAvisoRequest
- ID deve ser maior que zero
- Mensagem obrigatória 
- Mensagem máximo 1000 caracteres

### GetAviso/DeleteAvisoRequest
- ID deve ser maior que zero

**Justificativas das Validações:**
- **Validadores separados**: Cada request tem suas próprias regras
- **Limites de tamanho**: Baseados no mapeamento do banco de dados
- **Validação de ID**: Previne IDs inválidos (0, negativos) chegarem à aplicação

---

## 🔄 Handlers CQRS

### Command Handlers (Escrita)
- `CreateAvisoHandler` - Criação de avisos
- `UpdateAvisoHandler` - Atualização de avisos  
- `DeleteAvisoHandler` - Remoção (soft delete)

### Query Handlers (Leitura)
- `GetAvisoHandler` - Busca individual
- `GetAvisosHandler` - Listagem de ativos

**Benefícios CQRS:**
- **Separação clara**: Commands vs Queries com responsabilidades distintas
- **Otimização específica**: NoTracking em queries, tracking em commands
- **Escalabilidade**: Permite otimizações futuras (read replicas, cache)

---

## 🗃️ Repositório

### Interface
```csharp
public interface IAvisoRepository : IRepository<AvisoEntity>
{
    Task<List<AvisoEntity>> ObterTodosAvisosAsync(...);     // Lista ativos ordenados
    Task<AvisoEntity> ObterAvisoPorIdAsync(int id, ...);    // Busca ativo por ID
    Task<bool> ExisteAvisoAtivoAsync(int id, ...);          // Verifica existência
}
```

### Implementação
- Filtros automáticos para avisos ativos
- Suporte a NoTracking para queries read-only
- Ordenação por DataCriacao (mais recentes primeiro)

---

## 🚀 Como executar

1. **Clone o repositório**
```bash
git clone https://github.com/MiltonMPereira/BasicoDotNet.git
cd BasicoDotNet
```

2. **Execute a API**
```bash
dotnet restore
cd 1-Presentation/Bernhoeft.GRT.Teste.Api
dotnet run
```

A API estará disponível em: **https://localhost:5001**

3. **Execute os testes**
```bash
dotnet test
```

---

## 🔄 Passo a passo para testar

### 1. Criar aviso
```bash
curl -X POST "https://localhost:5001/api/v1/avisos" \
  -H "Content-Type: application/json" \
  -d '{
    "titulo": "Manutenção Programada",
    "mensagem": "Sistema offline das 02:00 às 04:00"
  }'
```

### 2. Listar avisos ativos
```bash
curl "https://localhost:5001/api/v1/avisos"
```

### 3. Buscar aviso por ID
```bash
curl "https://localhost:5001/api/v1/avisos/1"
```

### 4. Atualizar aviso 
```bash
curl -X PUT "https://localhost:5001/api/v1/avisos/1" \
  -H "Content-Type: application/json" \
  -d '{
    "mensagem": "Sistema offline das 01:00 às 03:00"
  }'
```

### 5. Remover aviso 
```bash
curl -X DELETE "https://localhost:5001/api/v1/avisos/1"
```

---

## 🧪 Testes Implementados

#### 🔬 Testes Unitários (50+ testes)
- **AvisoEntityTests** (10 testes) - Comportamento da entidade
- **ValidatorTests** (40+ testes) - Todos os cenários de validação
  - CreateAvisoRequestValidatorTests - 10 cenários
  - UpdateAvisoRequestValidatorTests - 8 cenários
  - DeleteAvisoRequestValidatorTests - 4 cenários
  - GetAvisoRequestValidatorTests - 4 cenários

#### 🔧 Testes de Contrato (25+ testes)
- **AvisosContractTests** - Todos os endpoints HTTP
- **ValidationContractTests** - Cenários de erro e validação

### Cobertura
- **Entidades**: 100% - Todas as regras de negócio testadas
- **Validadores**: 100% - Todos os cenários de validação
- **Endpoints**: 85% - Testados via contract tests
- **Cobertura total**: ~85%

### Padrões de Teste
```csharp
// Padrão AAA: Arrange, Act, Assert
[Fact]
public void Method_Scenario_ExpectedResult()
{
    // Arrange - Preparação
    var aviso = new AvisoEntity();
    
    // Act - Execução
    aviso.Desativar();
    
    // Assert - Verificação
    aviso.Ativo.Should().BeFalse();
    aviso.DataModificacao.Should().NotBeNull();
}
```

---

## 🏛️ Decisões Técnicas Específicas

#### Service Locator nos Handlers
```csharp
public class CreateAvisoHandler : IRequestHandler<...>
{
    private readonly IServiceProvider _serviceProvider;
    private IContext _context => _serviceProvider.GetRequiredService<IContext>();
    private IAvisoRepository _avisoRepository => _serviceProvider.GetRequiredService<IAvisoRepository>();
}
```
**Justificativa**: Evita construtores pesados, lazy loading de dependências

#### Duas Classes de Request para Update
```csharp
// Para uso interno (com ID)
public class UpdateAvisoRequest : IRequest<...>
{
    public int Id { get; set; }
    public string Mensagem { get; set; }
}

// Para binding do controller (sem ID)
public class UpdateAvisoRequestBody
{
    public string Mensagem { get; set; }
}
```
**Justificativa**: Separação de responsabilidades (ID da rota, mensagem do body)

---

## 📝 Regras de Negócio Detalhadas

### 1. Controle de Auditoria
- **DataCriacao**: Automaticamente preenchida na criação (UTC)
- **DataModificacao**: Preenchida apenas em atualizações (UTC)
- **Método DefinirDataModificacao()**: Garante consistência

### 2. Soft Delete
- **Campo Ativo**: Controla visibilidade dos registros
- **Método Desativar()**: Combina soft delete + auditoria
- **Filtros automáticos**: Repositório retorna apenas registros ativos

### 3. Validações de Entrada
- **Prevenção de IDs inválidos**: IDs ≤ 0 rejeitados
- **Campos obrigatórios**: Título e mensagem não podem ser vazios
- **Limites de tamanho**: 200 chars (título), 1000 chars (mensagem)

### 4. Restrições de Edição
- **Apenas mensagem editável**: Título preservado após criação
- **Validação específica**: Diferentes regras para create vs update
- **Auditoria automática**: Data atualizada em modificações
- 
---

## 📋 Sugestões para Futuras Melhorias

- [ ] Autenticação JWT
- [ ] Paginação na listagem
- [ ] Cache com Redis
- [ ] Logging estruturado
- [ ] Categorias de avisos
- [ ] Notificações push
- [ ] Métricas de observabilidade

---

## 🎯 Conclusão

Todos os Requisitos Atendidos.

**Diferenciais Implementados**:

- **Testes abrangentes** - 85%+ cobertura com múltiplos tipos
- **Documentação completa** - Decisões técnicas documentadas

O projeto pode ser considerado apto para ambiente de produção e pode ser expandido mantendo consistência arquitetural.

 
