namespace AgendaPalestras.Domain.Enums;

public enum DirecaoMensagem
{
    Envio = 1,
    Recebimento = 2
}

public enum TipoMensagem
{
    Convite = 1,
    Lembrete = 2,
    Resposta = 3,
    Outro = 4
}

public enum StatusMensagem
{
    Pendente = 1,
    Enviado = 2,
    Entregue = 3,
    Erro = 4,
    Processado = 5
}
