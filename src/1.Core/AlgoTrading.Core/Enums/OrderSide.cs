namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 주문의 매수/매도 방향을 정의하는 열거형
/// Details : 거래 주문의 방향(매수, 매도, 공매도)을 나타내며, Order 엔티티에서 사용됩니다.
/// Applied technology patterns : Enum Pattern, Domain-Driven Design
/// </summary>
public enum OrderSide
{
    /// <summary>
    /// 매수
    /// </summary>
    Buy = 1,

    /// <summary>
    /// 매도
    /// </summary>
    Sell = 2,

    /// <summary>
    /// 공매도
    /// </summary>
    Short = 3
}
