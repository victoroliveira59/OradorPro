namespace AgendaPalestras.Application.DTOs;

public record CongregacaoDto(
    Guid Id,
    string Nome,
    string Cidade,
    string Circuito,
    string DiasReuniao,
    string HorarioReuniao,
    bool Ativo,
    int TotalOradores,
    string? Endereco = null,
    string? LinkGoogleMaps = null,
    Guid? UsuarioCriadorId = null
);

public record CriarCongregacaoDto(
    string Nome,
    string Cidade,
    string Circuito,
    string DiasReuniao,
    string HorarioReuniao,
    string? Endereco = null,
    string? LinkGoogleMaps = null
);

public record AtualizarCongregacaoDto(
    Guid Id,
    string Nome,
    string Cidade,
    string Circuito,
    string DiasReuniao,
    string HorarioReuniao,
    bool Ativo,
    string? Endereco = null,
    string? LinkGoogleMaps = null
);
