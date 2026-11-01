namespace AgendaPalestras.Domain.Entities;

public class ConfiguracaoSistema
{
    public string Chave { get; private set; } = string.Empty;
    public string Valor { get; private set; } = string.Empty;
    public DateTime AtualizadoEm { get; private set; } = DateTime.UtcNow;

    protected ConfiguracaoSistema() { }

    public ConfiguracaoSistema(string chave, string valor)
    {
        if (string.IsNullOrWhiteSpace(chave))
            throw new ArgumentException("A chave de configuração não pode ser vazia.", nameof(chave));

        Chave = chave.Trim();
        Valor = valor ?? string.Empty;
        AtualizadoEm = DateTime.UtcNow;
    }

    public void Atualizar(string novoValor)
    {
        Valor = novoValor ?? string.Empty;
        AtualizadoEm = DateTime.UtcNow;
    }
}
