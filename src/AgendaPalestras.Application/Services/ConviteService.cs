using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace AgendaPalestras.Application.Services;

public class ConviteService : IConviteService
{
    private readonly IConviteCadastroRepository _conviteRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ILogger<ConviteService> _logger;

    public ConviteService(
        IConviteCadastroRepository conviteRepository,
        IUsuarioRepository usuarioRepository,
        ILogger<ConviteService> logger)
    {
        _conviteRepository = conviteRepository;
        _usuarioRepository = usuarioRepository;
        _logger = logger;
    }

    public async Task<ConviteCadastroDto> GerarConviteAsync(
        Guid usuarioCriadorId,
        CriarConviteCadastroDto dto,
        string baseUrl,
        CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioCriadorId, cancellationToken);
        if (usuario == null)
            throw new DomainException("Usuário criador do convite não encontrado.");

        var convite = new ConviteCadastro(
            usuarioCriadorId,
            dto.EmailConvidado,
            dto.DiasValidade > 0 ? dto.DiasValidade : 7
        );

        await _conviteRepository.AdicionarAsync(convite, cancellationToken);
        _logger.LogInformation("Convite de cadastro gerado com código {Codigo} por {Usuario}", convite.Codigo, usuario.Nome);

        return MapToDto(convite, usuario.Nome, baseUrl);
    }

    public async Task<List<ConviteCadastroDto>> ListarTodosAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        var convites = await _conviteRepository.ListarTodosAsync(cancellationToken);
        return convites.Select(c => MapToDto(c, c.CriadoPorUsuario?.Nome ?? "Super Admin", baseUrl)).ToList();
    }

    public async Task<ConviteCadastroDto?> ObterPorCodigoAsync(string codigo, string baseUrl, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return null;
        var convite = await _conviteRepository.ObterPorCodigoAsync(codigo.Trim(), cancellationToken);
        if (convite == null) return null;
        return MapToDto(convite, convite.CriadoPorUsuario?.Nome ?? "Super Admin", baseUrl);
    }

    public async Task<bool> ValidarConviteAsync(string codigo, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return false;
        var convite = await _conviteRepository.ObterPorCodigoAsync(codigo.Trim(), cancellationToken);
        return convite != null && convite.EstaValido();
    }

    public async Task CancelarConviteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var convite = await _conviteRepository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new DomainException("Convite não encontrado.");

        convite.Cancelar();
        await _conviteRepository.AtualizarAsync(convite, cancellationToken);
        _logger.LogInformation("Convite {Id} cancelado.", id);
    }

    private static ConviteCadastroDto MapToDto(ConviteCadastro convite, string criadoPorNome, string baseUrl)
    {
        var urlLimpa = baseUrl.TrimEnd('/');
        var link = $"{urlLimpa}/Auth/Register?token={convite.Codigo}";
        var estaExpirado = DateTime.UtcNow > convite.DataExpiracao && !convite.Usado;

        return new ConviteCadastroDto(
            convite.Id,
            convite.Codigo,
            convite.EmailConvidado,
            convite.CriadoEm,
            convite.DataExpiracao,
            convite.Usado,
            convite.DataUso,
            criadoPorNome,
            link,
            estaExpirado
        );
    }
}
