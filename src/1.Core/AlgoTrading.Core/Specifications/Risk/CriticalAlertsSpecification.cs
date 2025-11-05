using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Risk;

/// <summary>
/// description : 우선순위가 Critical인 리스크 알림만 필터링하는 스펙
/// Details : AlertPriority가 Critical 상태인 리스크 알림을 조회하는 데 사용됩니다.
///           Critical 알림은 즉각적인 조치가 필요한 중대한 리스크 상황(마진콜, 킬 스위치 발동, 일일 손실 한도 초과 등)을 나타냅니다.
///           모니터링 시스템에서 긴급 알림만 필터링하여 사용자에게 우선적으로 표시하거나 긴급 알림 채널(SMS, 전화)로 전송합니다.
/// Applied technology patterns : Specification Pattern, Repository Pattern, Alert Prioritization Pattern
/// </summary>
public class CriticalAlertsSpecification : Specification<Core.Entities.Risk.RiskAlert>
{
    public override Expression<Func<Core.Entities.Risk.RiskAlert, bool>> ToExpression()
    {
        return alert => alert.Priority == Core.Entities.Risk.AlertPriority.Critical;
    }
}
