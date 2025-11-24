using FluentAssertions;
using Moq;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.CriarContaCorrente;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Interfaces;
using ContaCorrente.Domain.Services;

namespace ContaCorrente.Tests.UseCases;

public class KafkaIntegrationTests
{
    private readonly Mock<IContaCorrenteRepository> _repositoryMock;
    private readonly Mock<IPasswordService> _passwordServiceMock;
    private readonly CriarContaCorrenteCommandHandler _handler;

    public KafkaIntegrationTests()
    {
        _repositoryMock = new Mock<IContaCorrenteRepository>();
        _passwordServiceMock = new Mock<IPasswordService>();
        _handler = new CriarContaCorrenteCommandHandler(
            _repositoryMock.Object,
            _passwordServiceMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoContaNaoExiste_DeveCriarComSucesso()
    {
        // Arrange
        var command = new CriarContaCorrenteCommand
        {
            Numero = 12345,
            Nome = "João Silva",
            Senha = "senha123"
        };

        _repositoryMock
            .Setup(x => x.ObterPorNumeroAsync(command.Numero))
            .ReturnsAsync((ContaCorrenteEntity?)null);

        _passwordServiceMock
            .Setup(x => x.HashPassword(command.Senha, out It.Ref<string>.IsAny))
            .Returns("hashed_password");

        _repositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<ContaCorrenteEntity>()))
            .ReturnsAsync("id-gerado");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("id-gerado");

        _repositoryMock.Verify(x => x.CriarAsync(It.IsAny<ContaCorrenteEntity>()), Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoContaJaExiste_DeveRetornarErro()
    {
        // Arrange
        var command = new CriarContaCorrenteCommand
        {
            Numero = 12345,
            Nome = "João Silva",
            Senha = "senha123"
        };

        var contaExistente = new ContaCorrenteEntity
        {
            IdContaCorrente = "id-existente",
            Numero = 12345,
            Nome = "Maria Santos",
            Ativo = true
        };

        _repositoryMock
            .Setup(x => x.ObterPorNumeroAsync(command.Numero))
            .ReturnsAsync(contaExistente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Já existe uma conta com este número");

        _repositoryMock.Verify(x => x.CriarAsync(It.IsAny<ContaCorrenteEntity>()), Times.Never);
    }
}