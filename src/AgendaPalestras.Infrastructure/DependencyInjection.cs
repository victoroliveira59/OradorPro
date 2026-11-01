using AgendaPalestras.Application.Services;
using AgendaPalestras.Domain.Interfaces;
using AgendaPalestras.Infrastructure.Persistence;
using AgendaPalestras.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgendaPalestras.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? Environment.GetEnvironmentVariable("DATABASE_URL");

        var pgDb = Environment.GetEnvironmentVariable("POSTGRES_DB") ?? configuration["POSTGRES_DB"];
        var pgUser = Environment.GetEnvironmentVariable("POSTGRES_USER") ?? configuration["POSTGRES_USER"];
        var pgPass = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD") ?? configuration["POSTGRES_PASSWORD"] ?? "postgres";
        var pgPort = Environment.GetEnvironmentVariable("POSTGRES_PORT") ?? configuration["POSTGRES_PORT"] ?? "5433";
        var pgHost = Environment.GetEnvironmentVariable("POSTGRES_HOST") ?? configuration["POSTGRES_HOST"] ?? "localhost";

        if (!string.IsNullOrWhiteSpace(pgDb) && !string.IsNullOrWhiteSpace(pgUser))
        {
            connectionString = $"Host={pgHost};Port={pgPort};Database={pgDb};Username={pgUser};Password={pgPass}";
        }

        // Se nenhuma configuração for informada, o padrão oficial é PostgreSQL na porta 5433
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = "Host=localhost;Port=5433;Database=agendapalestras;Username=postgres;Password=postgres";
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (connectionString.Contains("Host=", StringComparison.OrdinalIgnoreCase))
            {
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                options.UseNpgsql(connectionString);
            }
            else
            {
                options.UseSqlite(connectionString);
            }
        });

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ICongregacaoRepository, CongregacaoRepository>();
        services.AddScoped<IOradorRepository, OradorRepository>();
        services.AddScoped<IDiscursoRepository, DiscursoRepository>();
        services.AddScoped<IAgendaRepository, AgendaRepository>();
        services.AddScoped<IMensagemHistoricoRepository, MensagemHistoricoRepository>();
        services.AddScoped<IConfiguracaoSistemaRepository, ConfiguracaoSistemaRepository>();

        services.AddSingleton<IPasswordHasherService, Services.PasswordHasherService>();
        services.AddSingleton<IProgramacaoPdfService, Services.ProgramacaoPdfService>();
        services.AddHttpClient<IEvolutionApiService, Services.EvolutionApiService>();
        services.AddScoped<IWhatsAppNotificationService, Services.WhatsAppNotificationService>();
        services.AddHostedService<BackgroundJobs.Lembrete7DiasBackgroundService>();

        return services;
    }
}
