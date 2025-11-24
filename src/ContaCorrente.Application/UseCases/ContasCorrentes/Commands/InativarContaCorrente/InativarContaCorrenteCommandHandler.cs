using FluentResults;
using MediatR;
using ContaCorrente.Domain.Enums;
using ContaCorrente.Domain.Exceptions;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Domain.Services;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.InativarContaCorrente;

public class InativarContaCorrenteCommandHandler : IRequestHandler<InativarContaCorrenteCommand, Result>
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IPasswordService _passwordService;

    public InativarContaCorrenteCommandHandler(
        IContaCorrenteRepository repository,
        IPasswordService passwordService)
    {
        _repository = repository;
        _passwordService = passwordService;
    }

    public async Task<Result> Handle(InativarContaCorrenteCommand request, CancellationToken cancellationToken)
    {
        // Buscar conta
        var conta = await _repository.ObterPorIdAsync(request.IdContaCorrente);

        if (conta == null)
        {
            throw new DomainException("Conta corrente não encontrada", ErrorType.INVALID_ACCOUNT);
        }

        // Validar senha
        var senhaValida = _passwordService.VerifyPassword(request.Senha, conta.Senha, conta.Salt);

        if (!senhaValida)
        {
            throw new DomainException("Senha inválida", ErrorType.USER_UNAUTHORIZED);
        }

        // Inativar conta
        conta.Ativo = false;
        await _repository.AtualizarAsync(conta);

        return Result.Ok();
    }
}