namespace AgendaPalestras.Application.DTOs;

public record DiscursoDto(
    Guid Id,
    int Numero,
    string Tema,
    bool Ativo
);

public record CriarDiscursoDto(
    int Numero,
    string Tema
);

public record AtualizarDiscursoDto(
    Guid Id,
    int Numero,
    string Tema,
    bool Ativo
);
