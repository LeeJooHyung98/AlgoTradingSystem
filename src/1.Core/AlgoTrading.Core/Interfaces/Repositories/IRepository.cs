namespace AlgoTrading.Core.Interfaces.Repositories;

/// <summary>
/// description : 모든 리포지토리의 기본이 되는 제네릭 리포지토리 인터페이스
/// Details : CRUD 작업을 위한 기본 메서드를 정의하며, 모든 엔티티 리포지토리는 이 인터페이스를 상속받습니다.
///           Clean Architecture의 Domain Layer에 위치하여 인프라스트럭처 계층에 대한 의존성을 역전시킵니다.
/// Applied technology patterns : Repository Pattern, Generic Repository Pattern, Dependency Inversion Principle, Clean Architecture
/// </summary>
/// <typeparam name="T">엔티티 타입</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// ID로 엔티티 조회
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// 모든 엔티티 조회
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 엔티티 추가
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 엔티티 수정
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// 엔티티 삭제
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// ID로 엔티티 존재 여부 확인
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);
}
