namespace AgendaPalestras.Application.DTOs;

public record CongregacaoDto(
    Guid Id,
    string Nome,
    string Cidade,
    string Circuito,
    string DiasReuniao,
    string HorarioReuniao,
    bool Ativo,
    int TotalOradores
);

public record CriarCongregacaoDto(
    string Nome,
    string Cidade,
    string Circuito,
    string DiasReuniao,
    string HorarioReuniao
);

public record AtualizarCongregacaoDto(
    Guid Id,
    string Nome,
    string Cidade,
    string Circuito,
    string DiasReuniao,
    string HorarioReuniao,
    bool Ativo
);
