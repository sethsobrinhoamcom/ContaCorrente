using FluentAssertions;
using Moq;
using ContaCorrente.Application.UseCases.ContasCorrentes.Commands.RealizarSaque;
using ContaCorrente.Domain.Entities;
using ContaCorrente.Domain.Events;
using ContaCorrente.Domain.Interfaces;

namespace ContaCorrente.Tests.UseCases;

public class RealizarSaqueTests
{
    private readonly Mock<IContaCorrenteRepository> _contaRepositoryMock;
    private readonly Mock<IIdempotenciaRepository> _idempotenciaRepositoryMock;
    private readonly Mock<ITarifaRepository> _tarifaRepositoryMock;
    private readonly Mock<IEventPublisher> _eventPublisherMock;
    private readonly RealizarSaqueCommandHandler _handler;

    public RealizarSaqueTests()
    {
        _contaRepositoryMock = new Mock<IContaCorrenteRepository>();
        _idempotenciaRepositoryMock = new Mock<IIdempotenciaRepository>();
        _tarifaRepositoryMock = new Mock<ITarifaRepository>();
        _eventPublisherMock = new Mock<IEventPublisher>();

        _handler = new RealizarSaqueCommandHandler(
            _contaRepositoryMock.Object,
            _idempotenciaRepositoryMock.Object,
            _tarifaRepositoryMock.Object,
            _eventPublisherMock.Object);
    }

    [Fact]
    public async Task Handle_QuandoSaldoSuficiente_DeveRealizarSaque()
    {
        // Arrange
        var command = new RealizarSaqueCommand
        {
            IdContaCorrente = "conta-123",
            Valor = 100.00m
        };

        var conta = new ContaCorrenteEntity
        {
            IdContaCorrente = "conta-123",
            Ativo = true
        };

        _contaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(command.IdContaCorrente))
            .ReturnsAsync(conta);

        _contaRepositoryMock
            .Setup(x => x.ObterSaldoAsync(command.IdContaCorrente))
            .ReturnsAsync(200.00m); // Saldo suficiente

        _contaRepositoryMock
            .Setup(x => x.CriarMovimentoAsync(It.IsAny<Movimento>()))
            .ReturnsAsync("movimento-123");

        _tarifaRepositoryMock
            .Setup(x => x.CriarAsync(It.IsAny<Tarifa>()))
            .ReturnsAsync("tarifa-123");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verifica movimento de débito
        _contaRepositoryMock.Verify(
            x => x.CriarMovimentoAsync(It.Is<Movimento>(m =>
                m.TipoMovimento == 'D' &&
                m.Valor == 100.00m)),
            Times.Once);

        // Verifica movimento da tarifa
        _contaRepositoryMock.Verify(
            x => x.CriarMovimentoAsync(It.Is<Movimento>(m =>
                m.TipoMovimento == 'D' &&
                m.Valor == 0.50m)),
            Times.Once);

        // Verifica tarifa criada
        _tarifaRepositoryMock.Verify(
            x => x.CriarAsync(It.Is<Tarifa>(t => t.Valor == 0.50m)),
            Times.Once);

        // Verifica evento publicado
        _eventPublisherMock.Verify(
            x => x.PublishAsync(
                "saques",
                It.IsAny<string>(),
                It.IsAny<SaqueRealizadoEvent>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_QuandoSaldoInsuficiente_DeveRetornarErro()
    {
        // Arrange
        var command = new RealizarSaqueCommand
        {
            IdContaCorrente = "conta-123",
            Valor = 100.00m
        };

        var conta = new ContaCorrenteEntity
        {
            IdContaCorrente = "conta-123",
            Ativo = true
        };

        _contaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(command.IdContaCorrente))
            .ReturnsAsync(conta);

        _contaRepositoryMock
            .Setup(x => x.ObterSaldoAsync(command.IdContaCorrente))
            .ReturnsAsync(50.00m); // Saldo insuficiente (precisa de 100 + 0.50 tarifa)

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailed.Should().BeTrue();
        result.Errors.Should().Contain(e => e.Message.Contains("Saldo insuficiente"));

        _contaRepositoryMock.Verify(
            x => x.CriarMovimentoAsync(It.IsAny<Movimento>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ComIdempotencia_DeveRetornarResultadoAnterior()
    {
        // Arrange
        var command = new RealizarSaqueCommand
        {
            IdContaCorrente = "conta-123",
            Valor = 100.00m,
            ChaveIdempotencia = "chave-existente"
        };

        var idempotenciaExistente = new Idempotencia
        {
            ChaveIdempotencia = "chave-existente",
            Resultado = "movimento-anterior"
        };

        _idempotenciaRepositoryMock
            .Setup(x => x.ObterAsync(command.ChaveIdempotencia))
            .ReturnsAsync(idempotenciaExistente);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("movimento-anterior");

        _contaRepositoryMock.Verify(
            x => x.CriarMovimentoAsync(It.IsAny<Movimento>()),
            Times.Never);
    }
}