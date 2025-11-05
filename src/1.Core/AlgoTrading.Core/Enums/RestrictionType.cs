namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 계좌에 적용되는 거래 제한 유형 정의
/// Details : 증권 계좌에 부과될 수 있는 제한 사항을 WithdrawalBlock(출금 제한), TradingBlock(거래 제한), ShortSellingBlock(공매도 제한)으로 분류합니다. Account Context에서 계좌 제한 정보를 관리하며, Trading Context는 주문 생성 전 제한 사항을 검증합니다. 제한 유형에 따라 특정 거래 행위만 차단되며, 여러 제한이 동시에 적용될 수 있습니다. Risk Management Context는 리스크 위반 시 자동으로 TradingBlock을 적용하여 추가 손실을 방지합니다.
/// Applied technology patterns : Guard Pattern, Authorization Pattern, Business Rule Pattern
/// </summary>
public enum RestrictionType
{
    /// <summary>
    /// 출금 제한
    /// </summary>
    WithdrawalBlock = 1,

    /// <summary>
    /// 거래 제한
    /// </summary>
    TradingBlock = 2,

    /// <summary>
    /// 공매도 제한
    /// </summary>
    ShortSellingBlock = 3
}
