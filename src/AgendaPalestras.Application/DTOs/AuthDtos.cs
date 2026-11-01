namespace AgendaPalestras.Application.DTOs;

public record LoginDto(
    string Email,
    string Senha,
    bool LembrarMe = false
);

public record UsuarioAutenticadoDto(
    Guid Id,
    string Nome,
    string Email,
    string Papel,
    Guid? CongregacaoId = null,
    string? CongregacaoNome = null
);

public record RegistrarUsuarioDto(
    string Nome,
    string Email,
    string Senha,
    string NomeCongregacao,
    string TokenConvite = "",
    string? Cidade = null,
    string? Circuito = null,
    string? Endereco = null,
    string? LinkGoogleMaps = null,
    string? DiasReuniao = null,
    string? HorarioReuniao = null
);
