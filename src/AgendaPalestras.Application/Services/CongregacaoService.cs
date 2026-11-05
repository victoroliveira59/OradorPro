using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Domain.Interfaces;
using FluentValidation;

namespace AgendaPalestras.Application.Services;

public interface ICongregacaoService
{
    Task<List<CongregacaoDto>> ListarTodasAsync(bool apenasAtivas = false, CancellationToken cancellationToken = default);
    Task<List<CongregacaoDto>> ListarPorUsuarioAsync(Guid usuarioId, Guid? congregacaoId, bool ehSuperAdmin, bool apenasAtivas = false, CancellationToken cancellationToken = default);
    Task<CongregacaoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<CongregacaoDto> CriarAsync(CriarCongregacaoDto dto, Guid? usuarioCriadorId = null, CancellationToken cancellationToken = default);
    Task AtualizarAsync(AtualizarCongregacaoDto dto, CancellationToken cancellationToken = default);
    Task AlternarStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}

public class CongregacaoService : ICongregacaoService
{
    private readonly ICongregacaoRepository _repository;
    private readonly IValidator<CriarCongregacaoDto> _validator;

    public CongregacaoService(ICongregacaoRepository repository, IValidator<CriarCongregacaoDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<List<CongregacaoDto>> ListarTodasAsync(bool apenasAtivas = false, CancellationToken cancellationToken = default)
    {
        var lista = await _repository.ListarTodasAsync(apenasAtivas, cancellationToken);
        return lista.Select(MapToDto).ToList();
    }

    public async Task<List<CongregacaoDto>> ListarPorUsuarioAsync(Guid usuarioId, Guid? congregacaoId, bool ehSuperAdmin, bool apenasAtivas = false, CancellationToken cancellationToken = default)
    {
        var lista = await _repository.ListarPorUsuarioAsync(usuarioId, congregacaoId, ehSuperAdmin, apenasAtivas, cancellationToken);
        return lista.Select(MapToDto).ToList();
    }

    public async Task<CongregacaoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.ObterPorIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<CongregacaoDto> CriarAsync(CriarCongregacaoDto dto, Guid? usuarioCriadorId = null, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        if (await _repository.ExisteComNomeAsync(dto.Nome, cancellationToken: cancellationToken))
            throw new DomainException($"Já existe uma congregação com o nome '{dto.Nome}'.");

        var entity = new Congregacao(dto.Nome, dto.Cidade, dto.Circuito, dto.DiasReuniao, dto.HorarioReuniao, usuarioCriadorId, dto.Endereco, dto.LinkGoogleMaps);
        await _repository.AdicionarAsync(entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task AtualizarAsync(AtualizarCongregacaoDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.ObterPorIdAsync(dto.Id, cancellationToken)
            ?? throw new DomainException("Congregação não encontrada.");

        if (await _repository.ExisteComNomeAsync(dto.Nome, dto.Id, cancellationToken))
            throw new DomainException($"Já existe outra congregação cadastrada com o nome '{dto.Nome}'.");

        entity.Atualizar(dto.Nome, dto.Cidade, dto.Circuito, dto.DiasReuniao, dto.HorarioReuniao, dto.Endereco, dto.LinkGoogleMaps);
        if (dto.Ativo) entity.Ativar(); else entity.Desativar();

        await _repository.AtualizarAsync(entity, cancellationToken);
    }

    public async Task AlternarStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new DomainException("Congregação não encontrada.");

        if (entity.Ativo) entity.Desativar(); else entity.Ativar();
        await _repository.AtualizarAsync(entity, cancellationToken);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.ExcluirAsync(id, cancellationToken);
    }

    private static CongregacaoDto MapToDto(Congregacao c) =>
        new(c.Id, c.Nome, c.Cidade, c.Circuito, c.DiasReuniao, c.HorarioReuniao, c.Ativo, c.Oradores.Count, c.Endereco, c.LinkGoogleMaps, c.UsuarioCriadorId);
}
