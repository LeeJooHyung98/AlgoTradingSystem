namespace AlgoTrading.Core.Exceptions;

/// <summary>
/// description : 모든 도메인 예외의 기본 클래스
/// Details : Clean Architecture와 DDD 패턴에서 도메인 계층의 비즈니스 규칙 위반 시 발생하는 모든 예외는 이 클래스를 상속받습니다.
///           인프라스트럭처 계층의 예외(DatabaseException, NetworkException)와 구분되며, 도메인 불변성(Invariants) 위반을 명확하게 표현합니다.
///           예외 메시지는 비즈니스 용어로 작성되어 사용자에게 의미 있는 오류 정보를 제공하며, 로깅 및 모니터링 시스템에서 도메인 문제를 추적할 수 있습니다.
/// Applied technology patterns : Domain-Driven Design (DDD), Exception Hierarchy Pattern, Clean Architecture
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// 예외가 발생한 엔티티 또는 집계의 ID (선택사항)
    /// </summary>
    public Guid? EntityId { get; }

    /// <summary>
    /// 예외 코드 (선택사항, 외부 시스템 연동 시 사용)
    /// </summary>
    public string? ErrorCode { get; }

    protected DomainException(string message) : base(message)
    {
    }

    protected DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected DomainException(string message, Guid? entityId = null, string? errorCode = null) : base(message)
    {
        EntityId = entityId;
        ErrorCode = errorCode;
    }

    protected DomainException(string message, Exception innerException, Guid? entityId = null, string? errorCode = null)
        : base(message, innerException)
    {
        EntityId = entityId;
        ErrorCode = errorCode;
    }
}
