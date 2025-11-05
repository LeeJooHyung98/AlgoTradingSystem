namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 트레이딩 전략의 의사결정 규칙 유형 분류
/// Details : 알고리즘 트레이딩 전략을 구성하는 규칙을 Entry(진입), Exit(청산), Filter(필터링), Risk(리스크 관리)로 분류합니다. Strategy Context에서 복합 전략을 설계할 때 각 규칙 타입별로 조건을 정의하고, 규칙들을 조합하여 최종 신호를 생성합니다. Entry 규칙은 매수/매도 진입 조건을, Exit 규칙은 청산 조건을, Filter 규칙은 거래 필터링(예: 거래량 조건, 변동성 조건)을, Risk 규칙은 포지션 크기와 손절 조건을 정의합니다.
/// Applied technology patterns : Rule Engine Pattern, Specification Pattern (DDD), Composite Pattern
/// </summary>
public enum RuleType
{
    /// <summary>
    /// 진입 규칙
    /// </summary>
    Entry = 1,

    /// <summary>
    /// 청산 규칙
    /// </summary>
    Exit = 2,

    /// <summary>
    /// 필터 규칙
    /// </summary>
    Filter = 3,

    /// <summary>
    /// 리스크 관리 규칙
    /// </summary>
    Risk = 4
}
