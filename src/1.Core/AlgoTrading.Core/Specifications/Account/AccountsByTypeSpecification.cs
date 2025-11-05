using AlgoTrading.Core.Enums;
using System.Linq.Expressions;

namespace AlgoTrading.Core.Specifications.Account;

/// <summary>
/// description : 특정 계좌 유형으로 필터링하는 스펙
/// Details : AccountType(Cash, Margin, ISA, Retirement 등)을 기준으로 계좌를 조회합니다.
///           계좌 유형별로 허용되는 거래 유형 및 규제가 다르므로, 유형별 계좌 목록을 가져와 적절한 거래 규칙을 적용합니다.
///           예: var marginAccounts = repository.Where(new AccountsByTypeSpecification(AccountType.Margin)); // 신용 거래 계좌만 조회
/// Applied technology patterns : Specification Pattern, Repository Pattern, Account Type Pattern
/// </summary>
public class AccountsByTypeSpecification : Specification<Core.Entities.Portfolio.Account>
{
    private readonly AccountType _accountType;

    public AccountsByTypeSpecification(AccountType accountType)
    {
        _accountType = accountType;
    }

    public override Expression<Func<Core.Entities.Portfolio.Account, bool>> ToExpression()
    {
        return account => account.Type.Equals(_accountType);
    }
}
