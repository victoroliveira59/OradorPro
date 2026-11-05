# Agent Report — Implementação de Isolamento por Usuário e Congregação (Multi-Tenancy)

- **Agent:** ENGENHEIRO
- **Phase:** 16 — Isolamento de Dados por Usuário e Congregação (Multi-Tenancy)
- **Task:** T16.02 — Implementar filtros de isolamento por usuário/congregação em Congregações, Oradores e Agenda
- **Status:** COMPLETED (WAITING_REVIEW)

---

## 1. Resumo Executivo da Entrega

Em conformidade com a especificação do Analista (`docs/agent-runs/fase-16/t16-01/analyst-report.md`), a diretriz estratégica do usuário e as regras de segurança contra IDOR / Broken Access Control (OWASP Top 10), foi implementada a arquitetura completa de isolamento por usuário/congregação (Multi-Tenancy) sem bloqueio artificial de inclusão.

Cada coordenador regular gerencia suas próprias congregações cadastradas, sua congregação principal de vínculo e os oradores pertencentes ao seu escopo. O catálogo oficial de 194 discursos (S-34) permanece global e compartilhado. O perfil `SuperAdmin` retém visão global e irrestrita.

---

## 2. Modificações Realizadas por Camada

### 2.1 Camada de Domínio (`AgendaPalestras.Domain`)
- **`Orador.cs`**:
  - Propriedade `public Guid? UsuarioCriadorId { get; private set; }` e construtor com suporte a `usuarioCriadorId`.
- **`ICongregacaoRepository.cs`**:
  - Adicionado método `ListarPorUsuarioAsync(Guid usuarioId, Guid? congregacaoId, bool ehSuperAdmin, bool apenasAtivas = false, CancellationToken cancellationToken = default)`.
- **`IOradorRepository.cs`**:
  - Adicionado método `ListarPorUsuarioAsync(Guid usuarioId, Guid? congregacaoId, bool ehSuperAdmin, Guid? filtroCongregacaoId = null, bool apenasAtivos = false, CancellationToken cancellationToken = default)`.

### 2.2 Camada de Infraestrutura (`AgendaPalestras.Infrastructure`)
- **`OradorConfiguration.cs`**:
  - Mapeamento explícito de `UsuarioCriadorId` (nullable) e criação de índice `IX_Oradores_UsuarioCriadorId`.
- **`CongregacaoConfiguration.cs`**:
  - Mapeamento explícito de `UsuarioCriadorId` (nullable) e índice `IX_Congregacoes_UsuarioCriadorId`.
  - Flexibilização do índice em `Nome` (de único estrito global para índice não exclusivo), viabilizando homônimos entre diferentes circuitos/usuários.
- **Migration EF Core**:
  - Gerada a migration `20261105113338_AddUsuarioCriadorIdEmOradorEMapeamentosMultiTenant.cs` via `dotnet ef migrations add`.
- **`CongregacaoRepository.cs`**:
  - Implementação de `ListarPorUsuarioAsync`: se `!ehSuperAdmin`, filtra por `(congregacaoId.HasValue && c.Id == congregacaoId.Value) || c.UsuarioCriadorId == usuarioId`.
- **`OradorRepository.cs`**:
  - Implementação de `ListarPorUsuarioAsync`: se `!ehSuperAdmin`, filtra por `(congregacaoId.HasValue && o.CongregacaoId == congregacaoId.Value) || o.UsuarioCriadorId == usuarioId || (o.Congregacao != null && o.Congregacao.UsuarioCriadorId == usuarioId)`.

### 2.3 Camada de Aplicação (`AgendaPalestras.Application`)
- **`CongregacaoDtos.cs` & `OradorDtos.cs`**:
  - Incluído `UsuarioCriadorId` em `CongregacaoDto`, `OradorDto` e `CriarOradorDto`.
- **`CongregacaoService.cs`**:
  - Adicionado `ListarPorUsuarioAsync` repassando ao repositório.
  - Atualizado `MapToDto` com `UsuarioCriadorId`.
- **`OradorService.cs`**:
  - Adicionado `ListarPorUsuarioAsync` repassando ao repositório.
  - No método `CriarAsync`, propaga `dto.UsuarioCriadorId` para a entidade `Orador` e para a `Congregacao` criada sob demanda (caso use congregação nova em orador visitante).
  - Atualizado `MapToDto` com `UsuarioCriadorId`.

### 2.4 Camada Web MVC (`AgendaPalestras.Web`)
- **`CongregacoesController.cs`**:
  - `Index`: Chama `ListarPorUsuarioAsync(usuarioId, userCongId, ehSuperAdmin)`.
  - `Create (POST)`: Injeta `usuarioId` logado como criador.
  - `Edit` (GET/POST) e `AlternarStatus`: Validação de autorização com `PodeGerenciarCongregacao` (SuperAdmin ou pertencente ao seu escopo/criador), retornando `Forbid()` em violações IDOR.
