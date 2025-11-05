namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 수익 실현을 위한 익절매 방식의 유형 분류
/// Details : Risk Management Context와 Strategy Context에서 포지션의 수익을 확정하는 익절 전략을 정의합니다. Percentage(진입가 대비 퍼센트), Fixed(고정 금액), MultiLevel(다단계 분할 익절), Resistance(저항선 기반)를 지원합니다. MultiLevel 방식은 목표 수익률에 도달할 때마다 포지션의 일부를 청산하여 리스크를 낮추면서 추가 수익 기회를 유지합니다. Resistance 방식은 기술적 분석의 저항선을 목표가로 설정하여 가격 반전 리스크를 최소화합니다.
/// Applied technology patterns : Strategy Pattern, Partial Execution Pattern, Technical Analysis Integration
/// </summary>
public enum TakeProfitType
{
    /// <summary>
    /// 퍼센트 기반
    /// </summary>
    Percentage = 1,

    /// <summary>
    /// 고정 금액
    /// </summary>
    Fixed = 2,

    /// <summary>
    /// 다단계 익절
    /// </summary>
    MultiLevel = 3,

    /// <summary>
    /// 저항선 기반
    /// </summary>
    Resistance = 4
}
