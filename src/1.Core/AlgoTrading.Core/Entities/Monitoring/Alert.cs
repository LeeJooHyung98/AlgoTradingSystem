using AlgoTrading.Core.Entities.Portfolio;
using AlgoTrading.Core.Entities.Strategy;
using AlgoTrading.Core.Entities.Users;
using AlgoTrading.Core.Enums;
using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Monitoring;

/// <summary>
/// description(설명) : Alert entity for system, trading, and risk alerts (시스템, 거래, 리스크 알림을 위한 알림 엔티티)
/// Details(상세설명) : Manages alert lifecycle from triggered to acknowledged with notification channels (발생부터 확인까지의 알림 생명주기 및 알림 채널 관리)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, State Machine Pattern, Observer Pattern (엔티티 패턴, 상태 머신 패턴, 옵저버 패턴)
/// </summary>
public sealed class Alert : Entity<AlertId>
{
    /// <summary>
    /// 알림 타입 (System, Trading, Risk, Performance)
    /// </summary>
    public AlertType Type { get; private set; }

    /// <summary>
    /// 심각도 (Info, Medium, High, Critical)
    /// </summary>
    public AlertSeverity Severity { get; private set; }

    /// <summary>
    /// 알림 제목
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// 알림 상세 메시지
    /// </summary>
    public string Message { get; private set; }

    /// <summary>
    /// 알림 소스 (시스템 컴포넌트 또는 서비스 이름)
    /// </summary>
    public string Source { get; private set; }

    /// <summary>
    /// 계좌 ID (거래/리스크 알림의 경우)
    /// </summary>
    public AccountId? AccountId { get; private set; }

    /// <summary>
    /// 전략 ID (전략 관련 알림의 경우)
    /// </summary>
    public StrategyId? StrategyId { get; private set; }

    /// <summary>
    /// 알림 상태 (Triggered, Sent, Acknowledged, Resolved)
    /// </summary>
    public AlertStatus Status { get; private set; }

    /// <summary>
    /// 알림 채널 (Email, SMS, Telegram, Slack)
    /// </summary>
    public NotificationChannel Channel { get; private set; }

    /// <summary>
    /// 알림 수신 대상 (이메일, 전화번호, 사용자 ID 등)
    /// </summary>
    public string? RecipientTarget { get; private set; }

    /// <summary>
    /// 알림 발생 횟수 (중복 알림 카운트)
    /// </summary>
    public int OccurrenceCount { get; private set; }

    /// <summary>
    /// 알림 발생 일시
    /// </summary>
    public DateTime TriggeredAt { get; private set; }

    /// <summary>
    /// 알림 전송 일시
    /// </summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>
    /// 알림 확인 일시
    /// </summary>
    public DateTime? AcknowledgedAt { get; private set; }

    /// <summary>
    /// 알림 해결 일시
    /// </summary>
    public DateTime? ResolvedAt { get; private set; }

    /// <summary>
    /// 알림 확인자 (사용자 ID)
    /// </summary>
    public UserId? AcknowledgedBy { get; private set; }

    /// <summary>
    /// 해결 메모
    /// </summary>
    public string? ResolutionNotes { get; private set; }

    /// <summary>
    /// 다음 알림 허용 시각 (중복 방지용)
    /// </summary>
    public DateTime? NextAllowedAlertTime { get; private set; }

    /// <summary>
    /// 데이터 생성 일시
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    private Alert() { }

    private Alert(
        AlertId id,
        AlertType type,
        AlertSeverity severity,
        string title,
        string message,
        string source,
        NotificationChannel channel)
    {
        Id = id;
        Type = type;
        Severity = severity;
        Title = title;
        Message = message;
        Source = source;
        Channel = channel;
        Status = AlertStatus.Triggered;
        OccurrenceCount = 1;
        TriggeredAt = DateTime.UtcNow;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// description(설명) : Create alert (알림 생성)
    /// Details(상세설명) : Factory method to create a new alert (새로운 알림을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static Alert Create(
        AlertType type,
        AlertSeverity severity,
        string title,
        string message,
        string source,
        NotificationChannel channel,
        AccountId? accountId = null,
        StrategyId? strategyId = null,
        string? recipientTarget = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Message cannot be empty", nameof(message));

        if (string.IsNullOrWhiteSpace(source))
            throw new ArgumentException("Source cannot be empty", nameof(source));

        var id = AlertId.New();
        var alert = new Alert(id, type, severity, title, message, source, channel);

        if (accountId != null)
            alert.AccountId = accountId;

        if (strategyId != null)
            alert.StrategyId = strategyId;

        if (!string.IsNullOrWhiteSpace(recipientTarget))
            alert.RecipientTarget = recipientTarget;

        return alert;
    }

