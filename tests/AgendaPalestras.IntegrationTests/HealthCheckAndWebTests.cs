using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace AgendaPalestras.IntegrationTests;

public class HealthCheckAndWebTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckAndWebTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthCheck_DeveRetornarStatus200EHealthy()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/healthz");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", content);
    }

    [Fact]
    public async Task HomeIndex_DeveRetornarStatus200OK()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Login_DeveRetornar200OKComFormulario()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/Auth/Login");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Entrar", html);
    }

    [Fact]
    public async Task RotaProtegida_SemAutenticacao_DeveRedirecionarParaLogin()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/Agenda");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Auth/Login", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task WebhookWhatsApp_SemSecretValido_DeveRetornar401Unauthorized()
    {
        var client = _factory.CreateClient();

        var payload = new { from = "5511999998888", text = "1" };
        var response = await client.PostAsJsonAsync("/api/webhooks/whatsapp", payload);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task SincronizarRepertorio_DevePersistirAdicaoERemocaoSemErroDeConcorrencia()
    {
        using var scope = _factory.Services.CreateScope();
        var sp = scope.ServiceProvider;
        var oradorService = sp.GetRequiredService<AgendaPalestras.Application.Services.IOradorService>();
        var discursoService = sp.GetRequiredService<AgendaPalestras.Application.Services.IDiscursoService>();
        var congregacaoService = sp.GetRequiredService<AgendaPalestras.Application.Services.ICongregacaoService>();

        // Criar congregação
        var cong = await congregacaoService.CriarAsync(new AgendaPalestras.Application.DTOs.CriarCongregacaoDto(
            $"Cong Test {Guid.NewGuid():N}", "Cidade", "C1", "Quarta", "19:30"));

        // Criar discursos
        var d1 = await discursoService.CriarAsync(new AgendaPalestras.Application.DTOs.CriarDiscursoDto(
            new Random().Next(1000, 9999), $"Tema A {Guid.NewGuid():N}"));
        var d2 = await discursoService.CriarAsync(new AgendaPalestras.Application.DTOs.CriarDiscursoDto(
            new Random().Next(10000, 99999), $"Tema B {Guid.NewGuid():N}"));

        // Criar orador
        var orador = await oradorService.CriarAsync(new AgendaPalestras.Application.DTOs.CriarOradorDto(
            cong.Id, "Orador Teste Repertorio", $"1199{new Random().Next(100000, 999999)}", "teste@email.com"));

        // 1. Sincroniza adicionando d1
        await oradorService.SincronizarRepertorioAsync(new AgendaPalestras.Application.DTOs.SincronizarRepertorioDto(
            orador.Id, new List<Guid> { d1.Id }));

        var oradorConsultado1 = await oradorService.ObterPorIdAsync(orador.Id);
        Assert.NotNull(oradorConsultado1);
        Assert.Single(oradorConsultado1.Repertorio);
        Assert.Equal(d1.Id, oradorConsultado1.Repertorio[0].Id);

        // 2. Sincroniza removendo d1 e adicionando d2 (testa remoção + inserção simultânea)
        await oradorService.SincronizarRepertorioAsync(new AgendaPalestras.Application.DTOs.SincronizarRepertorioDto(
            orador.Id, new List<Guid> { d2.Id }));

        var oradorConsultado2 = await oradorService.ObterPorIdAsync(orador.Id);
        Assert.NotNull(oradorConsultado2);
        Assert.Single(oradorConsultado2.Repertorio);
        Assert.Equal(d2.Id, oradorConsultado2.Repertorio[0].Id);
    }
}
