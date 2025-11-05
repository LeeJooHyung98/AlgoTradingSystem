using AlgoTrading.Core.Enums;
using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Strategy;

/// <summary>
/// description : 특정 전략 유형으로 필터링하는 스펙
/// Details : StrategyType(Trend, MeanReversion, Momentum, Arbitrage, Statistical 등)을 기준으로 전략을 조회합니다.
///           사용자가 특정 유형의 전략 성과를 비교하거나, 포트폴리오에서 전략 유형별 배분을 확인할 때 사용됩니다.
///           예: var trendStrategies = repository.Where(new StrategyByTypeSpecification(StrategyType.Trend));
/// Applied technology patterns : Specification Pattern, Repository Pattern, Query Object Pattern
/// </summary>
public class StrategyByTypeSpecification : Specification<Core.Entities.Strategy.TradingStrategy>
{
    private readonly StrategyType _strategyType;

    public StrategyByTypeSpecification(StrategyType strategyType)
    {
        _strategyType = strategyType;
    }

    public override Expression<Func<Core.Entities.Strategy.TradingStrategy, bool>> ToExpression()
    {
        return strategy => strategy.Type.Equals(_strategyType);
    }
}
