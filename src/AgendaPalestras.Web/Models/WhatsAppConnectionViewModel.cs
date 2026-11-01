using AgendaPalestras.Application.DTOs;

namespace AgendaPalestras.Web.Models;

public class WhatsAppConnectionViewModel
{
    public ConfiguracaoWhatsAppDto Configuracao { get; set; } = null!;
    public EvolutionConnectionStatusDto Status { get; set; } = null!;
    public EvolutionQrCodeDto? QrCode { get; set; }

    public bool IsSuperAdmin { get; set; }
    public bool InstanciaNaoConfigurada { get; set; }
    public IReadOnlyList<UsuarioWhatsAppInstanciaDto> UsuariosInstancias { get; set; } = Array.Empty<UsuarioWhatsAppInstanciaDto>();
}
