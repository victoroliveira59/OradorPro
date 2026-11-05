# Agent Report — Especificação de Isolamento de Dados por Usuário e Congregação (Multi-Tenancy)

- **Agent:** ANALISTA
- **Phase:** 16 — Isolamento de Dados por Usuário e Congregação (Multi-Tenancy)
- **Task:** T16.01 — Especificar regras de isolamento multi-tenant, escopo de visibilidade e critérios de aceite
- **Status:** COMPLETED

---

## 1. Resumo Executivo da Funcionalidade

A plataforma **Agenda de Palestras** (OradorPro) opera em modelo multi-tenant onde múltiplos coordenadores e congregações gerenciam suas reuniões, oradores visitantes, catálogo de discursos e notificações via WhatsApp.

Conforme alinhamento e direcionamento estratégico do usuário:
> *"Não quero bloquear a parte de oradores e nem congregações a minha ideia é cada usuário gerenciar suas próprias informações com base na sua congregação. Ele vai gerenciar suas próprias inclusões."*

Em vez de restringir perfis por meio de papéis limitados (como "somente leitura"), a abordagem oficial é o **isolamento estrito de dados por congregação e usuário criador (Multi-Tenancy)**. 

### Pilares Fundamentais do Modelo:
1. **Discursos (Catálogo Oficial S-34):** Os 194 temas oficiais de discursos públicos permanecem públicos e compartilhados para todos os usuários consultarem e utilizarem no repertório dos oradores e agendamentos.
2. **Congregações:** Cada coordenador regular visualiza e gerencia exclusivamente a **sua própria congregação** de vínculo (`Usuario.CongregacaoId`) e as **congregações adicionais que ele mesmo cadastrou** (ex: congregações de oradores visitantes de fora). Ele **nunca** visualiza nem interfere nas congregações cadastradas por outros usuários.
3. **Oradores:** O usuário regular só visualiza, lista e gerencia os oradores pertencentes à sua congregação ou criados por ele. Oradores cadastrados por outros coordenadores em congregações alheias ficam invisíveis e inacessíveis.
4. **Agenda e Programação:** O usuário regular só cria, edita, visualiza e gerencia eventos e designações de reuniões da **sua própria congregação**.
5. **SuperAdmin:** Mantém visão e privilégio globais e irrestritos para auditar, visualizar e gerenciar congregações, oradores, agendas, discursos e instâncias de todos os usuários da plataforma.

---

## 2. Regras de Negócio Estritas (RN-16.01 a RN-16.05)

### RN-16.01 — Escopo e Visibilidade de Congregações
1. **Congregação Própria do Usuário:** Todo usuário coordenador (`User`/`Admin`) possui obrigatoriamente uma congregação principal associada (`Usuario.CongregacaoId`).
2. **Congregações Adicionais (Visitantes):** O coordenador tem permissão para cadastrar novas congregações no sistema (ex: ao cadastrar um orador visitante de outro circuito/cidade). No momento da criação, o sistema deve registrar compulsoriamente `Congregacao.UsuarioCriadorId = usuarioAutenticado.Id`.
3. **Filtro de Consulta (Read Scope):**
   - Para usuários regulares (`!ehSuperAdmin`), as consultas de congregações (na tela `/Congregacoes`, em seletores dropdown de Oradores, filtros de Agenda e Programação) devem retornar estritamente congregações onde:
     ```csharp
     c.Id == usuario.CongregacaoId || c.UsuarioCriadorId == usuario.Id
     ```
   - Congregações cujo `UsuarioCriadorId != usuario.Id` e cujo `Id != usuario.CongregacaoId` **nunca** devem ser retornadas.
4. **Proteção de Modificação (Write Scope / IDOR):**
   - Um usuário regular só pode editar (`/Congregacoes/Edit`) ou alternar status (`/Congregacoes/AlternarStatus`) de congregações pertencentes ao seu escopo visível.
   - Tentativas de acessar ou alterar congregação alheia devem retornar `404 Not Found` ou `403 Forbidden`.
5. **Unicidade de Nome de Congregação:**
   - A unicidade do nome da congregação no banco de dados deve considerar o escopo de visibilidade do usuário ou ser composta (`Nome + UsuarioCriadorId` ou `Nome + Cidade`), permitindo que dois usuários independentes cadastrem congregações com nomes homônimos em circuitos distintos sem colisão de constraint global.

