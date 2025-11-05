namespace AlgoTrading.Core.Enums;

/// <summary>
/// description : 리스크 모니터링 시스템의 운영 상태
/// Details : Monitoring Context에서 실시간 리스크 감시 시스템의 상태를 Initializing(초기화 중), Running(실행 중), Paused(일시 중지), Stopped(중지)로 관리합니다. Running 상태에서는 포지션, 주문, 계좌 잔고를 실시간으로 모니터링하여 리스크 위반을 탐지하고 알림을 발송합니다. Paused 상태는 시장 휴장 시간이나 유지보수 시 사용되며, 모니터링을 일시 중단하지만 상태는 보존됩니다. Initializing 상태에서는 필요한 리소스(Redis 연결, RabbitMQ 구독)를 설정합니다.
/// Applied technology patterns : State Pattern, Background Service Pattern, Health Check Pattern
/// </summary>
public enum MonitoringStatus
{
    /// <summary>
    /// 초기화 중
    /// </summary>
    Initializing = 1,

    /// <summary>
    /// 실행 중
    /// </summary>
    Running = 2,

    /// <summary>
    /// 일시 중지됨
    /// </summary>
    Paused = 3,

    /// <summary>
    /// 중지됨
    /// </summary>
    Stopped = 4
}
