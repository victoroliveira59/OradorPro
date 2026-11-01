namespace AgendaPalestras.Application.DTOs;

public record EvolutionConnectionStatusDto(
    string InstanceName,
    string State,
    bool Conectado,
    string? Numero = null,
    string? ProfileName = null
);

public record EvolutionQrCodeDto(
    string InstanceName,
    string? QrCodeBase64,
    string? PairingCode,
    int Count = 1
);

public record ConfiguracaoWhatsAppDto(
    string ApiUrl,
    string ApiKey,
    string InstanceName
);

public record TestarEnvioWhatsAppDto(
    string Telefone,
    string Mensagem
);
