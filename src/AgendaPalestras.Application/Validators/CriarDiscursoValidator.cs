using AgendaPalestras.Application.DTOs;
using FluentValidation;

namespace AgendaPalestras.Application.Validators;

public class CriarDiscursoValidator : AbstractValidator<CriarDiscursoDto>
{
    public CriarDiscursoValidator()
    {
        RuleFor(x => x.Numero)
            .GreaterThan(0).WithMessage("O número do discurso deve ser maior que zero.");

        RuleFor(x => x.Tema)
            .NotEmpty().WithMessage("O tema do discurso é obrigatório.")
            .MaximumLength(200).WithMessage("O tema deve ter no máximo 200 caracteres.");
    }
}