### RN-16.02 — Escopo e Vínculo de Oradores (Locais e Visitantes)
1. **Rastreabilidade de Autoria:** A entidade `Orador` deve possuir a propriedade `UsuarioCriadorId` (Guid?, nullable). No cadastro de qualquer orador (`/Oradores/Create`), o sistema deve registrar compulsoriamente `Orador.UsuarioCriadorId = usuarioAutenticado.Id`.
2. **Filtro de Consulta de Oradores:**
   - Para usuários regulares, a listagem de oradores (`/Oradores/Index`) e o dropdown de oradores na criação de agenda (`/Agenda/Create`) devem retornar estritamente oradores que atendam a pelo menos uma das condições:
     a) O orador pertence à congregação do usuário (`o.CongregacaoId == usuario.CongregacaoId`);
     b) O orador foi criado pelo usuário (`o.UsuarioCriadorId == usuario.Id`);
     c) O orador pertence a uma congregação criada pelo usuário (`o.Congregacao.UsuarioCriadorId == usuario.Id`).
   - Oradores vinculados a congregações de outros usuários e criados por outros usuários **não são visíveis**.
3. **Gerenciamento de Repertório e Atualização:**
   - Apenas o usuário que tem visibilidade do orador pode atualizar seus dados (`/Oradores/Edit`), gerenciar repertório de discursos (`/Oradores/Repertorio`) ou alternar seu status.
   - Requisições diretas a IDs de oradores fora do escopo devem ser rejeitadas com `404 Not Found` ou `403 Forbidden`.

### RN-16.03 — Isolamento da Agenda e Programação de Fim de Semana
1. **Vínculo Exclusivo à Congregação Anfitriã:** A agenda de palestras e a programação de reuniões de fim de semana são restritas à congregação do coordenador autenticado.
2. **Visualização do Calendário e Grade:**
   - Na página `/Agenda` (modo calendário e modo lista) e na página `/Programacao`, o parâmetro de congregação para usuários regulares é travado estritamente em `usuario.CongregacaoId`.
   - Usuários regulares não podem visualizar nem alternar o filtro para congregações de outros usuários.
3. **Criação e Operações da Agenda:**
   - Ao agendar uma palestra (`POST /Agenda/Create`), o campo `CongregacaoId` é sempre forçado pelo backend para `usuario.CongregacaoId`.
   - Detalhes (`/Agenda/Details/{id}`), Reagendamento (`/Agenda/Edit/{id}`), Confirmação Manual (`/Agenda/Confirmar/{id}`), Cancelamento (`/Agenda/Cancelar/{id}`) e Reenvio de Convite WhatsApp (`/Agenda/ReenviarConvite/{id}`) só podem ser operados se `agenda.CongregacaoId == usuario.CongregacaoId`.
4. **Programação e Exportação de PDF:**
   - O salvamento de designações semanais (`POST /Programacao/Salvar`) e a exportação em PDF (`GET /Programacao/ExportarPdf`) são restritos exclusivamente às reuniões da congregação do usuário (`usuario.CongregacaoId`).

### RN-16.04 — Catálogo Global Compartilhado de Discursos Públicos (S-34)
1. **Catálogo Público de Consulta:** O catálogo oficial de 194 temas de discursos (S-34) permanece global e compartilhado. Todos os usuários autenticados têm acesso de leitura irrestrito a todos os temas ativos em `/Discursos`, na seleção de repertório de oradores e no agendamento de reuniões.
2. **Proteção do Catálogo Oficial:** Usuários regulares não podem alterar temas ou desativar discursos do catálogo oficial. Apenas o perfil `SuperAdmin` pode criar ou editar registros em `Discursos`.

### RN-16.05 — Privilégios Globais do SuperAdmin e Prevenção contra IDOR
1. **Visão Global do SuperAdmin:** Usuários com `Role == "SuperAdmin"` não são restritos por filtros de congregação ou criador. Eles visualizam todas as congregações, todos os oradores, agendas de qualquer congregação e todas as instâncias da Evolution API.
2. **Prevenção contra Broken Access Control (OWASP Top 10 - A01:2021 / IDOR):**
   - Validações de propriedade e visibilidade devem ocorrer **obrigatoriamente na camada de negócio / repositórios (backend)**.
   - Manipulação de parâmetros de rota (ex: forjar `/Oradores/Edit/GUID-ALHEIO` ou `/Agenda/Details/GUID-ALHEIO`) ou payloads POST não autorizados devem ser interceptados e bloqueados independentemente do que é renderizado na View.