- **`OradoresController.cs`**:
  - `Index`: Lista oradores e congregações filtrados estritamente pelo escopo do usuário.
  - `Create` (GET/POST): Filtra congregações visíveis e injeta `UsuarioCriadorId` na criação do orador.
  - `Edit` (GET/POST), `Repertorio` e `AlternarStatus`: Protegidos contra adulteração de IDs (IDOR) via `PodeGerenciarOradorAsync`.
- **`AgendaController.cs`**:
  - `Index`: Para usuários comuns (`!ehSuperAdmin`), força o filtro de congregação para `usuario.CongregacaoId` e popula dropdowns apenas com congregações visíveis.
  - `Create` (POST): Força `CongregacaoId = userCongId` no backend para evitar que coordenadores agendem para congregações de outros.
  - `Edit`, `Details`, `Confirmar`, `ReenviarConvite`, `Cancelar`: Validação estrita `PodeAcessarAgenda` conferindo `agenda.CongregacaoId == userCongId`.
  - `CarregarCombosAsync`: Carrega congregações e oradores via `ListarPorUsuarioAsync`.
- **`ProgramacaoController.cs`**:
  - `Index`: Trava `congregacaoId` na congregação do usuário comum.
  - `Salvar`: Valida congregação para impedir manipulação de designações alheias.
  - `ExportarPdf`: Trava `congregacaoId` e exporta dados exclusivos da congregação do coordenador.

---

## 3. Testes Unitários e Evidências

Criado o arquivo de testes dedicado: `tests/AgendaPalestras.UnitTests/MultiTenantIsolationTests.cs` cobrindo:
1. Atribuição de `UsuarioCriadorId` na entidade `Orador`.
2. Atribuição de `UsuarioCriadorId` na entidade `Congregacao`.
3. Propagação de `UsuarioCriadorId` em `OradorService.CriarAsync` para orador e congregação criada automaticamente.
4. Repasse de parâmetros de isolamento em `CongregacaoService.ListarPorUsuarioAsync`.
5. Repasse de parâmetros de isolamento em `OradorService.ListarPorUsuarioAsync`.
6. Validação do predicado de isolamento de congregações (ocultação de congregações alheias para usuários regulares).
7. Validação do predicado de isolamento de oradores (ocultação de oradores externos para usuários regulares).

### Resultado dos Comandos Locais:
- **`dotnet build AgendaPalestras.sln -c Release`**:
  - Status: Sucesso (0 erros).
- **`dotnet test AgendaPalestras.sln -c Release`**:
  - UnitTests: 96 aprovados (100%).
  - ArchitectureTests: 4 aprovados (100%).
  - IntegrationTests: 4 aprovados (100%).
  - **Total:** 104 testes aprovados, 0 falhas, 0 ignorados.

---

## 4. Arquivos Modificados e Criados
- `src/AgendaPalestras.Domain/Entities/Orador.cs`
- `src/AgendaPalestras.Domain/Interfaces/ICongregacaoRepository.cs`
- `src/AgendaPalestras.Domain/Interfaces/IOradorRepository.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Configurations/OradorConfiguration.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Configurations/CongregacaoConfiguration.cs`
- `src/AgendaPalestras.Infrastructure/Migrations/20261105113338_AddUsuarioCriadorIdEmOradorEMapeamentosMultiTenant.cs`
- `src/AgendaPalestras.Infrastructure/Migrations/20261105113338_AddUsuarioCriadorIdEmOradorEMapeamentosMultiTenant.Designer.cs`
- `src/AgendaPalestras.Infrastructure/Migrations/ApplicationDbContextModelSnapshot.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Repositories/CongregacaoRepository.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Repositories/OradorRepository.cs`
- `src/AgendaPalestras.Application/DTOs/CongregacaoDtos.cs`
- `src/AgendaPalestras.Application/DTOs/OradorDtos.cs`
- `src/AgendaPalestras.Application/Services/CongregacaoService.cs`
- `src/AgendaPalestras.Application/Services/OradorService.cs`
- `src/AgendaPalestras.Web/Controllers/CongregacoesController.cs`
- `src/AgendaPalestras.Web/Controllers/OradoresController.cs`
- `src/AgendaPalestras.Web/Controllers/AgendaController.cs`
- `src/AgendaPalestras.Web/Controllers/ProgramacaoController.cs`
- `tests/AgendaPalestras.UnitTests/MultiTenantIsolationTests.cs`
- `docs/agent-runs/fase-16/t16-02/implementation-report.md`

---

## 5. Próximo Passo
Submeter ao **Maestro** para avanço do fluxo para validação com o **Tester** (`TESTING`).
