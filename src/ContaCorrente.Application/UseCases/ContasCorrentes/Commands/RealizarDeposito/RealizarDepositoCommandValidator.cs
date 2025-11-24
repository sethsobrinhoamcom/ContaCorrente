using FluentValidation;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarDeposito;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarDeposito;

public class RealizarDepositoCommandValidator : AbstractValidator<RealizarDepositoCommand>
{
    public RealizarDepositoCommandValidator()
    {
        RuleFor(x => x.IdContaCorrente)
            .NotEmpty()
            .WithMessage("ID da conta corrente é obrigatório");

        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("Valor do depósito deve ser maior que zero")
            .WithErrorCode("INVALID_VALUE")
            .LessThanOrEqualTo(10000)
            .WithMessage("Valor do depósito não pode exceder R$ 10.000,00 por operação")
            .WithErrorCode("INVALID_VALUE");
    }
}