    /// <summary>
    /// description(설명) : Mark alert as sent (알림을 전송됨으로 표시)
    /// Details(상세설명) : Changes status to Sent after notification is delivered (알림 전달 후 상태를 전송됨으로 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void MarkAsSent()
    {
        if (Status != AlertStatus.Triggered)
            throw new InvalidOperationException($"Cannot mark alert as sent in {Status} status");

        Status = AlertStatus.Sent;
        SentAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Acknowledge alert (알림 확인)
    /// Details(상세설명) : User acknowledges receiving the alert (사용자가 알림 수신을 확인)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Acknowledge(UserId acknowledgedBy)
    {
        if (Status == AlertStatus.Resolved)
            throw new InvalidOperationException("Cannot acknowledge resolved alert");

        Status = AlertStatus.Acknowledged;
        AcknowledgedAt = DateTime.UtcNow;
        AcknowledgedBy = acknowledgedBy;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Resolve alert (알림 해결)
    /// Details(상세설명) : Marks the alert as resolved with optional notes (알림을 해결됨으로 표시하고 선택적 메모 추가)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Resolve(string? resolutionNotes = null)
    {
        if (Status == AlertStatus.Resolved)
            throw new InvalidOperationException("Alert is already resolved");

        Status = AlertStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        ResolutionNotes = resolutionNotes;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Increment occurrence count (발생 횟수 증가)
    /// Details(상세설명) : Increments count when same alert triggers again (동일한 알림이 다시 발생할 때 횟수 증가)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void IncrementOccurrence()
    {
        OccurrenceCount++;
        TriggeredAt = DateTime.UtcNow; // Update to latest trigger time
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Set rate limit (알림 빈도 제한 설정)
    /// Details(상세설명) : Prevents duplicate alerts within specified time window (지정된 시간 동안 중복 알림 방지)
    /// Applied technology patterns(적용기술패턴) : Rate Limiting Pattern (속도 제한 패턴)
    /// </summary>
    public void SetRateLimit(int minutes)
    {
        if (minutes <= 0)
            throw new ArgumentException("Rate limit minutes must be positive", nameof(minutes));

        NextAllowedAlertTime = DateTime.UtcNow.AddMinutes(minutes);
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if alert can be sent (알림을 전송할 수 있는지 확인)
    /// Details(상세설명) : Returns false if within rate limit window (속도 제한 기간 내이면 false 반환)
    /// </summary>
    public bool CanSendAlert()
    {
        if (!NextAllowedAlertTime.HasValue)
            return true;

        return DateTime.UtcNow >= NextAllowedAlertTime.Value;
    }

    /// <summary>
    /// description(설명) : Check if alert requires immediate action (즉각적인 조치가 필요한지 확인)
    /// Details(상세설명) : Returns true if severity is critical and unacknowledged (심각도가 위험이고 미확인이면 true 반환)
    /// </summary>
    public bool RequiresImmediateAction()
    {
        return Severity == AlertSeverity.Critical &&
               (Status == AlertStatus.Triggered || Status == AlertStatus.Sent);
    }

    /// <summary>
    /// description(설명) : Check if alert is stale (알림이 오래되었는지 확인)
    /// Details(상세설명) : Returns true if alert is unresolved for more than specified hours (지정된 시간 이상 미해결이면 true 반환)
    /// </summary>
    public bool IsStale(int hoursThreshold = 24)
    {
        if (Status == AlertStatus.Resolved)
            return false;

        return DateTime.UtcNow.Subtract(TriggeredAt).TotalHours > hoursThreshold;
    }

    /// <summary>
    /// description(설명) : Get alert age in hours (알림 생성 후 경과 시간 가져오기)
    /// Details(상세설명) : Returns the number of hours since alert was triggered (알림 발생 이후 경과한 시간 반환)
    /// </summary>
    public double GetAgeInHours()
    {
        return DateTime.UtcNow.Subtract(TriggeredAt).TotalHours;
    }
}

/// <summary>
/// description(설명) : Alert ID value object (알림 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for Alert (Alert의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class AlertId : GuidId
{
    public AlertId(Guid value) : base(value) { }

    public static AlertId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Alert Type enumeration (알림 타입 열거형)
/// Details(상세설명) : Defines the category of the alert (알림의 카테고리를 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum AlertType
{
    System = 1,         // 시스템 알림 (서버, DB, 네트워크 등)
    Trading = 2,        // 거래 알림 (주문, 체결 등)
    Risk = 3,           // 리스크 알림 (손실 한도, VaR 초과 등)
    Performance = 4,    // 성과 알림 (목표 달성, 손실 경고 등)
    Compliance = 5      // 컴플라이언스 알림 (규정 위반 등)
}

/// <summary>
/// description(설명) : Alert Status enumeration (알림 상태 열거형)
/// Details(상세설명) : Represents the current state of the alert (알림의 현재 상태를 나타냄)
/// Applied technology patterns(적용기술패턴) : State Machine Pattern, Enumeration Pattern (상태 머신 패턴, 열거형 패턴)
/// </summary>
public enum AlertStatus
{
    Triggered = 1,      // 발생
    Sent = 2,           // 전송됨
    Acknowledged = 3,   // 확인됨
    Resolved = 4        // 해결됨
}

/// <summary>
/// description(설명) : Notification Channel enumeration (알림 채널 열거형)
/// Details(상세설명) : Defines the delivery channel for the alert (알림 전달 채널을 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum NotificationChannel
{
    Email = 1,      // 이메일
    SMS = 2,        // 문자 메시지
    Telegram = 3,   // 텔레그램
    Slack = 4,      // 슬랙
    WebPush = 5,    // 웹 푸시
    InApp = 6       // 인앱 알림
}
