namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 통화 유형을 정의하는 열거형
/// Details : 시스템에서 지원하는 통화 종류를 나타내며, 주로 계좌 및 거래 금액 표시에 사용됩니다.
/// Applied technology patterns : Enum Pattern, Value Object Pattern
/// </summary>
public enum Currency
{
    /// <summary>
    /// 한국 원화
    /// </summary>
    KRW = 1,

    /// <summary>
    /// 미국 달러
    /// </summary>
    USD = 2,

    /// <summary>
    /// 일본 엔화
    /// </summary>
    JPY = 3
}
