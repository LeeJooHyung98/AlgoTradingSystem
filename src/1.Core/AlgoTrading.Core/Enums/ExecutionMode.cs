namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 트레이딩 전략의 주문 실행 방식을 정의하는 열거형
/// Details : 전략의 신호를 실제 주문으로 전환하는 방식을 Manual(수동 승인), Auto(완전 자동), Paper(모의 투자), Hybrid(반자동)로 구분합니다. Strategy Context와 Trading Context 간 통합 지점에서 사용되며, Auto 모드는 신호 발생 즉시 주문을 생성하고, Manual 모드는 사용자 승인 후 주문을 생성합니다. Paper 모드는 실제 자금 없이 백테스팅과 유사하게 동작하지만 실시간 데이터를 사용하여 전략을 검증합니다.
/// Applied technology patterns : Strategy Pattern, Command Pattern, Feature Toggle Pattern
/// </summary>
public enum ExecutionMode
{
    /// <summary>
    /// 수동 실행
    /// </summary>
    Manual = 1,

    /// <summary>
    /// 자동 실행
    /// </summary>
    Auto = 2,

    /// <summary>
    /// 모의 투자 (페이퍼 트레이딩)
    /// </summary>
    Paper = 3,

    /// <summary>
    /// 혼합 모드 (반자동)
    /// </summary>
    Hybrid = 4
}
