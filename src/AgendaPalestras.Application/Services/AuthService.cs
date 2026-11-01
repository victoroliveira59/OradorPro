using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AgendaPalestras.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ICongregacaoRepository _congregacaoRepository;
    private readonly IPasswordHasherService _passwordHasher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUsuarioRepository usuarioRepository,
        ICongregacaoRepository congregacaoRepository,
        IPasswordHasherService passwordHasher,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _congregacaoRepository = congregacaoRepository;
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
        return new UsuarioAutenticadoDto(
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Papel,
            usuario.CongregacaoId,
            usuario.Congregacao?.Nome
        );
    }

    public async Task<UsuarioAutenticadoDto> RegistrarUsuarioAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Nome))
            throw new DomainException("O nome do usuário é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Email))
            throw new DomainException("O e-mail é obrigatório.");
        if (string.IsNullOrWhiteSpace(dto.Senha) || dto.Senha.Length < 6)
            throw new DomainException("A senha deve conter no mínimo 6 caracteres.");
        if (string.IsNullOrWhiteSpace(dto.NomeCongregacao))
            throw new DomainException("O nome da congregação é obrigatório.");

        var usuarioExistente = await _usuarioRepository.ObterPorEmailAsync(dto.Email, cancellationToken);
        if (usuarioExistente != null)
            throw new DomainException($"Já existe um usuário cadastrado com o e-mail '{dto.Email}'.");

        var nomeCongregacaoTrim = dto.NomeCongregacao.Trim();
        var existeCongregacao = await _congregacaoRepository.ExisteComNomeAsync(nomeCongregacaoTrim, cancellationToken: cancellationToken);
        if (existeCongregacao)
            throw new DomainException($"Já existe uma congregação cadastrada com o nome '{nomeCongregacaoTrim}'.");

        // 1. Criar a Congregação do usuário
        var congregacao = new Congregacao(
            nomeCongregacaoTrim,
            dto.Cidade ?? string.Empty,
            dto.Circuito ?? string.Empty,
            dto.DiasReuniao ?? string.Empty,
            dto.HorarioReuniao ?? string.Empty,
            usuarioCriadorId: null,
            endereco: dto.Endereco,
            linkGoogleMaps: dto.LinkGoogleMaps
        );
        await _congregacaoRepository.AdicionarAsync(congregacao, cancellationToken);

        // 2. Criar o Usuário vinculado à sua Congregação
        var hash = _passwordHasher.HashPassword(dto.Senha);
        var novoUsuario = new Usuario(dto.Nome, dto.Email, hash, "Admin", congregacao.Id);
        await _usuarioRepository.AdicionarAsync(novoUsuario, cancellationToken);

        _logger.LogInformation("Novo usuário {Email} registrado com sucesso vinculado à congregação {Congregacao}.", novoUsuario.Email, congregacao.Nome);

        return new UsuarioAutenticadoDto(
            novoUsuario.Id,
            novoUsuario.Nome,
            novoUsuario.Email,
            novoUsuario.Papel,
            congregacao.Id,
            congregacao.Nome
        );
    }

    public async Task InicializarAdminPadraoAsync(CancellationToken cancellationToken = default)
    {
        var existeAdmin = await _usuarioRepository.ExisteAdminAsync(cancellationToken);
        if (!existeAdmin)
        {
            var adminEmail = _configuration["ADMIN_DEFAULT_EMAIL"] ?? "admin@agendapalestras.local";
            var adminSenha = _configuration["ADMIN_DEFAULT_PASSWORD"] ?? "Admin@123456";

            // Se não houver congregação, provisionar uma inicial padrão
            var todasCongregacoes = await _congregacaoRepository.ListarTodasAsync(false, cancellationToken);
            Congregacao? congregacaoPadrao = todasCongregacoes.FirstOrDefault();
            if (congregacaoPadrao == null)
            {
                congregacaoPadrao = new Congregacao(
                    "Congregação Central",
                    "São Paulo",
                    "SP-01",
                    "Domingo",
                    "19:00",
                    endereco: "Rua das Reuniões, 100",
                    linkGoogleMaps: "https://maps.google.com"
                );
                await _congregacaoRepository.AdicionarAsync(congregacaoPadrao, cancellationToken);
            }

            var hash = _passwordHasher.HashPassword(adminSenha);
            var novoAdmin = new Usuario("Administrador Central", adminEmail, hash, "Admin", congregacaoPadrao.Id);

            await _usuarioRepository.AdicionarAsync(novoAdmin, cancellationToken);
            _logger.LogInformation("Usuário Admin inicial provisionado com sucesso ({Email}) para congregação {Congregacao}.", adminEmail, congregacaoPadrao.Nome);
        }
    }
}
