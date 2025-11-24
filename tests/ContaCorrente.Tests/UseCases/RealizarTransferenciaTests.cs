using ContaCorrente.Application.UseCases.Transferencias.Commands.RealizarTransferencia;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Enums;
using ContaCorrente.Domain.Exceptions;
using ContaCorrente.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace ContaCorrente.Tests.UseCases;

public class RealizarTransferenciaTests
{
    private readonly Mock<IContaCorrenteRepository> _contaRepositoryMock;
    private readonly Mock<ITransferenciaRepository> _transferenciaRepositoryMock;
    private readonly Mock<IIdempotenciaRepository> _idempotenciaRepositoryMock;
    private readonly Mock<ITarifaRepository> _tarifaRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock; // ADICIONAR
    private readonly RealizarTransferenciaCommandHandler _handler;

    public RealizarTransferenciaTests()
    {
        _contaRepositoryMock = new Mock<IContaCorrenteRepository>();
        _transferenciaRepositoryMock = new Mock<ITransferenciaRepository>();
        _idempotenciaRepositoryMock = new Mock<IIdempotenciaRepository>();
        _tarifaRepositoryMock = new Mock<ITarifaRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>(); // ADICIONAR

        _handler = new RealizarTransferenciaCommandHandler(
            _contaRepositoryMock.Object,
            _transferenciaRepositoryMock.Object,
            _idempotenciaRepositoryMock.Object,
            _tarifaRepositoryMock.Object,
            _eventPublisherMock.Object); // ADICIONAR
    }

    [Fact]
    public async Task Handle_QuandoSaldoSuficiente_DeveRealizarTransferencia()
    {
        // Arrange
        var command = new RealizarTransferenciaCommand
        {
            IdContaCorrenteOrigem = "origem-id",
            IdContaCorrenteDestino = "destino-id",
            Valor = 100m,
            ChaveIdempotencia = "chave-123"
        };

        var contaOrigem = new ContaCorrenteEntity
        {
            IdContaCorrente = "origem-id",
            Numero = 1,
            Nome = "João",
            Ativo = true
        };

        var contaDestino = new ContaCorrenteEntity
        {
            IdContaCorrente = "destino-id",
            Numero = 2,
            Nome = "Maria",
            Ativo = true
        };

        _idempotenciaRepositoryMock
            .Setup(x => x.ObterAsync(command.ChaveIdempotencia))
            .ReturnsAsync((Idempotencia?)null);

        _contaRepositoryMock
            .Setup(x => x.ObterPorIdAsync("origem-id"))
            .ReturnsAsync(contaOrigem);

        _contaRepositoryMock
            .Setup(x => x.ObterPorIdAsync("destino-id"))
            .ReturnsAsync(contaDestino);

        _contaRepositoryMock
            .Setup(x => x.ObterSaldoAsync("origem-id"))
            .ReturnsAsync(200m); // Saldo suficiente

        _contaRepositoryMock
            .Setup(x => x.CriarMovimentoAsync(It.IsAny<Movimento>()))
            .ReturnsAsync("movimento-123");

        _transferenciaRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Transferencia>()))
            .ReturnsAsync("transferencia-id");

        _tarifaRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Tarifa>()))
            .ReturnsAsync("tarifa-123");

        // ADICIONAR: Mock do EventPublisher
        _eventPublisherMock
            .Setup(x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        _transferenciaRepositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<Transferencia>()),
            Times.Once);

        _tarifaRepositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<Tarifa>()),
            Times.Once);

        // ADICIONAR: Verificar que o evento foi publicado
        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                "transferencias",
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoSaldoInsuficiente_DeveRetornarErro()
    {
        // Arrange
        var command = new RealizarTransferenciaCommand
        {
            IdContaCorrenteOrigem = "origem-id",
            IdContaCorrenteDestino = "destino-id",
            Valor = 100m
        };

        var contaOrigem = new ContaCorrenteEntity
        {
            IdContaCorrente = "origem-id",
            Ativo = true
        };

        var contaDestino = new ContaCorrenteEntity
        {
            IdContaCorrente = "destino-id",
            Ativo = true
        };

        _contaRepositoryMock
            .Setup(x => x.ObterPorIdAsync("origem-id"))
            .ReturnsAsync(contaOrigem);

        _contaRepositoryMock
            .Setup(x => x.ObterPorIdAsync("destino-id"))
            .ReturnsAsync(contaDestino);

        _contaRepositoryMock
            .Setup(x => x.ObterSaldoAsync("origem-id"))
            .ReturnsAsync(50m); // Saldo insuficiente

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message == "Saldo insuficiente");

        _transferenciaRepositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<Transferencia>()),
            Times.Never);

        // Evento não deve ser publicado em caso de erro
        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ComIdempotencia_DeveRetornarResultadoAnterior()
    {
        // Arrange
        var command = new RealizarTransferenciaCommand
        {
            IdContaCorrenteOrigem = "origem-id",
            IdContaCorrenteDestino = "destino-id",
            Valor = 100m,
            ChaveIdempotencia = "chave-existente"
        };

        var idempotenciaExistente = new Idempotencia
        {
            ChaveIdempotencia = "chave-existente",
            Resultado = "transferencia-anterior-id"
        };

        _idempotenciaRepositoryMock
            .Setup(x => x.ObterAsync(command.ChaveIdempotencia))
            .ReturnsAsync(idempotenciaExistente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("transferencia-anterior-id");

        _transferenciaRepositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<Transferencia>()),
            Times.Never);

        // Evento não deve ser publicado quando usa idempotência
        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_QuandoContaOrigemInativa_DeveLancarExcecao()
    {
        // Arrange
        var command = new RealizarTransferenciaCommand
        {
            IdContaCorrenteOrigem = "origem-id",
            IdContaCorrenteDestino = "destino-id",
            Valor = 100m
        };

        var contaOrigem = new ContaCorrenteEntity
        {
            IdContaCorrente = "origem-id",
            Ativo = false // INATIVA
        };

        _contaRepositoryMock
            .Setup(x => x.ObterPorIdAsync("origem-id"))
            .ReturnsAsync(contaOrigem);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .Where(e => e.ErrorType == ErrorType.INACTIVE_ACCOUNT);

        _transferenciaRepositoryMock.Verify(
            x => x.CriarAsync(It.IsAny<Transferencia>()),
            Times.Never);

        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}