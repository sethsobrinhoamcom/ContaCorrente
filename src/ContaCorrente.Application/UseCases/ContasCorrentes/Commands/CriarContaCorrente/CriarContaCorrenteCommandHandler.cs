using FluentResults;
using MediatR;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Enums;
using ContaCorrente.Domain.Exceptions;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Domain.Services;

namespace ContaCorrente.Application.UseCases.ContasCorrentes.Commands.CriarContaCorrente;

public class CriarContaCorrenteCommandHandler : IRequestHandler<CriarContaCorrenteCommand, Result<string>>
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IPasswordService _passwordService;
    private readonly ICpfValidator _cpfValidator;

    public CriarContaCorrenteCommandHandler(
        IContaCorrenteRepository repository,
        IPasswordService passwordService,
        ICpfValidator cpfValidator)
    {
        _repository = repository;
        _passwordService = passwordService;
        _cpfValidator = cpfValidator;
    }

    public async Task<Result<string>> Handle(CriarContaCorrenteCommand request, CancellationToken cancellationToken)
    {
        // Validar CPF
        if (!_cpfValidator.IsValid(request.Cpf))
        {
            throw new DomainException("CPF inválido", ErrorType.INVALID_DOCUMENT);
        }

        var cpfLimpo = _cpfValidator.RemoveMask(request.Cpf);

        // Validar se número já existe
        var contaExistente = await _repository.ObterPorNumeroAsync(request.Numero);
        if (contaExistente != null)
        {
            return Result.Fail<string>("Já existe uma conta com este número");
        }

        // Validar se CPF já existe
        var contaPorCpf = await _repository.ObterPorCpfAsync(cpfLimpo);
        if (contaPorCpf != null)
        {
            return Result.Fail<string>("Já existe uma conta com este CPF");
        }

        // Hash da senha
        var senhaHash = _passwordService.HashPassword(request.Senha, out var salt);

        // Criar conta
        var conta = new ContaCorrenteEntity
        {
            IdContaCorrente = Guid.NewGuid().ToString(),
            Numero = request.Numero,
            Cpf = cpfLimpo,
            Nome = request.Nome,
            Ativo = true,
            Senha = senhaHash,
            Salt = salt
        };

        var id = await _repository.CriarAsync(conta);

        return Result.Ok(id);
    }
}