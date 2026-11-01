using AgendaPalestras.Application.DTOs;
using FluentValidation;

namespace AgendaPalestras.Application.Validators;

public class CriarOradorValidator : AbstractValidator<CriarOradorDto>
{
    public CriarOradorValidator()
    {
        RuleFor(x => x.CongregacaoId)
            .NotEmpty().WithMessage("A congregação de origem é obrigatória.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do orador é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.TelefoneWhatsapp)
            .NotEmpty().WithMessage("Telefone de WhatsApp é obrigatório.")
            .Must(ValidarTelefone).WithMessage("Telefone deve conter DDD e no mínimo 10 dígitos numéricos.");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Formato de e-mail inválido.");
    }

    private static bool ValidarTelefone(string telefone)
    {
        if (string.IsNullOrWhiteSpace(telefone)) return false;
        var digitos = telefone.Where(char.IsDigit).Count();
        return digitos >= 10 && digitos <= 15;
    }
}
