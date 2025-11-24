using FluentValidation;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarSaque;

public class RealizarSaqueCommandValidator : AbstractValidator<RealizarSaqueCommand>
{
    public RealizarSaqueCommandValidator()
    {
        RuleFor(x => x.IdContaCorrente)
            .NotEmpty()
            .WithMessage("ID da conta corrente é obrigatório");

        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("Valor do saque deve ser maior que zero")
            .LessThanOrEqualTo(5000)
            .WithMessage("Valor do saque não pode exceder R$ 5.000,00 por operação");
    }
}