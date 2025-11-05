namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 시장 데이터의 시간 집계 단위 (차트 주기)
/// Details : Market Data Context에서 OHLC(시가/고가/저가/종가) 데이터를 집계하는 시간 단위를 정의합니다. Tick(실시간 체결), Minute1/3/5/10/15/30(분봉), Hour1/4(시간봉), Day(일봉), Week(주봉), Month(월봉)를 지원합니다. Strategy Context에서 전략별로 분석에 사용할 TimeFrame을 지정하며, 짧은 TimeFrame은 스캘핑/데이트레이딩 전략에, 긴 TimeFrame은 스윙/포지션 트레이딩 전략에 적합합니다. TimescaleDB를 활용하여 각 TimeFrame별 데이터를 효율적으로 저장하고 조회합니다.
/// Applied technology patterns : Time Series Data Pattern, Data Aggregation Pattern, Multi-Timeframe Analysis
/// </summary>
public enum TimeFrame
{
    /// <summary>
    /// 틱 (Tick)
    /// </summary>
    Tick = 1,

    /// <summary>
    /// 1분
    /// </summary>
    Minute1 = 2,

    /// <summary>
    /// 3분
    /// </summary>
    Minute3 = 3,

    /// <summary>
    /// 5분
    /// </summary>
    Minute5 = 4,

    /// <summary>
    /// 10분
    /// </summary>
    Minute10 = 5,

    /// <summary>
    /// 15분
    /// </summary>
    Minute15 = 6,

    /// <summary>
    /// 30분
    /// </summary>
    Minute30 = 7,

    /// <summary>
    /// 1시간
    /// </summary>
    Hour1 = 8,

    /// <summary>
    /// 4시간
    /// </summary>
    Hour4 = 9,

    /// <summary>
    /// 일봉
    /// </summary>
    Day = 10,

    /// <summary>
    /// 주봉
    /// </summary>
    Week = 11,

    /// <summary>
    /// 월봉
    /// </summary>
    Month = 12
}
