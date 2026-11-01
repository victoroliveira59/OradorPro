using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Domain.Interfaces;
using FluentValidation;

namespace AgendaPalestras.Application.Services;

public interface IDiscursoService
{
    Task<List<DiscursoDto>> ListarTodosAsync(bool apenasAtivos = false, CancellationToken cancellationToken = default);
    Task<DiscursoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DiscursoDto> CriarAsync(CriarDiscursoDto dto, CancellationToken cancellationToken = default);
    Task AtualizarAsync(AtualizarDiscursoDto dto, CancellationToken cancellationToken = default);
    Task AlternarStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}

public class DiscursoService : IDiscursoService
{
    private readonly IDiscursoRepository _repository;
    private readonly IValidator<CriarDiscursoDto> _validator;

    public DiscursoService(IDiscursoRepository repository, IValidator<CriarDiscursoDto> validator)
    {
        _repository = repository;
        _validator = validator;
    }

    public async Task<List<DiscursoDto>> ListarTodosAsync(bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var lista = await _repository.ListarTodosAsync(apenasAtivos, cancellationToken);
        return lista.Select(MapToDto).ToList();
    }

    public async Task<DiscursoDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.ObterPorIdAsync(id, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<DiscursoDto> CriarAsync(CriarDiscursoDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var existente = await _repository.ObterPorNumeroAsync(dto.Numero, cancellationToken);
        if (existente != null)
            throw new DomainException($"Já existe um discurso cadastrado com o número {dto.Numero} ('{existente.Tema}').");

        var entity = new Discurso(dto.Numero, dto.Tema);
        await _repository.AdicionarAsync(entity, cancellationToken);
        return MapToDto(entity);
    }

    public async Task AtualizarAsync(AtualizarDiscursoDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.ObterPorIdAsync(dto.Id, cancellationToken)
            ?? throw new DomainException("Discurso não encontrado.");

        var existente = await _repository.ObterPorNumeroAsync(dto.Numero, cancellationToken);
        if (existente != null && existente.Id != dto.Id)
            throw new DomainException($"Já existe outro discurso cadastrado com o número {dto.Numero}.");

        entity.Atualizar(dto.Numero, dto.Tema);
        if (dto.Ativo) entity.Ativar(); else entity.Desativar();

        await _repository.AtualizarAsync(entity, cancellationToken);
    }

    public async Task AlternarStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.ObterPorIdAsync(id, cancellationToken)
            ?? throw new DomainException("Discurso não encontrado.");

        if (entity.Ativo) entity.Desativar(); else entity.Ativar();
        await _repository.AtualizarAsync(entity, cancellationToken);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _repository.ExcluirAsync(id, cancellationToken);
    }

    private static DiscursoDto MapToDto(Discurso d) =>
        new(d.Id, d.Numero, d.Tema, d.Ativo);
}
