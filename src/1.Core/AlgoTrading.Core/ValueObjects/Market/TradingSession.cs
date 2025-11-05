namespace AlgoTrading.Core.ValueObjects.Market;

/// <summary>
/// Trading Session Value Object
/// Represents a trading session period
/// </summary>
public sealed class TradingSession : ValueObject
{
    /// <summary>
    /// Session code
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Session name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Session start time
    /// </summary>
    public TimeSpan StartTime { get; }

    /// <summary>
    /// Session end time
    /// </summary>
    public TimeSpan EndTime { get; }

    /// <summary>
    /// Session duration
    /// </summary>
    public TimeSpan Duration => EndTime - StartTime;

    /// <summary>
    /// Is this a regular trading session
    /// </summary>
    public bool IsRegularSession { get; }

    private TradingSession(
        string code,
        string name,
        TimeSpan startTime,
        TimeSpan endTime,
        bool isRegularSession = true)
    {
        if (endTime <= startTime)
            throw new ArgumentException("End time must be after start time");

        Code = code;
        Name = name;
        StartTime = startTime;
        EndTime = endTime;
        IsRegularSession = isRegularSession;
    }

    /// <summary>
    /// Pre-market session (장전 시간외)
    /// </summary>
    public static TradingSession PreMarket => new(
        "PRE",
        "장전 시간외",
        new TimeSpan(8, 30, 0),
        new TimeSpan(9, 0, 0),
        isRegularSession: false
    );

    /// <summary>
    /// Regular market session (정규장)
    /// </summary>
    public static TradingSession Regular => new(
        "REG",
        "정규장",
        new TimeSpan(9, 0, 0),
        new TimeSpan(15, 30, 0),
        isRegularSession: true
    );

    /// <summary>
    /// After-hours session (장후 시간외)
    /// </summary>
    public static TradingSession AfterHours => new(
        "POST",
        "장후 시간외",
        new TimeSpan(15, 40, 0),
        new TimeSpan(16, 0, 0),
        isRegularSession: false
    );

    /// <summary>
    /// Opening auction (개장 전 단일가)
    /// </summary>
    public static TradingSession OpeningAuction => new(
        "OPN_AUC",
        "개장 전 단일가",
        new TimeSpan(8, 0, 0),
        new TimeSpan(9, 0, 0),
        isRegularSession: false
    );

    /// <summary>
    /// Closing auction (종가 단일가)
    /// </summary>
    public static TradingSession ClosingAuction => new(
        "CLS_AUC",
        "종가 단일가",
        new TimeSpan(15, 20, 0),
        new TimeSpan(15, 30, 0),
        isRegularSession: false
    );

    /// <summary>
    /// Morning session (오전장)
    /// </summary>
    public static TradingSession Morning => new(
        "AM",
        "오전장",
        new TimeSpan(9, 0, 0),
        new TimeSpan(12, 0, 0),
        isRegularSession: true
    );

    /// <summary>
    /// Afternoon session (오후장)
    /// </summary>
    public static TradingSession Afternoon => new(
        "PM",
        "오후장",
        new TimeSpan(12, 0, 0),
        new TimeSpan(15, 30, 0),
        isRegularSession: true
    );

    /// <summary>
    /// All predefined sessions
    /// </summary>
    public static IReadOnlyList<TradingSession> All => new[]
    {
        OpeningAuction,
        PreMarket,
        Regular,
        Morning,
        Afternoon,
        ClosingAuction,
        AfterHours
    };

    /// <summary>
    /// Regular trading sessions only
    /// </summary>
    public static IReadOnlyList<TradingSession> RegularSessions => new[]
    {
        Regular,
        Morning,
        Afternoon
    };

    /// <summary>
    /// Extended hours sessions
    /// </summary>
    public static IReadOnlyList<TradingSession> ExtendedHours => new[]
    {
        PreMarket,
        AfterHours
    };

    /// <summary>
    /// Create a custom session
    /// </summary>
    public static TradingSession Create(
        string code,
        string name,
        TimeSpan startTime,
        TimeSpan endTime,
        bool isRegularSession = true)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Session code cannot be empty", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Session name cannot be empty", nameof(name));

        return new TradingSession(code, name, startTime, endTime, isRegularSession);
    }

    /// <summary>
    /// Get current session for a given time
    /// </summary>
    public static TradingSession? GetCurrentSession(TimeSpan currentTime)
    {
        return All.FirstOrDefault(s => s.Contains(currentTime));
    }

    /// <summary>
    /// Get current session for current time
    /// </summary>
    public static TradingSession? GetCurrentSession()
    {
        return GetCurrentSession(DateTime.Now.TimeOfDay);
    }

    /// <summary>
    /// Check if a time is within this session
    /// </summary>
    public bool Contains(TimeSpan time)
    {
        return time >= StartTime && time <= EndTime;
    }

    /// <summary>
    /// Check if this session overlaps with another
    /// </summary>
    public bool Overlaps(TradingSession other)
    {
        return StartTime < other.EndTime && EndTime > other.StartTime;
    }

    /// <summary>
    /// Get time until session starts
    /// </summary>
    public TimeSpan? TimeUntilStart(TimeSpan currentTime)
    {
        if (currentTime < StartTime)
            return StartTime - currentTime;

        return null;
    }

    /// <summary>
    /// Get time until session ends
    /// </summary>
    public TimeSpan? TimeUntilEnd(TimeSpan currentTime)
    {
        if (currentTime < EndTime)
            return EndTime - currentTime;

        return null;
    }

    /// <summary>
    /// Check if session has started
    /// </summary>
    public bool HasStarted(TimeSpan currentTime)
    {
        return currentTime >= StartTime;
    }

    /// <summary>
    /// Check if session has ended
    /// </summary>
    public bool HasEnded(TimeSpan currentTime)
    {
        return currentTime > EndTime;
    }

    /// <summary>
    /// Check if session is currently active
    /// </summary>
    public bool IsActive(TimeSpan currentTime)
    {
        return Contains(currentTime);
    }

    /// <summary>
    /// Check if this is an extended hours session
    /// </summary>
    public bool IsExtendedHours()
    {
        return !IsRegularSession;
    }

    /// <summary>
    /// Get percentage of session completed
    /// </summary>
    public decimal GetProgress(TimeSpan currentTime)
    {
        if (currentTime < StartTime)
            return 0;

        if (currentTime > EndTime)
            return 100;

        var elapsed = currentTime - StartTime;
        return (decimal)(elapsed.TotalMinutes / Duration.TotalMinutes) * 100;
    }

    /// <summary>
    /// Check if this is a market opening session
    /// </summary>
    public bool IsMarketOpening()
    {
        return Code == "OPN_AUC" || Code == "PRE";
    }

    /// <summary>
    /// Check if this is a market closing session
    /// </summary>
    public bool IsMarketClosing()
    {
        return Code == "CLS_AUC" || Code == "POST";
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return $"{Name} ({StartTime:hh\\:mm} - {EndTime:hh\\:mm})";
    }

    /// <summary>
    /// String representation with duration
    /// </summary>
    public string ToStringWithDuration()
    {
        return $"{Name} ({StartTime:hh\\:mm} - {EndTime:hh\\:mm}, {Duration.TotalMinutes:N0}분)";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Code;
        yield return StartTime;
        yield return EndTime;
    }
}
