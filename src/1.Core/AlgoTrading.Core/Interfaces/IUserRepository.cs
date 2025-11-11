using AlgoTrading.Core.Entities.Users;

namespace AlgoTrading.Core.Interfaces;

/// <summary>
/// description(설명) : User repository interface (사용자 리포지토리 인터페이스)
/// Details(상세설명) : Repository for User entity with specific query methods (특정 쿼리 메서드를 포함한 User 엔티티 리포지토리)
/// Applied technology patterns(적용기술패턴) : Repository Pattern (리포지토리 패턴)
/// </summary>
public interface IUserRepository : IRepository<User, UserId>
{
    /// <summary>
    /// Get user by username (사용자명으로 사용자 조회)
    /// </summary>
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get user by email (이메일로 사용자 조회)
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if username exists (사용자명 존재 여부 확인)
    /// </summary>
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if email exists (이메일 존재 여부 확인)
    /// </summary>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
}
