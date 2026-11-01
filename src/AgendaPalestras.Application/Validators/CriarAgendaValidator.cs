using AgendaPalestras.Application.DTOs;
using FluentValidation;

namespace AgendaPalestras.Application.Validators;

public class CriarAgendaValidator : AbstractValidator<CriarAgendaDto>
{
    public CriarAgendaValidator()
    {
        RuleFor(x => x.CongregacaoId)
            .NotEmpty().WithMessage("A congregação é obrigatória.");

        RuleFor(x => x.OradorId)
            .NotEmpty().WithMessage("O orador é obrigatório.");

        RuleFor(x => x.DiscursoId)
            .NotEmpty().WithMessage("O tema de discurso é obrigatório.");

        RuleFor(x => x.DataHora)
            .GreaterThan(DateTime.UtcNow.AddMinutes(-5))
            .WithMessage("A data e hora do agendamento deve ser uma data futura.");

        RuleFor(x => x.Observacoes)
            .MaximumLength(1000)
            .WithMessage("As observações não podem exceder 1000 caracteres.");
    }
}
