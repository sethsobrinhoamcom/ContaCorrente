using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarDeposito;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Enums;
using ContaCorrente.Domain.Events;
using ContaCorrente.Domain.Exceptions;
using ContaCorrente.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ContaCorrente.Tests.UseCases;

public class RealizarDepositoTests
{
    private readonly Mock<IContaCorrenteRepository> _contaRepositoryMock;
    private readonly Mock<IIdempotenciaRepository> _idempotenciaRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly RealizarDepositoCommandHandler _handler;

    public RealizarDepositoTests()
    {
        _contaRepositoryMock = new Mock<IContaCorrenteRepository>();
        _idempotenciaRepositoryMock = new Mock<IIdempotenciaRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _handler = new RealizarDepositoCommandHandler(
            _contaRepositoryMock.Object,
            _idempotenciaRepositoryMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoContaValida_DeveRealizarDeposito()
    {
        // Arrange
        var command = new RealizarDepositoCommand
        {
            IdContaCorrente = "conta-123",
            Valor = 100.00m,
            ChaveIdempotencia = "chave-123"
        };

        var conta = new ContaCorrenteEntity
        {
            IdContaCorrente = "conta-123",
            Numero = 12345,
            Nome = "João Silva",
            Ativo = true
        };

        _idempotenciaRepositoryMock
            .Setup(x => x.ObterAsync(command.ChaveIdempotencia))
            .ReturnsAsync((Idempotencia?)null);

        _contaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(command.IdContaCorrente))
            .ReturnsAsync(conta);

        _contaRepositoryMock
            .Setup(x => x.CriarMovimentoAsync(It.IsAny<Movimento>()))
            .ReturnsAsync("movimento-123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _contaRepositoryMock.Verify(
            x => x.CriarMovimentoAsync(It.Is<Movimento>(m =>
                m.TipoMovimento == 'C' &&
                m.Valor == 100.00m)),
            Times.Once);

        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                "depositos",
                It.IsAny<string>(),
                It.IsAny<DepositoRealizadoEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _idempotenciaRepositoryMock.Verify(
            x => x.SalvarAsync(It.IsAny<Idempotencia>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoContaInativa_DeveLancarExcecao()
    {
        // Arrange
        var command = new RealizarDepositoCommand
        {
            IdContaCorrente = "conta-123",
            Valor = 100.00m
        };

        var conta = new ContaCorrenteEntity
        {
            IdContaCorrente = "conta-123",
            Ativo = false
        };

        _contaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(command.IdContaCorrente))
            .ReturnsAsync(conta);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorType == ErrorType.INACTIVE_ACCOUNT);

        _contaRepositoryMock.Verify(
            x => x.CriarMovimentoAsync(It.IsAny<Movimento>()),
            Times.Never);

        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public async Task Handle_QuandoValorInvalido_DeveRetornarErro(decimal valor)
    {
        // Arrange
        var command = new RealizarDepositoCommand
        {
            IdContaCorrente = "conta-123",
            Valor = valor
        };

        var validator = new RealizarDepositoCommandValidator();

        // Act
        var validationResult = await validator.ValidateAsync(command);

        // Assert
        validationResult.IsValid.Should().BeFalse();
        validationResult.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("maior que zero"));
    }
}