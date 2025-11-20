using FluentValidation;

namespace ContaCorrente.Application.UseCases.Transferencias.Commands.RealizarTransferencia;

public class RealizarTransferenciaCommandValidator : AbstractValidator<RealizarTransferenciaCommand>
{
    public RealizarTransferenciaCommandValidator()
    {
        RuleFor(x => x.IdContaCorrenteOrigem)
            .NotEmpty()
            .WithMessage("ID da conta de origem é obrigatório");

        RuleFor(x => x.IdContaCorrenteDestino)
            .NotEmpty()
            .WithMessage("ID da conta de destino é obrigatório");

        RuleFor(x => x.Valor)
            .GreaterThan(0)
            .WithMessage("Valor deve ser maior que zero");

        RuleFor(x => x)
            .Must(x => x.IdContaCorrenteOrigem != x.IdContaCorrenteDestino)
            .WithMessage("Conta de origem e destino devem ser diferentes");
    }
}