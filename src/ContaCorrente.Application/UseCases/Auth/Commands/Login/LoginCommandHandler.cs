using FluentResults;
using MediatR;
using ContaCorrente.Domain.Enums;
using ContaCorrente.Domain.Exceptions;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Domain.Services;

namespace ContaCorrente.Application.UseCases.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IContaCorrenteRepository _repository;
    private readonly IPasswordService _passwordService;
    private readonly IJwtService _jwtService;
    private readonly ICpfValidator _cpfValidator;

    public LoginCommandHandler(
        IContaCorrenteRepository repository,
        IPasswordService passwordService,
        IJwtService jwtService,
        ICpfValidator cpfValidator)
    {
        _repository = repository;
        _passwordService = passwordService;
        _jwtService = jwtService;
        _cpfValidator = cpfValidator;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Tenta buscar por CPF ou número da conta
        var conta = await ObterContaAsync(request.CpfOrNumeroConta);

        if (conta == null)
        {
            throw new DomainException("Usuário ou senha inválidos", ErrorType.USER_UNAUTHORIZED);
        }

        // Validar senha
        var senhaValida = _passwordService.VerifyPassword(request.Senha, conta.Senha, conta.Salt);

        if (!senhaValida)
        {
            throw new DomainException("Usuário ou senha inválidos", ErrorType.USER_UNAUTHORIZED);
        }

        // Gerar token
        var token = _jwtService.GenerateToken(
            conta.IdContaCorrente,
            conta.Numero.ToString(),
            conta.Cpf);

        var response = new LoginResponse
        {
            Token = token,
            IdContaCorrente = conta.IdContaCorrente,
            NumeroConta = conta.Numero.ToString(),
            Nome = conta.Nome
        };

        return Result.Ok(response);
    }

    private async Task<Domain.Entities.ContaCorrenteEntity?> ObterContaAsync(string cpfOrNumeroConta)
    {
        // Verifica se é CPF (contém apenas números e tem 11 dígitos)
        var apenasNumeros = new string(cpfOrNumeroConta.Where(char.IsDigit).ToArray());

        if (_cpfValidator.IsValid(apenasNumeros) && apenasNumeros.Length == 11)
        {
            var cpfLimpo = _cpfValidator.RemoveMask(cpfOrNumeroConta);
            return await _repository.ObterPorCpfAsync(cpfLimpo);
        }

        // Tenta converter para número da conta
        if (int.TryParse(apenasNumeros, out var numeroConta))
        {
            return await _repository.ObterPorNumeroAsync(numeroConta);
        }

        return null;
    }
}