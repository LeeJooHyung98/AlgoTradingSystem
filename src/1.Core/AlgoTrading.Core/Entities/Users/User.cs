using AlgoTrading.Core.ValueObjects;

namespace AlgoTrading.Core.Entities.Users;

/// <summary>
/// description(설명) : User Aggregate Root representing a system user (시스템 사용자를 나타내는 사용자 집합 루트)
/// Details(상세설명) : Manages user authentication, authorization, roles, and account status (사용자 인증, 권한, 역할 및 계정 상태를 관리)
/// Applied technology patterns(적용기술패턴) : Aggregate Root Pattern, Domain-Driven Design (집합 루트 패턴, 도메인 주도 설계)
/// </summary>
public sealed class User : AggregateRoot<UserId>
{
    /// <summary>
    /// 사용자명 (로그인 ID)
    /// </summary>
    public string Username { get; private set; }

    /// <summary>
    /// 이메일 주소
    /// </summary>
    public string Email { get; private set; }

    /// <summary>
    /// 암호화된 비밀번호 해시
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// 실명
    /// </summary>
    public string? FullName { get; private set; }

    /// <summary>
    /// 전화번호
    /// </summary>
    public string? PhoneNumber { get; private set; }

    /// <summary>
    /// 사용자 역할
    /// </summary>
    public UserRole Role { get; private set; }

    /// <summary>
    /// 사용자 상태
    /// </summary>
    public UserStatus Status { get; private set; }

    /// <summary>
    /// 마지막 로그인 일시
    /// </summary>
    public DateTime? LastLoginAt { get; private set; }

    /// <summary>
    /// 마지막 로그인 IP 주소
    /// </summary>
    public string? LastLoginIp { get; private set; }

    /// <summary>
    /// 연속 로그인 실패 횟수
    /// </summary>
    public int FailedLoginAttempts { get; private set; }

    /// <summary>
    /// 계정 잠금 종료 시각
    /// </summary>
    public DateTime? LockedUntil { get; private set; }

    /// <summary>
    /// 생성 일시
    /// </summary>
    public DateTime CreatedAt { get; private set; }

    /// <summary>
    /// 최종 수정 일시
    /// </summary>
    public DateTime UpdatedAt { get; private set; }

    private User() { }

    private User(
        UserId id,
        string username,
        string email,
        string passwordHash,
        UserRole role)
    {
        Id = id;
        Username = username;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
        Status = UserStatus.Active;
        FailedLoginAttempts = 0;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// description(설명) : Create a new user (새로운 사용자 생성)
    /// Details(상세설명) : Factory method to create a user with credentials and role (자격 증명 및 역할로 사용자를 생성하는 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern, Domain-Driven Design (팩토리 패턴, 도메인 주도 설계)
    /// </summary>
    /// <returns>Created user instance (생성된 사용자 인스턴스)</returns>
    public static User Create(
        string username,
        string email,
        string passwordHash,
        UserRole role = UserRole.Trader)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format", nameof(email));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(passwordHash));

        var id = UserId.New();
        return new User(id, username, email, passwordHash, role);
    }

