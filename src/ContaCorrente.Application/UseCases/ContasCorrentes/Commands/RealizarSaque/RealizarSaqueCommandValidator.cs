using FluentValidation;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarSaque;

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
            .WithErrorCode("INVALID_VALUE")
            .LessThanOrEqualTo(5000)
            .WithMessage("Valor do saque não pode exceder R$ 5.000,00 por operação")
            .WithErrorCode("INVALID_VALUE");
    }
}