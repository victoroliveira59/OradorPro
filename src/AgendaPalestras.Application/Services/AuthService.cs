using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgendaPalestras.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        IPasswordHasherService passwordHasher,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _passwordHasher = passwordHasher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<UsuarioAutenticadoDto?> AutenticarAsync(LoginDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Senha))
            return null;

        var usuario = await _usuarioRepository.ObterPorEmailAsync(dto.Email, cancellationToken);
        if (usuario == null || !usuario.Ativo)
        {
            _logger.LogWarning("Falha de autenticação: usuário {Email} não encontrado ou inativo.", dto.Email);
            return null;
        }

        var senhaValida = _passwordHasher.VerifyPassword(dto.Senha, usuario.HashSenha);
        if (!senhaValida)
        {
            _logger.LogWarning("Falha de autenticação: senha inválida para {Email}.", dto.Email);
            return null;
        }

        _logger.LogInformation("Usuário {Email} autenticado com sucesso.", usuario.Email);
        return new UsuarioAutenticadoDto(usuario.Id, usuario.Nome, usuario.Email, usuario.Papel);
    }

    public async Task InicializarAdminPadraoAsync(CancellationToken cancellationToken = default)
    {
        var existeAdmin = await _usuarioRepository.ExisteAdminAsync(cancellationToken);
        if (!existeAdmin)
        {
            var adminEmail = _configuration["ADMIN_DEFAULT_EMAIL"] ?? "admin@agendapalestras.local";
            var adminSenha = _configuration["ADMIN_DEFAULT_PASSWORD"] ?? "Admin@123456";

            var hash = _passwordHasher.HashPassword(adminSenha);
            var novoAdmin = new Usuario("Administrador Central", adminEmail, hash, "Admin");

            await _usuarioRepository.AdicionarAsync(novoAdmin, cancellationToken);
            _logger.LogInformation("Usuário Admin inicial provisionado com sucesso ({Email}).", adminEmail);
        }
    }
}