---

## 3. Mapeamento Técnico de Filtros e Relacionamentos

### 3.1 Alterações no Modelo de Entidades e Banco de Dados

1. **Entidade `Orador` (`AgendaPalestras.Domain.Entities.Orador`):**
   - Adicionar propriedade:
     ```csharp
     public Guid? UsuarioCriadorId { get; private set; }
     ```
   - Atualizar construtores e método de criação para aceitar `usuarioCriadorId`.
   - Mapeamento Fluent API em `OradorConfiguration`:
     ```csharp
     builder.Property(o => o.UsuarioCriadorId).IsRequired(false);
     builder.HasIndex(o => o.UsuarioCriadorId);
     ```

2. **Entidade `Congregacao` (`AgendaPalestras.Domain.Entities.Congregacao`):**
   - Propriedade já existente: `public Guid? UsuarioCriadorId { get; private set; }`.
   - Mapeamento Fluent API em `CongregacaoConfiguration`:
     - Garantir que o índice em `Nome` suporte escopo por usuário ou remover a constraint única estrita caso gere colisão entre congregações homônimas de diferentes usuários.
     - Mapear `UsuarioCriadorId` explicitamente:
     ```csharp
     builder.Property(c => c.UsuarioCriadorId).IsRequired(false);
     builder.HasIndex(c => c.UsuarioCriadorId);
     ```

3. **Migration no EF Core:**
   - Criar migration `AddUsuarioCriadorIdEmOradorEMapeamentosMultiTenant`.

### 3.2 Lógica LINQ de Filtros por Módulo nos Repositórios

#### A. Repositório de Congregações (`CongregacaoRepository`)
```csharp
public async Task<List<Congregacao>> ListarPorUsuarioAsync(
    Guid usuarioId, 
    Guid? congregacaoId, 
    bool ehSuperAdmin, 
    bool apenasAtivas = false, 
    CancellationToken cancellationToken = default)
{
    var query = _context.Congregacoes
        .Include(c => c.Oradores)
        .AsNoTracking();

    if (!ehSuperAdmin)
    {
        query = query.Where(c => 
            (congregacaoId.HasValue && c.Id == congregacaoId.Value) || 
            c.UsuarioCriadorId == usuarioId);
    }

    if (apenasAtivas)
        query = query.Where(c => c.Ativo);

    return await query.OrderBy(c => c.Nome).ToListAsync(cancellationToken);
}
```

#### B. Repositório de Oradores (`OradorRepository`)
```csharp
public async Task<List<Orador>> ListarPorUsuarioAsync(
    Guid usuarioId, 
    Guid? congregacaoId, 
    bool ehSuperAdmin, 
    Guid? filtroCongregacaoId = null, 
    bool apenasAtivos = false, 
    CancellationToken cancellationToken = default)
{
    var query = _context.Oradores
        .Include(o => o.Congregacao)
        .Include(o => o.OradorDiscursos)
        .ThenInclude(od => od.Discurso)
        .AsNoTracking();

    if (!ehSuperAdmin)
    {
        query = query.Where(o => 
            (congregacaoId.HasValue && o.CongregacaoId == congregacaoId.Value) ||
            o.UsuarioCriadorId == usuarioId ||
            (o.Congregacao != null && o.Congregacao.UsuarioCriadorId == usuarioId));
    }

    if (filtroCongregacaoId.HasValue)
        query = query.Where(o => o.CongregacaoId == filtroCongregacaoId.Value);

    if (apenasAtivos)
        query = query.Where(o => o.Ativo);

    return await query.OrderBy(o => o.Nome).ToListAsync(cancellationToken);
}
```

