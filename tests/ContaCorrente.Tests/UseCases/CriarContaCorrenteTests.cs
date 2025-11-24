using FluentAssertions;
using Moq;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.CriarContaCorrente;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Domain.Services;

namespace ContaCorrente.Tests.UseCases;

public class CriarContaCorrenteTests
{
    private readonly Mock<IContaCorrenteRepository> _repositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly Mock<ICpfValidator> _cpfValidatorMock; // ADICIONAR
    private readonly CriarContaCorrenteCommandHandler _handler;

    public CriarContaCorrenteTests()
    {
        _repositoryMock = new Mock<IContaCorrenteRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _cpfValidatorMock = new Mock<ICpfValidator>(); // ADICIONAR

        _handler = new CriarContaCorrenteCommandHandler(
            _repositoryMock.Object,
            _passwordServiceMock.Object,
            _cpfValidatorMock.Object); // ADICIONAR
    }

    [Fact]
    public async Task Handle_QuandoContaNaoExiste_DeveCriarComSucesso()
    {
        // Arrange
        var command = new CriarContaCorrenteCommand
        {
            Numero = 123,
            Cpf = "12345678901", // ADICIONAR
            Nome = "João Silva",
            Senha = "senha123"
        };

        // ADICIONAR: Mock do CPF Validator
        _cpfValidatorMock
            .Setup(x => x.IsValid("12345678901"))
            .Returns(true);

        _cpfValidatorMock
            .Setup(x => x.RemoveMask("12345678901"))
            .Returns("12345678901");

        _repositoryMock
            .Setup(x => x.ObterPorNumeroAsync(123))
            .ReturnsAsync((ContaCorrenteEntity?)null);

        // ADICIONAR: Mock para ObterPorCpfAsync
        _repositoryMock
            .Setup(x => x.ObterPorCpfAsync("12345678901"))
            .ReturnsAsync((ContaCorrenteEntity?)null);

        _passwordServiceMock
            .Setup(x => x.HashPassword(It.IsAny<string>(), out It.Ref<string>.IsAny))
            .Returns((string password, out string salt) =>
            {
                salt = "test-salt";
                return "hashed-password";
            });

        _repositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<ContaCorrenteEntity>()))
            .ReturnsAsync("novo-id");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("novo-id");

        _repositoryMock.Verify(
            x => x.CriarAsync(It.Is<ContaCorrenteEntity>(c =>
                c.Numero == 123 &&
                c.Nome == "João Silva" &&
                c.Cpf == "12345678901" &&
                c.Ativo == true)),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoContaJaExiste_DeveRetornarErro()
    {
        // Arrange
        var command = new CriarContaCorrenteCommand
        {
            Numero = 123,
            Cpf = "12345678901",
            Nome = "João Silva",
            Senha = "senha123"
        };

        var contaExistente = new ContaCorrenteEntity
        {
            IdContaCorrente = "id-existente",
            Numero = 123,
            Nome = "Outra Pessoa"
        };

        _cpfValidatorMock
            .Setup(x => x.IsValid("12345678901"))
            .Returns(true);

        _cpfValidatorMock
            .Setup(x => x.RemoveMask("12345678901"))
            .Returns("12345678901");

        _repositoryMock
            .Setup(x => x.ObterPorNumeroAsync(123))
            .ReturnsAsync(contaExistente);

        // ADICIONAR: Mock para ObterPorCpfAsync (evitar null reference)
        _repositoryMock
            .Setup(x => x.ObterPorCpfAsync("12345678901"))
            .ReturnsAsync((ContaCorrenteEntity?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - O handler retorna Result.Fail, não lança exceção
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().NotBeEmpty();

        _repositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<ContaCorrenteEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoCpfInvalido_DeveLancarExcecao()
    {
        // Arrange
        var command = new CriarContaCorrenteCommand
        {
            Numero = 123,
            Cpf = "11111111111", // CPF inválido
            Nome = "João Silva",
            Senha = "senha123"
        };

        _cpfValidatorMock
            .Setup(x => x.IsValid("11111111111"))
            .Returns(false); // CPF inválido

        // Act & Assert
        await Assert.ThrowsAsync<Domain.Exceptions.DomainException>(
            async () => await _handler.Handle(command, CancellationToken.None));

        _repositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<ContaCorrenteEntity>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoCpfJaExiste_DeveRetornarErro()
    {
        // Arrange
        var command = new CriarContaCorrenteCommand
        {
            Numero = 123,
            Cpf = "12345678901",
            Nome = "João Silva",
            Senha = "senha123"
        };

        var contaComCpf = new ContaCorrenteEntity
        {
            IdContaCorrente = "id-existente",
            Numero = 456,
            Cpf = "12345678901",
            Nome = "Outra Pessoa"
        };

        _cpfValidatorMock
            .Setup(x => x.IsValid("12345678901"))
            .Returns(true);

        _cpfValidatorMock
            .Setup(x => x.RemoveMask("12345678901"))
            .Returns("12345678901");

        _repositoryMock
            .Setup(x => x.ObterPorNumeroAsync(123))
            .ReturnsAsync((ContaCorrenteEntity?)null);

        _repositoryMock
            .Setup(x => x.ObterPorCpfAsync("12345678901"))
            .ReturnsAsync(contaComCpf); // CPF já existe

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("CPF"));

        _repositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<ContaCorrenteEntity>()),
            Times.Never);
    }
}