using AgendaPalestras.Application.DTOs;
using AgendaPalestras.Application.Validators;
using Xunit;

namespace AgendaPalestras.UnitTests;

public class ValidatorTests
{
    [Fact]
    public void CriarCongregacaoValidator_ComNomeVazio_DeveFalhar()
    {
        var validator = new CriarCongregacaoValidator();
        var dto = new CriarCongregacaoDto("", "Cidade", "Circuito", "Domingo", "18:00");

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Nome");
    }

    [Fact]
    public void CriarOradorValidator_ComTelefoneInvalido_DeveFalhar()
    {
        var validator = new CriarOradorValidator();
        var dto = new CriarOradorDto(Guid.NewGuid(), "Nome Orador", "123", ""); // Telefone curto

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TelefoneWhatsapp");
    }

    [Fact]
    public void CriarDiscursoValidator_ComNumeroZero_DeveFalhar()
    {
        var validator = new CriarDiscursoValidator();
        var dto = new CriarDiscursoDto(0, "Tema Teste");

        var result = validator.Validate(dto);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Numero");
    }
}
