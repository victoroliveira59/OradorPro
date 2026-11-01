using NetArchTest.Rules;
using Xunit;

namespace AgendaPalestras.ArchitectureTests;

public class CleanArchitectureTests
{
    private const string DomainNamespace = "AgendaPalestras.Domain";
    private const string ApplicationNamespace = "AgendaPalestras.Application";
    private const string InfrastructureNamespace = "AgendaPalestras.Infrastructure";
    private const string WebNamespace = "AgendaPalestras.Web";

    [Fact]
    public void Domain_NaoDeveDependerDeNenhumaOutraCamada()
    {
        var result = Types.InAssembly(typeof(Domain.Common.BaseEntity).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace, WebNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Domain não deve depender de Application, Infrastructure ou Web.");
    }

    [Fact]
    public void Application_NaoDeveDependerDeInfrastructureOuWeb()
    {
        var result = Types.InAssembly(typeof(Application.Services.CongregacaoService).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(InfrastructureNamespace, WebNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Application não deve depender de Infrastructure ou Web.");
    }

    [Fact]
    public void Infrastructure_NaoDeveDependerDeWeb()
    {
        var result = Types.InAssembly(typeof(Infrastructure.Persistence.ApplicationDbContext).Assembly)
            .ShouldNot()
            .HaveDependencyOn(WebNamespace)
            .GetResult();

        Assert.True(result.IsSuccessful, "Infrastructure não deve depender de Web.");
    }

    [Fact]
    public void Controllers_DevemResidirNoNamespaceWebControllers()
    {
        var result = Types.InAssembly(typeof(Web.Controllers.HomeController).Assembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .ResideInNamespace("AgendaPalestras.Web.Controllers")
            .GetResult();

        Assert.True(result.IsSuccessful, "Controllers devem residir no namespace AgendaPalestras.Web.Controllers.");
    }
}
