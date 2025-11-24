using FluentValidation;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.InativarContaCorrente;

public class InativarContaCorrenteCommandValidator : AbstractValidator<InativarContaCorrenteCommand>
{
    public InativarContaCorrenteCommandValidator()
    {
        RuleFor(x => x.IdContaCorrente)
            .NotEmpty()
            .WithMessage("ID da conta corrente é obrigatório");

        RuleFor(x => x.Senha)
            .NotEmpty()
            .WithMessage("Senha é obrigatória");
    }
}