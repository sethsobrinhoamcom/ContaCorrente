using FluentAssertions;
using Moq;
using ContaCorrente.Application.UseCases.Auth.Commands.Login;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Domain.Services;
using ContaCorrente.Domain.Exceptions;
using ContaCorrente.Domain.Enums;

namespace ContaCorrente.Tests.UseCases;

public class LoginTests
{
    private readonly Mock<IContaCorrenteRepository> _repositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly Mock<ICpfValidator> _cpfValidatorMock;
    private readonly LoginCommandHandler _handler;

    public LoginTests()
    {
        _repositoryMock = new Mock<IContaCorrenteRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _jwtServiceMock = new Mock<IJwtService>();
        _cpfValidatorMock = new Mock<ICpfValidator>();

        _handler = new LoginCommandHandler(
            _repositoryMock.Object,
            _passwordServiceMock.Object,
            _jwtServiceMock.Object,
            _cpfValidatorMock.Object);
    }

    [Fact]
    public async Task Handle_ComCredenciaisValidas_DeveRetornarToken()
    {
        // Arrange
        var command = new LoginCommand
        {
            CpfOrNumeroConta = "12345678901",
            Senha = "senha123"
        };

        var conta = new ContaCorrenteEntity
        {
            IdContaCorrente = "conta-123",
            Numero = 12345,
            Cpf = "12345678901",
            Nome = "João Silva",
            Ativo = true,
            Senha = "hashed_password",
            Salt = "salt"
        };

        _cpfValidatorMock
            .Setup(x => x.IsValid("12345678901"))
            .Returns(true);

        _cpfValidatorMock
            .Setup(x => x.RemoveMask("12345678901"))
            .Returns("12345678901");

        _repositoryMock
            .Setup(x => x.ObterPorCpfAsync("12345678901"))
            .ReturnsAsync(conta);

        _passwordServiceMock
            .Setup(x => x.VerifyPassword("senha123", "hashed_password", "salt"))
            .Returns(true);

        _jwtServiceMock
            .Setup(x => x.GenerateToken("conta-123", "12345", "12345678901"))
            .Returns("token_jwt_valido");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Token.Should().Be("token_jwt_valido");
        result.Value.IdContaCorrente.Should().Be("conta-123");
        result.Value.Nome.Should().Be("João Silva");
    }

    [Fact]
    public async Task Handle_ComSenhaInvalida_DeveLancarExcecao()
    {
        // Arrange
        var command = new LoginCommand
        {
            CpfOrNumeroConta = "12345678901",
            Senha = "senha_errada"
        };

        var conta = new ContaCorrenteEntity
        {
            IdContaCorrente = "conta-123",
            Cpf = "12345678901",
            Senha = "hashed_password",
            Salt = "salt"
        };

        _cpfValidatorMock
            .Setup(x => x.IsValid("12345678901"))
            .Returns(true);

        _cpfValidatorMock
            .Setup(x => x.RemoveMask("12345678901"))
            .Returns("12345678901");

        _repositoryMock
            .Setup(x => x.ObterPorCpfAsync("12345678901"))
            .ReturnsAsync(conta);

        _passwordServiceMock
            .Setup(x => x.VerifyPassword("senha_errada", "hashed_password", "salt"))
            .Returns(false);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorType == ErrorType.USER_UNAUTHORIZED);
    }

    [Fact]
    public async Task Handle_ComContaNaoEncontrada_DeveLancarExcecao()
    {
        // Arrange
        var command = new LoginCommand
        {
            CpfOrNumeroConta = "99999999999",
            Senha = "senha123"
        };

        _cpfValidatorMock
            .Setup(x => x.IsValid("99999999999"))
            .Returns(true);

        _cpfValidatorMock
            .Setup(x => x.RemoveMask("99999999999"))
            .Returns("99999999999");

        _repositoryMock
            .Setup(x => x.ObterPorCpfAsync("99999999999"))
            .ReturnsAsync((ContaCorrenteEntity?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorType == ErrorType.USER_UNAUTHORIZED);
    }
}