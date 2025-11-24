namespace ContaCorrente.Domain.Interfaces;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(string topic, TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
    Task PublishAsync<TEvent>(string topic, string key, TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;
}