#### C. Repositório de Agenda (`AgendaRepository`)
```csharp
public async Task<IReadOnlyList<Agenda>> ObterPorPeriodoAsync(
    DateTime inicio,
    DateTime fim,
    Guid? congregacaoId = null,
    CancellationToken cancellationToken = default)
{
    var inicioUtc = ToUtc(inicio);
    var fimUtc = ToUtc(fim);

    var query = _context.Agendas
        .Include(a => a.Congregacao)
        .Include(a => a.Orador)
        .Include(a => a.Discurso)
        .Where(a => a.DataHora >= inicioUtc && a.DataHora <= fimUtc);

    if (congregacaoId.HasValue)
    {
        query = query.Where(a => a.CongregacaoId == congregacaoId.Value);
    }

    return await query.OrderBy(a => a.DataHora).ToListAsync(cancellationToken);
}
```

---

## 4. Critérios de Aceite em Formato Gherkin (BDD)

### Épico 1: Isolamento de Congregações

#### Cenário 1.1: Coordenador lista apenas sua congregação e congregações cadastradas por ele
- **Dado** que o Usuário A está autenticado e pertence à "Congregação Alvorada" (`Usuario.CongregacaoId`)
- **E** o Usuário A cadastrou a "Congregação Visitante Betel" (`UsuarioCriadorId = A.Id`)
- **E** outro Usuário B cadastrou a "Congregação Primavera" (`UsuarioCriadorId = B.Id`)
- **Quando** o Usuário A acessa a listagem de congregações (`/Congregacoes`)
- **Então** o sistema exibe "Congregação Alvorada" e "Congregação Visitante Betel"
- **E** o sistema **não** exibe "Congregação Primavera"

#### Cenário 1.2: Cadastro de nova congregação associa automaticamente o usuário criador
- **Dado** que o Usuário A está autenticado
- **Quando** submete o cadastro de uma nova congregação "Congregação Central"
- **Então** o registro é salvo no banco com `UsuarioCriadorId = A.Id`
- **E** a nova congregação passa a ser visível na lista do Usuário A

#### Cenário 1.3: Bloqueio de edição de congregação pertencente a outro usuário (Proteção IDOR)
- **Dado** que o Usuário A está autenticado
- **E** a "Congregação Primavera" pertence ao Usuário B (`UsuarioCriadorId = B.Id`)
- **Quando** o Usuário A tenta acessar via URL `GET /Congregacoes/Edit/{id-primavera}` ou enviar `POST /Congregacoes/Edit`
- **Então** o backend rejeita a operação com código `404 Not Found` ou `403 Forbidden`
- **E** nenhum dado da congregação é alterado

---

### Épico 2: Isolamento de Oradores

#### Cenário 2.1: Coordenador visualiza apenas oradores locais e oradores criados por ele
- **Dado** que o Usuário A pertence à congregação "Alvorada"
- **E** existem os oradores:
  - "Irmão Silva" (Congregação Alvorada)
  - "Irmão Costa" (Congregação Visitante Betel, cadastrado pelo Usuário A)
  - "Irmão Pereira" (Congregação Primavera, cadastrado pelo Usuário B)
- **Quando** o Usuário A acessa a listagem de oradores (`/Oradores`)
- **Então** a lista exibe "Irmão Silva" e "Irmão Costa"
- **E** a lista **não** exibe "Irmão Pereira"

#### Cenário 2.2: Dropdown de congregações ao cadastrar orador exibe apenas congregações permitidas
- **Dado** que o Usuário A está autenticado
- **Quando** acessa a tela de cadastro de orador (`/Oradores/Create`)
- **Então** o dropdown de congregações de origem lista apenas a congregação do Usuário A e as congregações adicionais cadastradas por ele

#### Cenário 2.3: Bloqueio de acesso e edição a orador de outro usuário
- **Dado** que o Usuário A está autenticado
- **E** o "Irmão Pereira" pertence ao Usuário B
- **Quando** o Usuário A tenta acessar `GET /Oradores/Edit/{id-pereira}` ou `GET /Oradores/Repertorio/{id-pereira}`
- **Então** o sistema bloqueia o acesso com `404 Not Found` ou `403 Forbidden`

---

### Épico 3: Isolamento de Agenda e Programação

#### Cenário 3.1: Coordenador visualiza e agenda palestras apenas para sua própria congregação
- **Dado** que o Usuário A pertence à "Congregação Alvorada"
- **Quando** acessa a Agenda mensal (`/Agenda`)
- **Então** o calendário exibe apenas palestras agendadas para a "Congregação Alvorada"
- **E** o usuário não tem opção de alternar a visualização para outras congregações
- **Quando** cria um novo agendamento (`/Agenda/Create`)
- **Então** a congregação anfitriã é compulsoriamente vinculada à "Congregação Alvorada"

