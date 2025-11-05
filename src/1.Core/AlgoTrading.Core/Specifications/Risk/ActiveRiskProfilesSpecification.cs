using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Risk;

/// <summary>
/// description : 현재 활성화된 리스크 프로필만 필터링하는 스펙
/// Details : IsActive가 true인 리스크 프로필을 조회하는 데 사용됩니다.
///           활성 리스크 프로필은 현재 적용 중인 리스크 한도 및 규칙을 정의하며, 거래 실행 전 리스크 검증에 사용됩니다.
///           계좌별로 활성 리스크 프로필을 조회하여 현재 적용 중인 리스크 관리 정책을 확인할 수 있습니다.
/// Applied technology patterns : Specification Pattern, Repository Pattern, Risk Management Pattern
/// </summary>
public class ActiveRiskProfilesSpecification : Specification<Core.Entities.Risk.RiskProfile>
{
    public override Expression<Func<Core.Entities.Risk.RiskProfile, bool>> ToExpression()
    {
        return riskProfile => riskProfile.IsActive == true;
    }
}
