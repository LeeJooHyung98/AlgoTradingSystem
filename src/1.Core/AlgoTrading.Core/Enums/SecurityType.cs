namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 증권(금융 상품) 유형을 정의하는 열거형
/// Details : 한국 주식시장에서 거래 가능한 증권 유형을 분류합니다. 주식, ETF, 옵션, 선물 등 다양한 금융 상품을 구분하여 각 상품별로 적절한 거래 전략과 리스크 관리를 적용할 수 있도록 합니다. Market Data Context에서 시장 데이터 수집 시 증권 유형별로 다른 API 엔드포인트를 호출하고, Trading Context에서 주문 생성 시 증권 유형에 따라 주문 파라미터를 검증합니다.
/// Applied technology patterns : Value Object Pattern (DDD), Type Safety Pattern
/// </summary>
public enum SecurityType
{
    /// <summary>
    /// 주식
    /// </summary>
    Stock = 1,

    /// <summary>
    /// 상장지수펀드 (ETF)
    /// </summary>
    ETF = 2,

    /// <summary>
    /// 옵션
    /// </summary>
    Option = 3,

    /// <summary>
    /// 선물
    /// </summary>
    Futures = 4
}