#### Cenário 3.2: Tentativa de manipulação de evento de agenda de outra congregação
- **Dado** que existe uma palestra agendada para a congregação do Usuário B (`Agenda.CongregacaoId = B.CongregacaoId`)
- **Quando** o Usuário A tenta acessar os detalhes (`/Agenda/Details/{id-b}`) ou cancelar (`/Agenda/Cancelar/{id-b}`)
- **Então** o backend bloqueia a ação com `404 Not Found` ou `403 Forbidden`

#### Cenário 3.3: Programação de Fim de Semana e PDF restritos à congregação do usuário
- **Dado** que o Usuário A pertence à "Congregação Alvorada"
- **Quando** acessa a Programação (`/Programacao`) ou exporta o PDF (`/Programacao/ExportarPdf`)
- **Então** os dados gerados referem-se estritamente às reuniões e designações da "Congregação Alvorada"

---

### Épico 4: Catálogo Oficial de Discursos Compartilhado (S-34)

#### Cenário 4.1: Todos os coordenadores consultam o catálogo oficial completo
- **Dado** que o Usuário A está autenticado
- **Quando** acessa a tela de Discursos (`/Discursos`) ou seleciona temas no repertório de oradores
- **Então** todos os 194 temas oficiais do catálogo S-34 estão disponíveis para consulta e associação

---

### Épico 5: SuperAdmin com Acesso Global

#### Cenário 5.1: SuperAdmin visualiza todas as congregações, oradores e agendas
- **Dado** que estou autenticado com a role `SuperAdmin`
- **Quando** acesso a listagem de congregações, oradores ou agenda
- **Então** visualizo os registros de todos os usuários e congregações do sistema
- **E** posso filtrar a agenda e a programação por qualquer congregação cadastrada na plataforma

---

## 5. Guia de Implementação para o Engenheiro (T16.02)

Para a execução da tarefa técnica **T16.02**, recomenda-se a seguinte ordem de passos:

### 1. Camada de Domínio (`AgendaPalestras.Domain`)
- Em `Orador.cs`:
  - Adicionar propriedade `public Guid? UsuarioCriadorId { get; private set; }`.
  - Atualizar construtor e método `Atualizar` para aceitar `usuarioCriadorId`.
- Em `Congregacao.cs`:
  - Confirmar integridade do campo `UsuarioCriadorId` e construtores.

### 2. Camada de Infraestrutura (`AgendaPalestras.Infrastructure`)
- Em `OradorConfiguration.cs`: mapear `UsuarioCriadorId`.
- Em `CongregacaoConfiguration.cs`: revisar constraint de `Nome` (avaliar se o índice único em `Nome` deve ser flexibilizado para não bloquear congregações homônimas de outros usuários).
- Criar e aplicar migration EF Core:
  ```powershell
  dotnet ef migrations add AddMultiTenantIsolationFase16 -s src/AgendaPalestras.Web -p src/AgendaPalestras.Infrastructure
  ```
- Atualizar `CongregacaoRepository`:
  - Implementar método com filtro de usuário/congregação: `ListarPorUsuarioAsync(Guid usuarioId, Guid? congregacaoId, bool ehSuperAdmin, bool apenasAtivas)`.
- Atualizar `OradorRepository`:
  - Implementar método com filtro de usuário/congregação: `ListarPorUsuarioAsync(Guid usuarioId, Guid? congregacaoId, bool ehSuperAdmin, Guid? filtroCongregacaoId, bool apenasAtivos)`.
- Atualizar `AgendaRepository`:
  - Assegurar que `ObterPorPeriodoAsync` e verificações de conflito respeitem o `congregacaoId` passado.

### 3. Camada de Aplicação (`AgendaPalestras.Application`)
- Atualizar interfaces e serviços `ICongregacaoService` e `IOradorService` para receber `usuarioId`, `congregacaoId` e `ehSuperAdmin` (ou via claims/contexto).
- Atualizar `OradorService.CriarAsync` para registrar `usuarioCriadorId`.
- Em `CongregacaoService.CriarAsync`, garantir que `usuarioCriadorId` seja salvo.
- Adicionar validações de autorização (ownership check) em `ObterPorIdAsync`, `AtualizarAsync` e exclusões/alterações de status para garantir que usuários não acessem registros fora de seu escopo.

