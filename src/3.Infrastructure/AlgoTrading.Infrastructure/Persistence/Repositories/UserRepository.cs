using AlgoTrading.Core.Entities.Users;
using AlgoTrading.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoTrading.Infrastructure.Persistence.Repositories;

/// <summary>
/// description(설명) : User repository implementation (사용자 리포지토리 구현)
/// Details(상세설명) : User entity repository with username and email queries (사용자명 및 이메일 쿼리를 포함한 User 엔티티 리포지토리)
/// Applied technology patterns(적용기술패턴) : Repository Pattern (리포지토리 패턴)
/// </summary>
public class UserRepository : Repository<User, UserId>, IUserRepository
{
    public UserRepository(AlgoTradingDbContext context) : base(context)
    {
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        return await _dbSet
            .FirstOrDefaultAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return await _dbSet
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be empty", nameof(username));

        return await _dbSet
            .AnyAsync(u => u.Username == username, cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        return await _dbSet
            .AnyAsync(u => u.Email == email, cancellationToken);
    }
}
