using AgendaPalestras.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace AgendaPalestras.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<ICongregacaoService, CongregacaoService>();
        services.AddScoped<IOradorService, OradorService>();
        services.AddScoped<IDiscursoService, DiscursoService>();
        services.AddScoped<IAgendaService, AgendaService>();
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
