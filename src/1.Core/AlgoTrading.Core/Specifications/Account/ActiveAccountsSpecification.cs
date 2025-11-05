using AlgoTrading.Core.Enums;
using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Account;

/// <summary>
/// description : 현재 활성화된 계좌만 필터링하는 스펙
/// Details : AccountStatus가 Active 상태인 계좌를 조회하는 데 사용됩니다.
///           활성 계좌는 거래가 가능한 상태이며, 주문 생성 및 포지션 관리가 허용됩니다.
///           멀티 계좌 환경에서 현재 사용 가능한 계좌 목록을 표시하거나, 거래 실행 전 계좌 상태를 검증할 때 사용됩니다.
/// Applied technology patterns : Specification Pattern, Repository Pattern, Account Management Pattern
/// </summary>
public class ActiveAccountsSpecification : Specification<Core.Entities.Portfolio.Account>
{
    public override Expression<Func<Core.Entities.Portfolio.Account, bool>> ToExpression()
    {
        return account => account.Status.Equals(AccountStatus.Active);
    }
}
