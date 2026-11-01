using AgendaPalestras.Application.DTOs;
using FluentValidation;

namespace AgendaPalestras.Application.Validators;

public class CriarCongregacaoValidator : AbstractValidator<CriarCongregacaoDto>
{
    public CriarCongregacaoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome da congregação é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Cidade)
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Circuito)
            .MaximumLength(50).WithMessage("Circuito deve ter no máximo 50 caracteres.");
    }
}
