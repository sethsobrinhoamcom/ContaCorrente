using Microsoft.Extensions.Logging;

namespace ContaCorrente.Infrastructure.Services;

public interface INotificationService
{
    Task EnviarNotificacaoDepositoAsync(string idConta, decimal valor);
    Task EnviarNotificacaoSaqueAsync(string idConta, decimal valor);
    Task EnviarNotificacaoTransferenciaAsync(string idContaOrigem, string idContaDestino, decimal valor);
}

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task EnviarNotificacaoDepositoAsync(string idConta, decimal valor)
    {
        // Implementar lógica de envio (Email, SMS, Push, etc)
        _logger.LogInformation("Notificação de depósito enviada: Conta {IdConta}, Valor {Valor}", idConta, valor);
        await Task.CompletedTask;
    }

    public async Task EnviarNotificacaoSaqueAsync(string idConta, decimal valor)
    {
        _logger.LogInformation("Notificação de saque enviada: Conta {IdConta}, Valor {Valor}", idConta, valor);
        await Task.CompletedTask;
    }

    public async Task EnviarNotificacaoTransferenciaAsync(string idContaOrigem, string idContaDestino, decimal valor)
    {
        _logger.LogInformation(
            "Notificação de transferência enviada: Origem {Origem}, Destino {Destino}, Valor {Valor}",
            idContaOrigem, idContaDestino, valor);
        await Task.CompletedTask;
    }
}