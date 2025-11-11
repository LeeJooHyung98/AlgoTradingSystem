using AlgoTrading.Core.Entities.Users;
using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Audit;

/// <summary>
/// description(설명) : Audit Log entity for tracking all critical operations (모든 중요 작업을 추적하는 감사 로그 엔티티)
/// Details(상세설명) : Time-series data with user actions, entity changes, stored in TimescaleDB (TimescaleDB에 저장되는 사용자 작업, 엔티티 변경 내역을 포함한 시계열 데이터)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, Time-Series Pattern, Audit Trail Pattern (엔티티 패턴, 시계열 패턴, 감사 추적 패턴)
/// </summary>
public sealed class AuditLog : Entity<AuditLogId>
{
    /// <summary>
    /// 로그 기록 시각 (TimescaleDB 시계열 기준)
    /// </summary>
    public DateTime LogTimestamp { get; private set; }

    /// <summary>
    /// 작업 타입 (Create, Update, Delete, Execute, Cancel, Login, Logout)
    /// </summary>
    public ActionType Action { get; private set; }

    /// <summary>
    /// 대상 엔티티 타입 (Order, Portfolio, Strategy, User 등)
    /// </summary>
    public string EntityType { get; private set; }

    /// <summary>
    /// 대상 엔티티 ID (변경된 객체의 ID)
    /// </summary>
    public Guid EntityId { get; private set; }

    /// <summary>
    /// 사용자 ID (작업 수행자)
    /// </summary>
    public UserId UserId { get; private set; }

    /// <summary>
    /// 사용자 IP 주소
    /// </summary>
    public string? UserIp { get; private set; }

    /// <summary>
    /// 변경 전 값 (JSON 형식)
    /// </summary>
    public string? OldValues { get; private set; }

    /// <summary>
    /// 변경 후 값 (JSON 형식)
    /// </summary>
    public string? NewValues { get; private set; }

    /// <summary>
    /// 액션 설명
    /// </summary>
    public string? Description { get; private set; }

    private AuditLog() { }

    private AuditLog(
        AuditLogId id,
        DateTime logTimestamp,
        ActionType action,
        string entityType,
        Guid entityId,
        UserId userId)
    {
        Id = id;
        LogTimestamp = logTimestamp;
        Action = action;
        EntityType = entityType;
        EntityId = entityId;
        UserId = userId;
    }

    /// <summary>
    /// description(설명) : Create audit log entry (감사 로그 엔트리 생성)
    /// Details(상세설명) : Factory method to create audit trail record (감사 추적 레코드를 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static AuditLog Create(
        ActionType action,
        string entityType,
        Guid entityId,
        UserId userId,
        string? description = null,
        string? userIp = null)
    {
        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty", nameof(entityType));

        if (entityId == Guid.Empty)
            throw new ArgumentException("Entity ID cannot be empty", nameof(entityId));

        var id = AuditLogId.New();
        var logTimestamp = DateTime.UtcNow;
        var log = new AuditLog(id, logTimestamp, action, entityType, entityId, userId);

        if (!string.IsNullOrWhiteSpace(description))
            log.Description = description;

        if (!string.IsNullOrWhiteSpace(userIp))
            log.UserIp = userIp;

        return log;
    }

    /// <summary>
    /// description(설명) : Set change values (변경 값 설정)
    /// Details(상세설명) : Records the before and after values of entity changes (엔티티 변경의 이전/이후 값 기록)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void SetChangeValues(string? oldValues, string? newValues)
    {
        OldValues = oldValues;
        NewValues = newValues;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if action is sensitive (민감한 작업인지 확인)
    /// Details(상세설명) : Returns true for security-related actions requiring extra logging (추가 로깅이 필요한 보안 관련 작업이면 true 반환)
    /// </summary>
    public bool IsSensitiveAction()
    {
        return Action == ActionType.Login ||
               Action == ActionType.Logout ||
               Action == ActionType.Delete;
    }

    /// <summary>
    /// description(설명) : Get audit summary (감사 요약 가져오기)
    /// Details(상세설명) : Returns a formatted summary string for the audit log (감사 로그의 포맷된 요약 문자열 반환)
    /// </summary>
    public string GetSummary()
    {
        var userInfo = UserId != null ? $"User {UserId.Value}" : "System";
        return $"[{LogTimestamp:yyyy-MM-dd HH:mm:ss}] {userInfo} - {Action} {EntityType}:{EntityId}";
    }

    /// <summary>
    /// description(설명) : Check if audit log is recent (최근 로그인지 확인)
    /// Details(상세설명) : Returns true if the log is within specified hours (지정된 시간 내의 로그이면 true 반환)
    /// </summary>
    public bool IsRecent(int hoursThreshold = 24)
    {
        return DateTime.UtcNow.Subtract(LogTimestamp).TotalHours < hoursThreshold;
    }
}

/// <summary>
/// description(설명) : Audit Log ID value object (감사 로그 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for AuditLog (AuditLog의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class AuditLogId : GuidId
{
    public AuditLogId(Guid value) : base(value) { }

    public static AuditLogId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Action Type enumeration (작업 타입 열거형)
/// Details(상세설명) : Defines the type of action performed (수행된 작업의 유형을 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum ActionType
{
    Create = 1,     // 생성
    Update = 2,     // 수정
    Delete = 3,     // 삭제
    Execute = 4,    // 실행
    Cancel = 5,     // 취소
    Login = 6,      // 로그인
    Logout = 7      // 로그아웃
}
