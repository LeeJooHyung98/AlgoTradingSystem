namespace AlgoTrading.Core.Interfaces.External;

/// <summary>
/// description : 비동기 메시지 발행/구독 및 큐 관리 서비스 인터페이스
/// Details : Infrastructure Layer에서 RabbitMQ를 사용하여 도메인 이벤트를 비동기적으로 전파하고, Bounded Context 간 통신을 중재합니다. Publish/Subscribe 패턴으로 도메인 이벤트(OrderFilledEvent, SignalGeneratedEvent 등)를 여러 구독자에게 브로드캐스트하며, Point-to-Point 큐로 작업 메시지(백테스트 실행 요청 등)를 전달합니다. Application Layer의 Domain Event Handler는 이 인터페이스를 통해 이벤트를 발행하고, Infrastructure Layer의 Event Subscriber는 이벤트를 구독하여 사이드 이펙트(알림 전송, 캐시 무효화)를 처리합니다.
/// Applied technology patterns : Message Broker Pattern, Publish-Subscribe Pattern, Event-Driven Architecture
/// </summary>
public interface IMessageBus
{
    /// <summary>
    /// 이벤트 발행
    /// </summary>
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class;

    /// <summary>
    /// 이벤트 구독
    /// </summary>
    Task SubscribeAsync<TEvent>(Func<TEvent, Task> handler, CancellationToken cancellationToken = default) where TEvent : class;

    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    Task UnsubscribeAsync<TEvent>(CancellationToken cancellationToken = default) where TEvent : class;

    /// <summary>
    /// 큐에 메시지 전송
    /// </summary>
    Task SendAsync<TMessage>(string queueName, TMessage message, CancellationToken cancellationToken = default) where TMessage : class;

    /// <summary>
    /// 큐에서 메시지 수신
    /// </summary>
    Task<TMessage?> ReceiveAsync<TMessage>(string queueName, CancellationToken cancellationToken = default) where TMessage : class;
}
