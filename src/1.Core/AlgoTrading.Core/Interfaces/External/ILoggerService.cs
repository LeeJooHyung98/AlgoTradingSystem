namespace AlgoTrading.Core.Interfaces.External;

/// <summary>
/// description : 구조화된 로깅 서비스 인터페이스
/// Details : Infrastructure Layer에서 Serilog를 사용하여 애플리케이션 로그를 기록하고, ELK Stack(Elasticsearch, Logstash, Kibana)으로 전송하여 중앙 집중식 로그 분석을 지원합니다. Debug, Information, Warning, Error, Critical 레벨로 로그를 분류하며, 구조화된 로그(Structured Logging)로 주문 ID, 계좌번호, 전략 ID 등의 컨텍스트 정보를 포함합니다. 예외 발생 시 스택 트레이스와 함께 로그를 기록하여 문제 진단을 용이하게 하며, Application Layer의 모든 Service와 Command Handler에서 실행 추적과 오류 기록에 사용됩니다.
/// Applied technology patterns : Structured Logging Pattern, Centralized Logging Pattern, Cross-Cutting Concern Pattern
/// </summary>
public interface ILoggerService
{
    /// <summary>
    /// 디버그 로그
    /// </summary>
    void LogDebug(string message, params object[] args);

    /// <summary>
    /// 정보 로그
    /// </summary>
    void LogInformation(string message, params object[] args);

    /// <summary>
    /// 경고 로그
    /// </summary>
    void LogWarning(string message, params object[] args);

    /// <summary>
    /// 오류 로그
    /// </summary>
    void LogError(Exception exception, string message, params object[] args);

    /// <summary>
    /// 치명적 오류 로그
    /// </summary>
    void LogCritical(Exception exception, string message, params object[] args);

    /// <summary>
    /// 구조화된 로그
    /// </summary>
    void LogStructured(string messageTemplate, IDictionary<string, object> properties);
}
