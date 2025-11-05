using AlgoTrading.Core.Enums;
using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Strategy;

/// <summary>
/// description : 현재 활성화된 전략만 필터링하는 스펙
/// Details : StrategyStatus가 Active 상태인 전략을 조회하는 데 사용됩니다.
///           활성 전략은 실시간으로 시장 데이터를 수신하고 시그널을 생성하는 전략으로, 모니터링 및 실행 관리 시스템에서 자주 조회됩니다.
///           이 스펙은 Repository.Where(new ActiveStrategySpecification())와 같이 사용되어 활성 전략 목록을 가져옵니다.
/// Applied technology patterns : Specification Pattern, Repository Pattern, Query Object Pattern
/// </summary>
public class ActiveStrategySpecification : Specification<Core.Entities.Strategy.TradingStrategy>
{
    public override Expression<Func<Core.Entities.Strategy.TradingStrategy, bool>> ToExpression()
    {
        return strategy => strategy.Status.Equals(StrategyStatus.Active);
    }
}