### 4. Camada Web (`AgendaPalestras.Web`)
- Em `CongregacoesController`:
  - Obter `usuarioId` (`ClaimTypes.NameIdentifier`), `congregacaoId` (`CongregacaoId`) e `ehSuperAdmin` (`User.IsInRole("SuperAdmin")`).
  - Filtrar listagem no `Index` chamando o serviço com o escopo do usuário.
  - No `Create`, repassar `usuarioId` como criador.
  - No `Edit` e `AlternarStatus`, validar se o ID solicitado está dentro do escopo do usuário; retornar `NotFound()` ou `Forbid()` caso contrário.
- Em `OradoresController`:
  - Filtrar listagens de oradores e congregações no `Index`, `Create` e `Edit` respeitando o escopo do usuário.
  - No `Create`, passar `usuarioId` do criador.
  - Proteger ações de `Edit`, `Repertorio` e `AlternarStatus` contra acesso a oradores alheios.
- Em `AgendaController`:
  - Fixar o filtro de congregação para `usuario.CongregacaoId` quando não for `SuperAdmin`.
  - Ocultar/desabilitar dropdown de congregação para coordenadores comuns.
  - No `Create`, forçar `dto.CongregacaoId = userCongId.Value`.
  - Proteger `Details`, `Edit`, `Confirmar`, `Cancelar` contra IDs de palestras de outras congregações.
- Em `ProgramacaoController`:
  - Fixar a congregação em `userCongId` para usuários regulares no `Index`, `Salvar` e `ExportarPdf`.

---

## 6. Arquivos Consultados
- `SPEC.md`
- `.ai/tasks.md`
- `.ai/workflow-state.md`
- `docs/product/requirements.md`
- `docs/product/business-rules.md`
- `docs/product/acceptance-criteria.md`
- `src/AgendaPalestras.Domain/Entities/Usuario.cs`
- `src/AgendaPalestras.Domain/Entities/Congregacao.cs`
- `src/AgendaPalestras.Domain/Entities/Orador.cs`
- `src/AgendaPalestras.Domain/Entities/Agenda.cs`
- `src/AgendaPalestras.Domain/Entities/Discurso.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Configurations/CongregacaoConfiguration.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Configurations/OradorConfiguration.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Configurations/AgendaConfiguration.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Repositories/CongregacaoRepository.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Repositories/OradorRepository.cs`
- `src/AgendaPalestras.Infrastructure/Persistence/Repositories/AgendaRepository.cs`
- `src/AgendaPalestras.Application/Services/CongregacaoService.cs`
- `src/AgendaPalestras.Application/Services/OradorService.cs`
- `src/AgendaPalestras.Application/Services/AgendaService.cs`
- `src/AgendaPalestras.Web/Controllers/CongregacoesController.cs`
- `src/AgendaPalestras.Web/Controllers/OradoresController.cs`
- `src/AgendaPalestras.Web/Controllers/AgendaController.cs`
- `src/AgendaPalestras.Web/Controllers/ProgramacaoController.cs`
- `src/AgendaPalestras.Web/Controllers/DiscursosController.cs`

## 7. Arquivos Criados ou Alterados
- `docs/product/requirements.md` (atualizado com RF-13)
- `docs/product/business-rules.md` (atualizado com RN-14 a RN-18)
- `docs/product/acceptance-criteria.md` (atualizado com CA-ISO-01 a CA-ISO-06)
- `docs/agent-runs/fase-16/t16-01/analyst-report.md` (criado e formalizado com a especificação completa de multi-tenancy)

## 8. Riscos e Pendências
- **Nenhum risco impeditivo.**
- As especificações atendem 100% ao novo direcionamento do usuário de manter oradores e congregações abertos para inclusão autônoma, isolando os dados de cada usuário/congregação.
- Não existem itens pendentes em `docs/product/open-questions.md`.

## 9. Próximo Agente Recomendado
- **Engenheiro:** Para implementar os filtros de isolamento multi-tenant por usuário/congregação em `T16.02`.
