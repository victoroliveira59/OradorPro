using AgendaPalestras.Application.DTOs;

namespace AgendaPalestras.Application.Services;

public interface IAuthService
{
    Task<UsuarioAutenticadoDto?> AutenticarAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task InicializarAdminPadraoAsync(CancellationToken cancellationToken = default);
}
