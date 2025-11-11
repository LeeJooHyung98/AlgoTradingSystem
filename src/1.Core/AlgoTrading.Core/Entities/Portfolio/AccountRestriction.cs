using AlgoTrading.Core.Entities.Users;
using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Portfolio;

/// <summary>
/// description(설명) : Account Restriction entity for managing account limitations (계좌 제한 사항을 관리하는 계좌 제약 엔티티)
/// Details(상세설명) : Tracks trading suspensions, withdrawal blocks, and compliance holds (거래 정지, 출금 차단, 컴플라이언스 보류를 추적)
/// Applied technology patterns(적용기술패턴) : Entity Pattern, Domain-Driven Design (엔티티 패턴, 도메인 주도 설계)
/// </summary>
public sealed class AccountRestriction : Entity<AccountRestrictionId>
{
    /// <summary>
    /// 계좌 ID
    /// </summary>
    public AccountId AccountId { get; private set; }

    /// <summary>
    /// 제약 타입
    /// </summary>
    public RestrictionType Type { get; private set; }

    /// <summary>
    /// 제약 상태
    /// </summary>
    public RestrictionStatus Status { get; private set; }

    /// <summary>
    /// 제약 사유
    /// </summary>
    public string Reason { get; private set; }

    /// <summary>
    /// 제약 적용 일시
    /// </summary>
    public DateTime AppliedAt { get; private set; }

    /// <summary>
    /// 제약 만료 일시 (null이면 무기한)
    /// </summary>
    public DateTime? ExpiresAt { get; private set; }

    /// <summary>
    /// 제약 철회 일시
    /// </summary>
    public DateTime? RevokedAt { get; private set; }

    /// <summary>
    /// 제약 적용자 (관리자 ID 또는 시스템)
    /// </summary>
    public UserId AppliedBy { get; private set; }

    /// <summary>
    /// 제약 철회자 (관리자 ID)
    /// </summary>
    public UserId? RevokedBy { get; private set; }

    private AccountRestriction() { }

    private AccountRestriction(
        AccountRestrictionId id,
        AccountId accountId,
        RestrictionType type,
        string reason,
        UserId appliedBy,
        DateTime? expiresAt = null)
    {
        Id = id;
        AccountId = accountId;
        Type = type;
        Reason = reason;
        Status = RestrictionStatus.Active;
        AppliedBy = appliedBy;
        AppliedAt = DateTime.UtcNow;
        ExpiresAt = expiresAt;
    }

    /// <summary>
    /// description(설명) : Apply a restriction to an account (계좌에 제약 적용)
    /// Details(상세설명) : Factory method to create a new restriction (새로운 제약을 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    public static AccountRestriction Apply(
        AccountId accountId,
        RestrictionType type,
        string reason,
        UserId appliedBy,
        DateTime? expiresAt = null)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason cannot be empty", nameof(reason));

        var id = AccountRestrictionId.New();
        return new AccountRestriction(id, accountId, type, reason, appliedBy, expiresAt);
    }

    /// <summary>
    /// description(설명) : Revoke the restriction (제약 철회)
    /// Details(상세설명) : Marks the restriction as revoked (제약을 철회됨으로 표시)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Revoke(UserId revokedBy)
    {
        if (Status == RestrictionStatus.Revoked)
            throw new InvalidOperationException("Restriction is already revoked");

        Status = RestrictionStatus.Revoked;
        RevokedAt = DateTime.UtcNow;
        RevokedBy = revokedBy;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if restriction is active (제약이 활성 상태인지 확인)
    /// Details(상세설명) : Returns true if restriction is active and not expired (제약이 활성 상태이고 만료되지 않았으면 true 반환)
    /// </summary>
    public bool IsActive()
    {
        if (Status != RestrictionStatus.Active)
            return false;

        if (ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow)
        {
            Status = RestrictionStatus.Expired;
            MarkAsModified();
            return false;
        }

        return true;
    }

    /// <summary>
    /// description(설명) : Extend expiration date (만료일 연장)
    /// Details(상세설명) : Updates the expiration date of the restriction (제약의 만료일 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void ExtendExpiration(DateTime newExpiresAt)
    {
        if (Status != RestrictionStatus.Active)
            throw new InvalidOperationException("Can only extend active restrictions");

        if (newExpiresAt <= DateTime.UtcNow)
            throw new ArgumentException("New expiration date must be in the future", nameof(newExpiresAt));

        ExpiresAt = newExpiresAt;
        MarkAsModified();
    }
}

/// <summary>
/// description(설명) : Account Restriction ID value object (계좌 제약 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for AccountRestriction (AccountRestriction의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class AccountRestrictionId : GuidId
{
    public AccountRestrictionId(Guid value) : base(value) { }

    public static AccountRestrictionId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : Restriction Type enumeration (제약 타입 열거형)
/// Details(상세설명) : Defines the type of restriction applied to an account (계좌에 적용된 제약의 유형을 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum RestrictionType
{
    TradingSuspended = 1,      // 거래 정지
    WithdrawalBlocked = 2,     // 출금 차단
    DepositOnly = 3,           // 입금만 가능
    ComplianceHold = 4         // 컴플라이언스 보류
}

/// <summary>
/// description(설명) : Restriction Status enumeration (제약 상태 열거형)
/// Details(상세설명) : Represents the current state of a restriction (제약의 현재 상태를 나타냄)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum RestrictionStatus
{
    Active = 1,     // 활성
    Expired = 2,    // 만료
    Revoked = 3     // 철회
}