    /// <summary>
    /// description(설명) : Update user profile (사용자 프로필 업데이트)
    /// Details(상세설명) : Updates full name and phone number (실명 및 전화번호 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void UpdateProfile(string? fullName, string? phoneNumber)
    {
        FullName = fullName;
        PhoneNumber = phoneNumber;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Change user password (사용자 비밀번호 변경)
    /// Details(상세설명) : Updates password hash and resets failed login attempts (비밀번호 해시 업데이트 및 실패 로그인 횟수 초기화)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("Password hash cannot be empty", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Record successful login (성공적인 로그인 기록)
    /// Details(상세설명) : Updates last login time, IP, and resets failed attempts (마지막 로그인 시간, IP 업데이트 및 실패 횟수 초기화)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void RecordSuccessfulLogin(string ipAddress)
    {
        LastLoginAt = DateTime.UtcNow;
        LastLoginIp = ipAddress;
        FailedLoginAttempts = 0;
        LockedUntil = null;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Record failed login attempt (실패한 로그인 시도 기록)
    /// Details(상세설명) : Increments failed attempts and locks account if threshold exceeded (실패 횟수 증가 및 임계값 초과시 계정 잠금)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design, Security Pattern (도메인 주도 설계, 보안 패턴)
    /// </summary>
    public void RecordFailedLoginAttempt(int maxAttempts = 5, int lockoutMinutes = 30)
    {
        FailedLoginAttempts++;
        UpdatedAt = DateTime.UtcNow;

        if (FailedLoginAttempts >= maxAttempts)
        {
            Status = UserStatus.Locked;
            LockedUntil = DateTime.UtcNow.AddMinutes(lockoutMinutes);
        }

        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Unlock user account (사용자 계정 잠금 해제)
    /// Details(상세설명) : Resets lock status and failed login attempts (잠금 상태 및 실패 로그인 횟수 초기화)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void Unlock()
    {
        Status = UserStatus.Active;
        LockedUntil = null;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Suspend user account (사용자 계정 정지)
    /// Details(상세설명) : Changes status to Suspended, preventing login (상태를 정지로 변경하여 로그인 방지)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Suspend()
    {
        Status = UserStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Activate user account (사용자 계정 활성화)
    /// Details(상세설명) : Changes status to Active, enabling login (상태를 활성으로 변경하여 로그인 활성화)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Activate()
    {
        Status = UserStatus.Active;
        LockedUntil = null;
        FailedLoginAttempts = 0;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Deactivate user account (사용자 계정 비활성화)
    /// Details(상세설명) : Changes status to Inactive (상태를 비활성으로 변경)
    /// Applied technology patterns(적용기술패턴) : State Machine Pattern (상태 머신 패턴)
    /// </summary>
    public void Deactivate()
    {
        Status = UserStatus.Inactive;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Change user role (사용자 역할 변경)
    /// Details(상세설명) : Updates user role (Admin, Trader, Viewer, Developer) (사용자 역할 업데이트)
    /// Applied technology patterns(적용기술패턴) : Domain-Driven Design (도메인 주도 설계)
    /// </summary>
    public void ChangeRole(UserRole newRole)
    {
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
        MarkAsModified();
    }

    /// <summary>
    /// description(설명) : Check if account is locked (계정이 잠겨있는지 확인)
    /// Details(상세설명) : Returns true if account is locked and lock hasn't expired (계정이 잠겨있고 잠금이 만료되지 않았으면 true 반환)
    /// </summary>
    public bool IsLocked()
    {
        if (Status != UserStatus.Locked)
            return false;

        if (LockedUntil.HasValue && LockedUntil.Value <= DateTime.UtcNow)
        {
            // Auto-unlock if lock period expired
            Unlock();
            return false;
        }

        return true;
    }

    /// <summary>
    /// description(설명) : Check if user can login (사용자가 로그인할 수 있는지 확인)
    /// Details(상세설명) : Returns true if user is active and not locked (사용자가 활성 상태이고 잠겨있지 않으면 true 반환)
    /// </summary>
    public bool CanLogin()
    {
        return Status == UserStatus.Active && !IsLocked();
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// description(설명) : User ID value object (사용자 ID 값 객체)
/// Details(상세설명) : Strongly typed identifier for User aggregate (User 집합의 강타입 식별자)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Strongly Typed ID Pattern (값 객체 패턴, 강타입 ID 패턴)
/// </summary>
public sealed class UserId : GuidId
{
    public UserId(Guid value) : base(value) { }

    public static UserId New() => new(Guid.NewGuid());
}

/// <summary>
/// description(설명) : User Role enumeration (사용자 역할 열거형)
/// Details(상세설명) : Defines the role and permissions of a user (사용자의 역할과 권한을 정의)
/// Applied technology patterns(적용기술패턴) : Enumeration Pattern (열거형 패턴)
/// </summary>
public enum UserRole
{
    Admin = 1,          // 관리자 - 모든 권한
    Trader = 2,         // 트레이더 - 거래 및 전략 관리
    Viewer = 3,         // 조회자 - 읽기 전용
    Developer = 4       // 개발자 - 시스템 개발 및 테스트
}

/// <summary>
/// description(설명) : User Status enumeration (사용자 상태 열거형)
/// Details(상세설명) : Represents the current state of a user account (사용자 계정의 현재 상태를 나타냄)
/// Applied technology patterns(적용기술패턴) : State Machine Pattern, Enumeration Pattern (상태 머신 패턴, 열거형 패턴)
/// </summary>
public enum UserStatus
{
    Active = 1,         // 활성 - 로그인 가능
    Inactive = 2,       // 비활성 - 일시적으로 사용 안함
    Suspended = 3,      // 정지 - 관리자에 의해 정지됨
    Locked = 4          // 잠금 - 로그인 실패로 인한 자동 잠금
}
