namespace AgendaPalestras.Application.DTOs;

public record ConviteCadastroDto(
    Guid Id,
    string Codigo,
    string? EmailConvidado,
    DateTime DataCriacao,
    DateTime DataExpiracao,
    bool Usado,
    DateTime? DataUso,
    string CriadoPorNome,
    string LinkCadastro,
    bool EstaExpirado
);

public record CriarConviteCadastroDto(
    string? EmailConvidado = null,
    int DiasValidade = 7
);
