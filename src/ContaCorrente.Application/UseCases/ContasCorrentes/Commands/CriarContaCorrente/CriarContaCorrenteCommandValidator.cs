using FluentValidation;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.CriarContaCorrente;

public class CriarContaCorrenteCommandValidator : AbstractValidator<CriarContaCorrenteCommand>
{
    public CriarContaCorrenteCommandValidator()
    {
        RuleFor(x => x.Numero)
            .GreaterThan(0)
            .WithMessage("Número da conta deve ser maior que zero");

        RuleFor(x => x.Cpf)
            .NotEmpty()
            .WithMessage("CPF é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty()
            .WithMessage("Nome é obrigatório")
            .MaximumLength(100)
            .WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Senha)
            .NotEmpty()
            .WithMessage("Senha é obrigatória")
            .MinimumLength(6)
            .WithMessage("Senha deve ter no mínimo 6 caracteres");
    }
}