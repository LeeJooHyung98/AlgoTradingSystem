using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications;

/// <summary>
/// description : 모든 스펙의 기본이 되는 추상 스펙 클래스
/// Details : Specification Pattern을 구현하는 기본 클래스로, Repository에서 사용되는 복잡한 쿼리 로직을 캡슐화합니다.
///           도메인 계층에서 쿼리 조건을 정의하고, 인프라스트럭처 계층의 Repository에서 IQueryable.Where()로 적용됩니다.
///           여러 Specification을 And(), Or(), Not() 메서드로 조합하여 복잡한 쿼리를 간결하고 재사용 가능하게 작성할 수 있습니다.
///           이 패턴은 도메인 로직과 쿼리 로직을 분리하여 Clean Architecture의 원칙을 준수하며, 단위 테스트를 용이하게 합니다.
/// Applied technology patterns : Specification Pattern, Repository Pattern, Expression Trees, Composite Pattern
/// </summary>
/// <typeparam name="T">스펙이 적용될 엔티티 타입</typeparam>
public abstract class Specification<T>
{
    /// <summary>
    /// 스펙의 조건을 표현하는 Expression
    /// </summary>
    public abstract Expression<Func<T, bool>> ToExpression();

    /// <summary>
    /// 스펙을 평가하여 엔티티가 조건을 만족하는지 확인
    /// </summary>
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    /// <summary>
    /// 두 스펙을 AND 조건으로 결합
    /// </summary>
    public Specification<T> And(Specification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    /// <summary>
    /// 두 스펙을 OR 조건으로 결합
    /// </summary>
    public Specification<T> Or(Specification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    /// <summary>
    /// 스펙을 NOT 조건으로 반전
    /// </summary>
    public Specification<T> Not()
    {
        return new NotSpecification<T>(this);
    }

    /// <summary>
    /// Specification을 Func로 암시적 변환 (편의성)
    /// </summary>
    public static implicit operator Expression<Func<T, bool>>(Specification<T> specification)
    {
        return specification.ToExpression();
    }
}

/// <summary>
/// description : AND 조건으로 두 스펙을 결합하는 복합 스펙
/// Details : Composite Pattern을 활용하여 두 개의 Specification을 논리 AND 연산자로 결합합니다.
///           예: ActiveStrategySpecification.And(StrategyByTypeSpecification) → 활성 상태이면서 특정 타입인 전략
/// Applied technology patterns : Specification Pattern, Composite Pattern, Expression Trees
/// </summary>
internal class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));
        var combined = Expression.AndAlso(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}

/// <summary>
/// description : OR 조건으로 두 스펙을 결합하는 복합 스펙
/// Details : Composite Pattern을 활용하여 두 개의 Specification을 논리 OR 연산자로 결합합니다.
///           예: PendingOrdersSpecification.Or(PartiallyFilledOrdersSpecification) → 대기 중이거나 부분 체결된 주문
/// Applied technology patterns : Specification Pattern, Composite Pattern, Expression Trees
/// </summary>
internal class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));
        var combined = Expression.OrElse(
            Expression.Invoke(leftExpression, parameter),
            Expression.Invoke(rightExpression, parameter)
        );

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}

/// <summary>
/// description : NOT 조건으로 스펙을 반전하는 복합 스펙
/// Details : Composite Pattern을 활용하여 Specification의 조건을 논리 NOT 연산자로 반전합니다.
///           예: ActiveStrategySpecification.Not() → 비활성 상태인 전략 (InactiveStrategySpecification과 동일)
/// Applied technology patterns : Specification Pattern, Composite Pattern, Expression Trees
/// </summary>
internal class NotSpecification<T> : Specification<T>
{
    private readonly Specification<T> _specification;

    public NotSpecification(Specification<T> specification)
    {
        _specification = specification;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var expression = _specification.ToExpression();
        var parameter = Expression.Parameter(typeof(T));
        var negated = Expression.Not(Expression.Invoke(expression, parameter));

        return Expression.Lambda<Func<T, bool>>(negated, parameter);
    }
}
