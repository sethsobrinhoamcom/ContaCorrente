using FluentResults;
using MediatR;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Domain.Services;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.CriarContaCorrente;

public class CriarContaCorrenteCommandHandler : IRequestHandler<CriarContaCorrenteCommand, Result<string>>
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IPasswordService _passwordService;

    public CriarContaCorrenteCommandHandler(
        IContaCorrenteRepository repository,
        IPasswordService passwordService)
    {
        _repository = repository;
        _passwordService = passwordService;
    }

    public async Task<Result<string>> Handle(CriarContaCorrenteCommand request, CancellationToken cancellationToken)
    {
        // Validar se número já existe
        var contaExistente = await _repository.ObterPorNumeroAsync(request.Numero);
        if (contaExistente != null)
        {
            return Result.Fail<string>("Já existe uma conta com este número");
        }

        // Hash da senha
        var senhaHash = _passwordService.HashPassword(request.Senha, out var salt);

        // Criar conta
        var conta = new ContaCorrenteEntity
        {
            IdContaCorrente = Guid.NewGuid().ToString(),
            Numero = request.Numero,
            Nome = request.Nome,
            Ativo = true,
            Senha = senhaHash,
            Salt = salt
        };

        var id = await _repository.CriarAsync(conta);

        return Result.Ok(id);
    }
}