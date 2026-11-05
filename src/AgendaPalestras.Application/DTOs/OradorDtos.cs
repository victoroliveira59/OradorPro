namespace AgendaPalestras.Application.DTOs;

public record OradorDto(
    Guid Id,
    Guid CongregacaoId,
    string CongregacaoNome,
    string Nome,
    string TelefoneWhatsapp,
    string Email,
    bool Ativo,
    List<DiscursoDto> Repertorio,
    Guid? UsuarioCriadorId = null
);

public record CriarOradorDto(
    Guid CongregacaoId,
    string Nome,
    string TelefoneWhatsapp,
    string Email,
    List<Guid>? DiscursosIniciaisIds = null,
    string? NovaCongregacaoNome = null,
    Guid? UsuarioCriadorId = null
);

public record AtualizarOradorDto(
    Guid Id,
    Guid CongregacaoId,
    string Nome,
    string TelefoneWhatsapp,
    string Email,
    bool Ativo
);

public record SincronizarRepertorioDto(
    Guid OradorId,
    List<Guid> DiscursosIds
);
