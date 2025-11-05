namespace AlgoTrading.Core.Interfaces.External;

/// <summary>
/// description : 다중 채널 알림 전송 서비스 인터페이스
/// Details : Monitoring Context에서 사용자에게 시스템 알림, 리스크 경고, 거래 체결 알림을 다양한 채널(Email, SMS, Telegram, Slack, Push)을 통해 전송합니다. AlertSeverity에 따라 전송 채널을 자동 선택하며, Critical 알림은 모든 채널로 즉시 발송됩니다. Infrastructure Layer에서 외부 알림 서비스 API(SendGrid, Twilio, Telegram Bot API)와 통합하여 구현되며, 알림 전송 실패 시 재시도 로직을 수행합니다. Domain Event Handler에서 RiskLimitViolatedEvent나 OrderFilledEvent를 수신하면 이 서비스를 호출하여 사용자에게 알립니다.
/// Applied technology patterns : Adapter Pattern, Multi-Channel Pattern, Observer Pattern
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 이메일 전송
    /// </summary>
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);

    /// <summary>
    /// SMS 전송
    /// </summary>
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 텔레그램 메시지 전송
    /// </summary>
    Task SendTelegramMessageAsync(string chatId, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 슬랙 메시지 전송
    /// </summary>
    Task SendSlackMessageAsync(string channel, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 푸시 알림 전송
    /// </summary>
    Task SendPushNotificationAsync(string userId, string title, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// 알림 전송 (모든 채널)
    /// </summary>
    Task SendNotificationAsync(string userId, string title, string message, IEnumerable<NotificationChannel> channels, CancellationToken cancellationToken = default);
}

/// <summary>
/// 알림 채널
/// </summary>
public enum NotificationChannel
{
    Email = 1,
    Sms = 2,
    Telegram = 3,
    Slack = 4,
    Push = 5
}
