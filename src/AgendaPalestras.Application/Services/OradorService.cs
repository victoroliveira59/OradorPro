using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Domain.Entities;
using AgendaPalestras.Domain.Exceptions;
using AgendaPalestras.Domain.Interfaces;
using FluentValidation;

namespace AgendaPalestras.Application.Services;

public interface IOradorService
{
    Task<List<OradorDto>> ListarTodosAsync(Guid? congregacaoId = null, bool apenasAtivos = false, CancellationToken cancellationToken = default);
    Task<OradorDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<OradorDto> CriarAsync(CriarOradorDto dto, CancellationToken cancellationToken = default);
    Task AtualizarAsync(AtualizarOradorDto dto, CancellationToken cancellationToken = default);
    Task SincronizarRepertorioAsync(SincronizarRepertorioDto dto, CancellationToken cancellationToken = default);
    Task AlternarStatusAsync(Guid id, CancellationToken cancellationToken = default);
    Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default);
}

public class OradorService : IOradorService
{
    private readonly IOradorRepository _oradorRepository;
    private readonly ICongregacaoRepository _congregacaoRepository;
    private readonly IDiscursoRepository _discursoRepository;
    private readonly IValidator<CriarOradorDto> _validator;

    public OradorService(
        IOradorRepository oradorRepository,
        ICongregacaoRepository congregacaoRepository,
        IDiscursoRepository discursoRepository,
        IValidator<CriarOradorDto> validator)
    {
        _oradorRepository = oradorRepository;
        _congregacaoRepository = congregacaoRepository;
        _discursoRepository = discursoRepository;
        _validator = validator;
    }

    public async Task<List<OradorDto>> ListarTodosAsync(Guid? congregacaoId = null, bool apenasAtivos = false, CancellationToken cancellationToken = default)
    {
        var lista = await _oradorRepository.ListarTodosAsync(congregacaoId, apenasAtivos, cancellationToken);
        return lista.Select(MapToDto).ToList();
    }

    public async Task<OradorDto?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _oradorRepository.ObterPorIdAsync(id, incluirRepertorio: true, cancellationToken);
        return entity == null ? null : MapToDto(entity);
    }

    public async Task<OradorDto> CriarAsync(CriarOradorDto dto, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        var congregacao = await _congregacaoRepository.ObterPorIdAsync(dto.CongregacaoId, cancellationToken)
            ?? throw new DomainException("Congregação de origem não encontrada.");

        var entity = new Orador(dto.CongregacaoId, dto.Nome, dto.TelefoneWhatsapp, dto.Email);

        if (dto.DiscursosIniciaisIds != null && dto.DiscursosIniciaisIds.Count > 0)
        {
            var discursosValidos = await _discursoRepository.ListarPorIdsAsync(dto.DiscursosIniciaisIds, cancellationToken);
            foreach (var discurso in discursosValidos)
            {
                entity.AdicionarDiscurso(discurso.Id);
            }
        }

        await _oradorRepository.AdicionarAsync(entity, cancellationToken);
        return MapToDto(entity, congregacao.Nome);
    }

    public async Task AtualizarAsync(AtualizarOradorDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _oradorRepository.ObterPorIdAsync(dto.Id, cancellationToken: cancellationToken)
            ?? throw new DomainException("Orador não encontrado.");

        var congregacao = await _congregacaoRepository.ObterPorIdAsync(dto.CongregacaoId, cancellationToken)
            ?? throw new DomainException("Congregação de origem não encontrada.");

        entity.Atualizar(dto.CongregacaoId, dto.Nome, dto.TelefoneWhatsapp, dto.Email);
        if (dto.Ativo) entity.Ativar(); else entity.Desativar();

        await _oradorRepository.AtualizarAsync(entity, cancellationToken);
    }

    public async Task SincronizarRepertorioAsync(SincronizarRepertorioDto dto, CancellationToken cancellationToken = default)
    {
        var entity = await _oradorRepository.ObterPorIdAsync(dto.OradorId, incluirRepertorio: true, cancellationToken)
            ?? throw new DomainException("Orador não encontrado.");

        var discursosValidos = await _discursoRepository.ListarPorIdsAsync(dto.DiscursosIds, cancellationToken);
        entity.SincronizarRepertorio(discursosValidos.Select(d => d.Id));

        await _oradorRepository.AtualizarAsync(entity, cancellationToken);
    }

    public async Task AlternarStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await _oradorRepository.ObterPorIdAsync(id, cancellationToken: cancellationToken)
            ?? throw new DomainException("Orador não encontrado.");

        if (entity.Ativo) entity.Desativar(); else entity.Ativar();
        await _oradorRepository.AtualizarAsync(entity, cancellationToken);
    }

    public async Task ExcluirAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await _oradorRepository.ExcluirAsync(id, cancellationToken);
    }

    private static OradorDto MapToDto(Orador o) =>
        MapToDto(o, o.Congregacao?.Nome ?? string.Empty);

    private static OradorDto MapToDto(Orador o, string congregacaoNome)
    {
        var repertorio = o.OradorDiscursos
            .Where(od => od.Discurso != null)
            .Select(od => new DiscursoDto(od.Discurso.Id, od.Discurso.Numero, od.Discurso.Tema, od.Discurso.Ativo))
            .OrderBy(d => d.Numero)
            .ToList();

        return new OradorDto(
            o.Id,
            o.CongregacaoId,
            congregacaoNome,
            o.Nome,
            o.TelefoneWhatsapp,
            o.Email,
            o.Ativo,
            repertorio
        );
    }
}
