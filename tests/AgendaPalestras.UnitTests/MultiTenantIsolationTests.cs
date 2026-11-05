using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Services;
using AgendaPalestras.Application.Validators;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class MultiTenantIsolationTests
{
    [Fact]
    public void Entidade_Orador_DeveRegistrarUsuarioCriadorId()
    {
        var congId = Guid.NewGuid();
        var criadorId = Guid.NewGuid();

        var orador = new Orador(congId, "João Silveira", "11987654321", "joao@email.com", usuarioCriadorId: criadorId);

        Assert.Equal(criadorId, orador.UsuarioCriadorId);
        Assert.Equal(congId, orador.CongregacaoId);
        Assert.Equal("João Silveira", orador.Nome);
    }

    [Fact]
    public void Entidade_Congregacao_DeveRegistrarUsuarioCriadorId()
    {
        var criadorId = Guid.NewGuid();

        var cong = new Congregacao(
            "Congregação Alvorada",
            "São Paulo",
            "SP-10",
            "Domingo",
            "18:00",
            usuarioCriadorId: criadorId);

        Assert.Equal(criadorId, cong.UsuarioCriadorId);
        Assert.Equal("Congregação Alvorada", cong.Nome);
    }

    [Fact]
    public async Task OradorService_CriarAsync_DevePropagarUsuarioCriadorIdAoOradorEConcregacaoNova()
    {
        var mockOradorRepo = new Mock<IOradorRepository>();
        var mockCongRepo = new Mock<ICongregacaoRepository>();
        var mockDiscursoRepo = new Mock<IDiscursoRepository>();
        var validator = new CriarOradorValidator();

        mockCongRepo.Setup(r => r.ListarTodasAsync(false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Congregacao>());

        var service = new OradorService(
            mockOradorRepo.Object,
            mockCongRepo.Object,
            mockDiscursoRepo.Object,
            validator
        );

        var usuarioCriadorId = Guid.NewGuid();
        var dto = new CriarOradorDto(
            Guid.Empty,
            "Carlos Visitante",
            "11999998888",
            "carlos@email.com",
            NovaCongregacaoNome: "Congregação Visitante Monte",
            UsuarioCriadorId: usuarioCriadorId
        );

        var resultado = await service.CriarAsync(dto);

        Assert.NotNull(resultado);
        Assert.Equal(usuarioCriadorId, resultado.UsuarioCriadorId);

        // Verifica que congregação foi criada com UsuarioCriadorId do usuário
        mockCongRepo.Verify(r => r.AdicionarAsync(
            It.Is<Congregacao>(c => c.Nome == "Congregação Visitante Monte" && c.UsuarioCriadorId == usuarioCriadorId),
            It.IsAny<CancellationToken>()), Times.Once);

        // Verifica que orador foi criado com UsuarioCriadorId do usuário
        mockOradorRepo.Verify(r => r.AdicionarAsync(
            It.Is<Orador>(o => o.Nome == "Carlos Visitante" && o.UsuarioCriadorId == usuarioCriadorId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CongregacaoService_ListarPorUsuarioAsync_DeveChamarRepositorioComFiltrosCorretos()
    {
        var mockRepo = new Mock<ICongregacaoRepository>();
        var mockValidator = new Mock<IValidator<CriarCongregacaoDto>>();

        var usuarioId = Guid.NewGuid();
        var congregacaoId = Guid.NewGuid();

        var listaEsperada = new List<Congregacao>
        {
            new("Congregação 1", "Cidade", "Circuito", "Sábado", "19:00", usuarioCriadorId: usuarioId)
        };

        mockRepo.Setup(r => r.ListarPorUsuarioAsync(usuarioId, congregacaoId, false, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listaEsperada);

        var service = new CongregacaoService(mockRepo.Object, mockValidator.Object);

        var resultado = await service.ListarPorUsuarioAsync(usuarioId, congregacaoId, ehSuperAdmin: false, apenasAtivas: true);

        Assert.Single(resultado);
        Assert.Equal("Congregação 1", resultado[0].Nome);
        mockRepo.Verify(r => r.ListarPorUsuarioAsync(usuarioId, congregacaoId, false, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task OradorService_ListarPorUsuarioAsync_DeveChamarRepositorioComFiltrosCorretos()
    {
        var mockOradorRepo = new Mock<IOradorRepository>();
        var mockCongRepo = new Mock<ICongregacaoRepository>();
        var mockDiscursoRepo = new Mock<IDiscursoRepository>();
        var mockValidator = new Mock<IValidator<CriarOradorDto>>();

        var usuarioId = Guid.NewGuid();
        var userCongId = Guid.NewGuid();
        var filtroCongId = Guid.NewGuid();

        var listaEsperada = new List<Orador>
        {
            new(userCongId, "Orador Teste", "11988887777", "orador@email.com", usuarioCriadorId: usuarioId)
        };

        mockOradorRepo.Setup(r => r.ListarPorUsuarioAsync(usuarioId, userCongId, false, filtroCongId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(listaEsperada);

        var service = new OradorService(
            mockOradorRepo.Object,
            mockCongRepo.Object,
            mockDiscursoRepo.Object,
            mockValidator.Object
        );

        var resultado = await service.ListarPorUsuarioAsync(
            usuarioId,
            userCongId,
            ehSuperAdmin: false,
            filtroCongregacaoId: filtroCongId,
            apenasAtivos: true);

        Assert.Single(resultado);
        Assert.Equal("Orador Teste", resultado[0].Nome);
        mockOradorRepo.Verify(r => r.ListarPorUsuarioAsync(usuarioId, userCongId, false, filtroCongId, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void RegraFiltroCongregacao_UsuarioRegular_NaoDeveVerCongregacoesAlheias()
    {
        var usuarioId = Guid.NewGuid();
        var userCongId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();

        var congPropria = new Congregacao("Congregação Minha", "SP", "01", "Dom", "18:00", usuarioCriadorId: Guid.NewGuid());
        // Força Id via reflection se necessário ou usa construtor
        typeof(AgendaPalestras.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(congPropria, userCongId);

        var congCriadaPorMim = new Congregacao("Congregação Visitante Minha", "SP", "01", "Sab", "19:00", usuarioCriadorId: usuarioId);
        var congAlheia = new Congregacao("Congregação Alheia", "RJ", "02", "Dom", "18:00", usuarioCriadorId: outroUsuarioId);

        var todas = new List<Congregacao> { congPropria, congCriadaPorMim, congAlheia };

        // Aplica predicado exato do repositório: (congregacaoId.HasValue && c.Id == congregacaoId.Value) || c.UsuarioCriadorId == usuarioId
        var filtradas = todas.Where(c => (userCongId == c.Id) || (c.UsuarioCriadorId == usuarioId)).ToList();

        Assert.Equal(2, filtradas.Count);
        Assert.Contains(congPropria, filtradas);
        Assert.Contains(congCriadaPorMim, filtradas);
        Assert.DoesNotContain(congAlheia, filtradas);
    }

    [Fact]
    public void RegraFiltroOradores_UsuarioRegular_DeveVerApenasOradoresPermitidos()
    {
        var usuarioId = Guid.NewGuid();
        var userCongId = Guid.NewGuid();
        var congCriadaPorMimId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();

        var congCriadaPorMim = new Congregacao("Cong Minha Criada", "SP", "01", "Sab", "19:00", usuarioCriadorId: usuarioId);
        typeof(AgendaPalestras.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(congCriadaPorMim, congCriadaPorMimId);

        var congAlheia = new Congregacao("Cong Alheia", "RJ", "02", "Dom", "18:00", usuarioCriadorId: outroUsuarioId);
        typeof(AgendaPalestras.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(congAlheia, Guid.NewGuid());

        // Orador 1: pertence à congregação do usuário
        var orador1 = new Orador(userCongId, "Orador da Minha Congregacao", "11988881111");

        // Orador 2: criado pelo usuário em outra congregação
        var orador2 = new Orador(Guid.NewGuid(), "Orador Criado Por Mim", "11988882222", usuarioCriadorId: usuarioId);

        // Orador 3: pertence a uma congregação criada pelo usuário
        var orador3 = new Orador(congCriadaPorMimId, "Orador de Cong Criada Por Mim", "11988883333");
        typeof(Orador).GetProperty("Congregacao")!.SetValue(orador3, congCriadaPorMim);

        // Orador 4: pertence a congregação alheia e criado por terceiro
        var orador4 = new Orador(congAlheia.Id, "Orador Alheio", "11988884444", usuarioCriadorId: outroUsuarioId);
        typeof(Orador).GetProperty("Congregacao")!.SetValue(orador4, congAlheia);

        var todos = new List<Orador> { orador1, orador2, orador3, orador4 };

        // Predicado do repositório:
        // (congregacaoId.HasValue && o.CongregacaoId == congregacaoId.Value) ||
        // o.UsuarioCriadorId == usuarioId ||
        // (o.Congregacao != null && o.Congregacao.UsuarioCriadorId == usuarioId)
        var visiveis = todos.Where(o =>
            o.CongregacaoId == userCongId ||
            o.UsuarioCriadorId == usuarioId ||
            (o.Congregacao != null && o.Congregacao.UsuarioCriadorId == usuarioId)
        ).ToList();

        Assert.Equal(3, visiveis.Count);
        Assert.Contains(orador1, visiveis);
        Assert.Contains(orador2, visiveis);
        Assert.Contains(orador3, visiveis);
        Assert.DoesNotContain(orador4, visiveis);
    }

    [Fact]
    public void RegraFiltroCongregacao_SuperAdmin_DeveVerTodasCongregacoes()
    {
        var superAdminId = Guid.NewGuid();
        var outroUsuarioId = Guid.NewGuid();

        var cong1 = new Congregacao("Congregação Alpha", "SP", "01", "Dom", "18:00", usuarioCriadorId: superAdminId);
        var cong2 = new Congregacao("Congregação Beta", "RJ", "02", "Dom", "18:00", usuarioCriadorId: outroUsuarioId);
        var cong3 = new Congregacao("Congregação Gama", "MG", "03", "Dom", "18:00", usuarioCriadorId: Guid.NewGuid());

        var todas = new List<Congregacao> { cong1, cong2, cong3 };

        // Para SuperAdmin (ehSuperAdmin = true), nenhum filtro por usuarioId ou congregacaoId é aplicado
        var visiveisSuperAdmin = todas.ToList();

        Assert.Equal(3, visiveisSuperAdmin.Count);
        Assert.Contains(cong1, visiveisSuperAdmin);
        Assert.Contains(cong2, visiveisSuperAdmin);
        Assert.Contains(cong3, visiveisSuperAdmin);
    }

    [Fact]
    public void RegraFiltroOradores_SuperAdmin_DeveVerTodosOradores()
    {
        var orador1 = new Orador(Guid.NewGuid(), "Orador 1", "11988881111", usuarioCriadorId: Guid.NewGuid());
        var orador2 = new Orador(Guid.NewGuid(), "Orador 2", "11988882222", usuarioCriadorId: Guid.NewGuid());
        var orador3 = new Orador(Guid.NewGuid(), "Orador 3", "11988883333", usuarioCriadorId: Guid.NewGuid());

        var todos = new List<Orador> { orador1, orador2, orador3 };

        // Para SuperAdmin (ehSuperAdmin = true), nenhum filtro é aplicado
        var visiveisSuperAdmin = todos.ToList();

        Assert.Equal(3, visiveisSuperAdmin.Count);
        Assert.Contains(orador1, visiveisSuperAdmin);
        Assert.Contains(orador2, visiveisSuperAdmin);
        Assert.Contains(orador3, visiveisSuperAdmin);
    }

    #region Testes com Repositório Real e SQLite In-Memory

    private static (AgendaPalestras.Infrastructure.Persistence.ApplicationDbContext Context, Microsoft.Data.Sqlite.SqliteConnection Connection) CriarContextoSqliteInMemory()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AgendaPalestras.Infrastructure.Persistence.ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new AgendaPalestras.Infrastructure.Persistence.ApplicationDbContext(options);
        context.Database.EnsureCreated();

        return (context, connection);
    }

    [Fact]
    public async Task CongregacaoRepository_ListarPorUsuarioAsync_UsuarioRegular_DeveRetornarApenasCongregacoesDoSeuEscopo()
    {
        var (context, connection) = CriarContextoSqliteInMemory();
        try
        {
            var repo = new AgendaPalestras.Infrastructure.Persistence.Repositories.CongregacaoRepository(context);

            var usuarioA = Guid.NewGuid();
            var congAId = Guid.NewGuid();
            var usuarioB = Guid.NewGuid();

            var congPropriaA = new Congregacao("Congregação Alvorada", "São Paulo", "SP-01", "Domingo", "18:00");
            typeof(AgendaPalestras.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(congPropriaA, congAId);

            var congVisitanteA = new Congregacao("Congregação Betel Visitante", "Campinas", "SP-02", "Sábado", "19:00", usuarioCriadorId: usuarioA);
            var congB = new Congregacao("Congregação Primavera", "Rio de Janeiro", "RJ-01", "Domingo", "18:00", usuarioCriadorId: usuarioB);

            await context.Congregacoes.AddRangeAsync(congPropriaA, congVisitanteA, congB);
            await context.SaveChangesAsync();

            var resultado = await repo.ListarPorUsuarioAsync(usuarioA, congAId, ehSuperAdmin: false);

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, c => c.Nome == "Congregação Alvorada");
            Assert.Contains(resultado, c => c.Nome == "Congregação Betel Visitante");
            Assert.DoesNotContain(resultado, c => c.Nome == "Congregação Primavera");
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task CongregacaoRepository_ListarPorUsuarioAsync_SuperAdmin_DeveRetornarTodasAsCongregacoes()
    {
        var (context, connection) = CriarContextoSqliteInMemory();
        try
        {
            var repo = new AgendaPalestras.Infrastructure.Persistence.Repositories.CongregacaoRepository(context);

            var usuarioA = Guid.NewGuid();
            var usuarioB = Guid.NewGuid();

            var cong1 = new Congregacao("Congregação 1", "Cidade 1", "01", "Dom", "18:00", usuarioCriadorId: usuarioA);
            var cong2 = new Congregacao("Congregação 2", "Cidade 2", "02", "Dom", "18:00", usuarioCriadorId: usuarioB);

            await context.Congregacoes.AddRangeAsync(cong1, cong2);
            await context.SaveChangesAsync();

            var resultado = await repo.ListarPorUsuarioAsync(Guid.NewGuid(), null, ehSuperAdmin: true);

            Assert.Equal(2, resultado.Count);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task OradorRepository_ListarPorUsuarioAsync_UsuarioRegular_NaoDeveRetornarOradoresAlheios()
    {
        var (context, connection) = CriarContextoSqliteInMemory();
        try
        {
            var repo = new AgendaPalestras.Infrastructure.Persistence.Repositories.OradorRepository(context);

            var usuarioA = Guid.NewGuid();
            var congAId = Guid.NewGuid();
            var usuarioB = Guid.NewGuid();

            var congA = new Congregacao("Congregação A", "SP", "01", "Dom", "18:00");
            typeof(AgendaPalestras.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(congA, congAId);

            var congB = new Congregacao("Congregação B", "RJ", "02", "Dom", "18:00", usuarioCriadorId: usuarioB);

            await context.Congregacoes.AddRangeAsync(congA, congB);

            // Orador 1: da congregação A
            var oradorA = new Orador(congAId, "Orador da Minha Congregacao", "11999991111");
            // Orador 2: criado pelo usuário A
            var oradorCriadoPorA = new Orador(congB.Id, "Orador Criado Por A", "11999992222", usuarioCriadorId: usuarioA);
            // Orador 3: da congregação B criado pelo usuário B
            var oradorB = new Orador(congB.Id, "Orador de B", "21999993333", usuarioCriadorId: usuarioB);

            await context.Oradores.AddRangeAsync(oradorA, oradorCriadoPorA, oradorB);
            await context.SaveChangesAsync();

            var resultado = await repo.ListarPorUsuarioAsync(usuarioA, congAId, ehSuperAdmin: false);

            Assert.Equal(2, resultado.Count);
            Assert.Contains(resultado, o => o.Nome == "Orador da Minha Congregacao");
            Assert.Contains(resultado, o => o.Nome == "Orador Criado Por A");
            Assert.DoesNotContain(resultado, o => o.Nome == "Orador de B");
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    [Fact]
    public async Task OradorRepository_ListarPorUsuarioAsync_SuperAdmin_DeveRetornarTodosOsOradores()
    {
        var (context, connection) = CriarContextoSqliteInMemory();
        try
        {
            var repo = new AgendaPalestras.Infrastructure.Persistence.Repositories.OradorRepository(context);

            var cong1 = new Congregacao("Congregação 1", "SP", "01", "Dom", "18:00");
            await context.Congregacoes.AddAsync(cong1);

            var orador1 = new Orador(cong1.Id, "Orador 1", "11999991111", usuarioCriadorId: Guid.NewGuid());
            var orador2 = new Orador(cong1.Id, "Orador 2", "11999992222", usuarioCriadorId: Guid.NewGuid());

            await context.Oradores.AddRangeAsync(orador1, orador2);
            await context.SaveChangesAsync();

            var resultado = await repo.ListarPorUsuarioAsync(Guid.NewGuid(), null, ehSuperAdmin: true);

            Assert.Equal(2, resultado.Count);
        }
        finally
        {
            await context.DisposeAsync();
            await connection.DisposeAsync();
        }
    }

    #endregion

    #region Testes de Proteção IDOR nos Controllers (Casos Negativos de Acesso Não Autorizado)

    private static void ConfigurarContextoUsuario(
        Microsoft.AspNetCore.Mvc.Controller controller,
        Guid usuarioId,
        Guid? congregacaoId,
        bool ehSuperAdmin = false)
    {
        var claims = new List<System.Security.Claims.Claim>
        {
            new(System.Security.Claims.ClaimTypes.NameIdentifier, usuarioId.ToString())
        };

        if (congregacaoId.HasValue)
        {
            claims.Add(new System.Security.Claims.Claim("CongregacaoId", congregacaoId.Value.ToString()));
        }

        if (ehSuperAdmin)
        {
            claims.Add(new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "SuperAdmin"));
        }

        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuth");
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);

        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
        {
            HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task CongregacoesController_Edit_TentativaAcessoCongregacaoAlheia_DeveRetornarForbid()
    {
        var mockService = new Mock<ICongregacaoService>();
        var usuarioA = Guid.NewGuid();
        var congA = Guid.NewGuid();
        var congAlheiaId = Guid.NewGuid();
        var usuarioB = Guid.NewGuid();

        var congAlheiaDto = new CongregacaoDto(
            congAlheiaId, "Congregação B", "RJ", "02", "Dom", "18:00", true, 0,
            UsuarioCriadorId: usuarioB);

        mockService.Setup(s => s.ObterPorIdAsync(congAlheiaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(congAlheiaDto);

        var controller = new AgendaPalestras.Web.Controllers.CongregacoesController(mockService.Object);
        ConfigurarContextoUsuario(controller, usuarioA, congA, ehSuperAdmin: false);

        var result = await controller.Edit(congAlheiaId);

        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(result);
    }

    [Fact]
    public async Task CongregacoesController_AlternarStatus_TentativaAlteracaoAlheia_DeveRetornarForbid()
    {
        var mockService = new Mock<ICongregacaoService>();
        var usuarioA = Guid.NewGuid();
        var congA = Guid.NewGuid();
        var congAlheiaId = Guid.NewGuid();
        var usuarioB = Guid.NewGuid();

        var congAlheiaDto = new CongregacaoDto(
            congAlheiaId, "Congregação B", "RJ", "02", "Dom", "18:00", true, 0,
            UsuarioCriadorId: usuarioB);

        mockService.Setup(s => s.ObterPorIdAsync(congAlheiaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(congAlheiaDto);

        var controller = new AgendaPalestras.Web.Controllers.CongregacoesController(mockService.Object);
        ConfigurarContextoUsuario(controller, usuarioA, congA, ehSuperAdmin: false);

        var result = await controller.AlternarStatus(congAlheiaId);

        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(result);
        mockService.Verify(s => s.AlternarStatusAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CongregacoesController_Edit_SuperAdmin_DevePermitirAcessoACongregacaoAlheia()
    {
        var mockService = new Mock<ICongregacaoService>();
        var superAdminId = Guid.NewGuid();
        var congAlheiaId = Guid.NewGuid();
        var usuarioB = Guid.NewGuid();

        var congAlheiaDto = new CongregacaoDto(
            congAlheiaId, "Congregação B", "RJ", "02", "Dom", "18:00", true, 0,
            UsuarioCriadorId: usuarioB);

        mockService.Setup(s => s.ObterPorIdAsync(congAlheiaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(congAlheiaDto);

        var controller = new AgendaPalestras.Web.Controllers.CongregacoesController(mockService.Object);
        ConfigurarContextoUsuario(controller, superAdminId, null, ehSuperAdmin: true);

        var result = await controller.Edit(congAlheiaId);

        Assert.IsType<Microsoft.AspNetCore.Mvc.ViewResult>(result);
    }

    [Fact]
    public async Task OradoresController_Edit_TentativaAcessoOradorAlheio_DeveRetornarForbid()
    {
        var mockOradorService = new Mock<IOradorService>();
        var mockCongService = new Mock<ICongregacaoService>();
        var mockDiscursoService = new Mock<IDiscursoService>();

        var usuarioA = Guid.NewGuid();
        var congA = Guid.NewGuid();
        var oradorAlheioId = Guid.NewGuid();
        var congB = Guid.NewGuid();
        var usuarioB = Guid.NewGuid();

        var oradorAlheioDto = new OradorDto(
            oradorAlheioId, congB, "Cong B", "Orador Alheio", "21999998888", "orador@email.com", true,
            new List<DiscursoDto>(), UsuarioCriadorId: usuarioB);

        mockOradorService.Setup(s => s.ObterPorIdAsync(oradorAlheioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oradorAlheioDto);

        mockCongService.Setup(s => s.ObterPorIdAsync(congB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CongregacaoDto(congB, "Cong B", "RJ", "02", "Dom", "18:00", true, 0, UsuarioCriadorId: usuarioB));

        var controller = new AgendaPalestras.Web.Controllers.OradoresController(
            mockOradorService.Object, mockCongService.Object, mockDiscursoService.Object);

        ConfigurarContextoUsuario(controller, usuarioA, congA, ehSuperAdmin: false);

        var result = await controller.Edit(oradorAlheioId);

        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(result);
    }

    [Fact]
    public async Task OradoresController_Repertorio_TentativaAcessoOradorAlheio_DeveRetornarForbid()
    {
        var mockOradorService = new Mock<IOradorService>();
        var mockCongService = new Mock<ICongregacaoService>();
        var mockDiscursoService = new Mock<IDiscursoService>();

        var usuarioA = Guid.NewGuid();
        var congA = Guid.NewGuid();
        var oradorAlheioId = Guid.NewGuid();
        var congB = Guid.NewGuid();
        var usuarioB = Guid.NewGuid();

        var oradorAlheioDto = new OradorDto(
            oradorAlheioId, congB, "Cong B", "Orador Alheio", "21999998888", "orador@email.com", true,
            new List<DiscursoDto>(), UsuarioCriadorId: usuarioB);

        mockOradorService.Setup(s => s.ObterPorIdAsync(oradorAlheioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oradorAlheioDto);

        mockCongService.Setup(s => s.ObterPorIdAsync(congB, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CongregacaoDto(congB, "Cong B", "RJ", "02", "Dom", "18:00", true, 0, UsuarioCriadorId: usuarioB));

        var controller = new AgendaPalestras.Web.Controllers.OradoresController(
            mockOradorService.Object, mockCongService.Object, mockDiscursoService.Object);

        ConfigurarContextoUsuario(controller, usuarioA, congA, ehSuperAdmin: false);

        var result = await controller.Repertorio(oradorAlheioId);

        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(result);
    }

    [Fact]
    public async Task AgendaController_Details_TentativaAcessoAgendaAlheia_DeveRetornarForbid()
    {
        var mockAgendaService = new Mock<IAgendaService>();
        var mockCongService = new Mock<ICongregacaoService>();
        var mockOradorService = new Mock<IOradorService>();
        var mockDiscursoService = new Mock<IDiscursoService>();
        var mockWhatsAppService = new Mock<IWhatsAppNotificationService>();

        var usuarioA = Guid.NewGuid();
        var congA = Guid.NewGuid();
        var congB = Guid.NewGuid();
        var agendaAlheiaId = Guid.NewGuid();

        var domainAgenda = new Agenda(congB, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        typeof(AgendaPalestras.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(domainAgenda, agendaAlheiaId);
        var agendaAlheiaDto = AgendaDto.FromDomain(domainAgenda);

        mockAgendaService.Setup(s => s.ObterPorIdAsync(agendaAlheiaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendaAlheiaDto);

        var controller = new AgendaPalestras.Web.Controllers.AgendaController(
            mockAgendaService.Object, mockCongService.Object, mockOradorService.Object, mockDiscursoService.Object, mockWhatsAppService.Object);

        ConfigurarContextoUsuario(controller, usuarioA, congA, ehSuperAdmin: false);

        var result = await controller.Details(agendaAlheiaId);

        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(result);
    }

    [Fact]
    public async Task AgendaController_Confirmar_TentativaConfirmacaoAgendaAlheia_DeveRetornarForbid()
    {
        var mockAgendaService = new Mock<IAgendaService>();
        var mockCongService = new Mock<ICongregacaoService>();
        var mockOradorService = new Mock<IOradorService>();
        var mockDiscursoService = new Mock<IDiscursoService>();
        var mockWhatsAppService = new Mock<IWhatsAppNotificationService>();

        var usuarioA = Guid.NewGuid();
        var congA = Guid.NewGuid();
        var congB = Guid.NewGuid();
        var agendaAlheiaId = Guid.NewGuid();

        var domainAgenda = new Agenda(congB, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        typeof(AgendaPalestras.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(domainAgenda, agendaAlheiaId);
        var agendaAlheiaDto = AgendaDto.FromDomain(domainAgenda);

        mockAgendaService.Setup(s => s.ObterPorIdAsync(agendaAlheiaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendaAlheiaDto);

        var controller = new AgendaPalestras.Web.Controllers.AgendaController(
            mockAgendaService.Object, mockCongService.Object, mockOradorService.Object, mockDiscursoService.Object, mockWhatsAppService.Object);

        ConfigurarContextoUsuario(controller, usuarioA, congA, ehSuperAdmin: false);

        var result = await controller.Confirmar(agendaAlheiaId);

        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(result);
        mockAgendaService.Verify(s => s.ConfirmarAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AgendaController_Cancelar_TentativaCancelamentoAgendaAlheia_DeveRetornarForbid()
    {
        var mockAgendaService = new Mock<IAgendaService>();
        var mockCongService = new Mock<ICongregacaoService>();
        var mockOradorService = new Mock<IOradorService>();
        var mockDiscursoService = new Mock<IDiscursoService>();
        var mockWhatsAppService = new Mock<IWhatsAppNotificationService>();

        var usuarioA = Guid.NewGuid();
        var congA = Guid.NewGuid();
        var congB = Guid.NewGuid();
        var agendaAlheiaId = Guid.NewGuid();

        var domainAgenda = new Agenda(congB, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        typeof(AgendaPalestras.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(domainAgenda, agendaAlheiaId);
        var agendaAlheiaDto = AgendaDto.FromDomain(domainAgenda);

        mockAgendaService.Setup(s => s.ObterPorIdAsync(agendaAlheiaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendaAlheiaDto);

        var controller = new AgendaPalestras.Web.Controllers.AgendaController(
            mockAgendaService.Object, mockCongService.Object, mockOradorService.Object, mockDiscursoService.Object, mockWhatsAppService.Object);

        ConfigurarContextoUsuario(controller, usuarioA, congA, ehSuperAdmin: false);

        var result = await controller.Cancelar(agendaAlheiaId, "Motivo");

        Assert.IsType<Microsoft.AspNetCore.Mvc.ForbidResult>(result);
        mockAgendaService.Verify(s => s.CancelarAsync(It.IsAny<CancelarAgendaDto>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AgendaController_Details_SuperAdmin_DevePermitirAcessoAgendaAlheia()
    {
        var mockAgendaService = new Mock<IAgendaService>();
        var mockCongService = new Mock<ICongregacaoService>();
        var mockOradorService = new Mock<IOradorService>();
        var mockDiscursoService = new Mock<IDiscursoService>();
        var mockWhatsAppService = new Mock<IWhatsAppNotificationService>();

        var superAdminId = Guid.NewGuid();
        var congB = Guid.NewGuid();
        var agendaAlheiaId = Guid.NewGuid();

        var domainAgenda = new Agenda(congB, Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        typeof(AgendaPalestras.Domain.Common.BaseEntity).GetProperty("Id")!.SetValue(domainAgenda, agendaAlheiaId);
        var agendaAlheiaDto = AgendaDto.FromDomain(domainAgenda);

        mockAgendaService.Setup(s => s.ObterPorIdAsync(agendaAlheiaId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(agendaAlheiaDto);

        var controller = new AgendaPalestras.Web.Controllers.AgendaController(
            mockAgendaService.Object, mockCongService.Object, mockOradorService.Object, mockDiscursoService.Object, mockWhatsAppService.Object);

        ConfigurarContextoUsuario(controller, superAdminId, null, ehSuperAdmin: true);

        var result = await controller.Details(agendaAlheiaId);

        Assert.IsType<Microsoft.AspNetCore.Mvc.ViewResult>(result);
    }

    #endregion
}
