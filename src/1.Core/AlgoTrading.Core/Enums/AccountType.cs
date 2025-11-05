namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 증권 계좌의 거래 방식에 따른 유형 분류
/// Details : 한국 주식시장의 계좌 유형을 Cash(현금 계좌), Margin(신용 거래 계좌), Futures(선물 계좌), DayTrading(데이 트레이딩 계좌)로 구분합니다. Account Context에서 계좌별 거래 가능 상품, 레버리지 한도, 증거금 요구사항을 관리합니다. Margin 계좌는 신용 거래를 지원하며 추가 리스크 관리가 필요하고, DayTrading 계좌는 당일 매매 전용으로 오버나잇 포지션이 제한됩니다. Risk Management Context는 계좌 유형에 따라 다른 리스크 프로필을 적용합니다.
/// Applied technology patterns : Domain-Driven Design (Bounded Context), Policy Pattern
/// </summary>
public enum AccountType
{
    /// <summary>
    /// 현금 계좌
    /// </summary>
    Cash = 1,

    /// <summary>
    /// 마진 계좌 (신용 거래)
    /// </summary>
    Margin = 2,

    /// <summary>
    /// 선물 계좌
    /// </summary>
    Futures = 3,

    /// <summary>
    /// 데이 트레이딩 계좌
    /// </summary>
    DayTrading = 4
}
