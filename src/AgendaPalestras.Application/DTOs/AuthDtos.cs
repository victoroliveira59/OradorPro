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
    string Papel
);
