namespace AlgoTrading.Core.ValueObjects.Common;

/// <summary>
/// TimeFrame Value Object
/// Represents a time interval for chart data (1m, 5m, 1h, 1d, etc.)
/// </summary>
public sealed class TimeFrame : ValueObject, IComparable<TimeFrame>
{
    /// <summary>
    /// Interval value
    /// </summary>
    public int Value { get; }

    /// <summary>
    /// Interval unit
    /// </summary>
    public TimeFrameUnit Unit { get; }

    /// <summary>
    /// Duration in seconds
    /// </summary>
    public int TotalSeconds { get; }

    /// <summary>
    /// Display name
    /// </summary>
    public string DisplayName { get; }

    private TimeFrame(int value, TimeFrameUnit unit)
    {
        if (value <= 0)
            throw new ArgumentException("TimeFrame value must be positive", nameof(value));

        Value = value;
        Unit = unit;
        TotalSeconds = CalculateTotalSeconds(value, unit);
        DisplayName = GenerateDisplayName(value, unit);
    }

    /// <summary>
    /// Create a time frame
    /// </summary>
    public static TimeFrame Create(int value, TimeFrameUnit unit)
    {
        return new TimeFrame(value, unit);
    }

    // Predefined time frames
    public static TimeFrame Tick => new(1, TimeFrameUnit.Tick);
    public static TimeFrame OneSecond => new(1, TimeFrameUnit.Second);
    public static TimeFrame OneMinute => new(1, TimeFrameUnit.Minute);
    public static TimeFrame ThreeMinutes => new(3, TimeFrameUnit.Minute);
    public static TimeFrame FiveMinutes => new(5, TimeFrameUnit.Minute);
    public static TimeFrame TenMinutes => new(10, TimeFrameUnit.Minute);
    public static TimeFrame FifteenMinutes => new(15, TimeFrameUnit.Minute);
    public static TimeFrame ThirtyMinutes => new(30, TimeFrameUnit.Minute);
    public static TimeFrame OneHour => new(1, TimeFrameUnit.Hour);
    public static TimeFrame TwoHours => new(2, TimeFrameUnit.Hour);
    public static TimeFrame FourHours => new(4, TimeFrameUnit.Hour);
    public static TimeFrame OneDay => new(1, TimeFrameUnit.Day);
    public static TimeFrame OneWeek => new(1, TimeFrameUnit.Week);
    public static TimeFrame OneMonth => new(1, TimeFrameUnit.Month);

    /// <summary>
    /// Parse from string (e.g., "1m", "5m", "1h", "1d")
    /// </summary>
    public static TimeFrame Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("TimeFrame string cannot be empty", nameof(value));

        value = value.Trim().ToLower();

        // Extract number and unit
        int i = 0;
        while (i < value.Length && char.IsDigit(value[i]))
            i++;

        if (i == 0)
            throw new FormatException($"Invalid TimeFrame format: {value}");

        var numberPart = value.Substring(0, i);
        var unitPart = value.Substring(i);

        if (!int.TryParse(numberPart, out var number))
            throw new FormatException($"Invalid number in TimeFrame: {numberPart}");

        var unit = unitPart switch
        {
            "t" or "tick" => TimeFrameUnit.Tick,
            "s" or "sec" or "second" => TimeFrameUnit.Second,
            "m" or "min" or "minute" => TimeFrameUnit.Minute,
            "h" or "hr" or "hour" => TimeFrameUnit.Hour,
            "d" or "day" => TimeFrameUnit.Day,
            "w" or "wk" or "week" => TimeFrameUnit.Week,
            "M" or "mo" or "month" => TimeFrameUnit.Month,
            _ => throw new FormatException($"Invalid unit in TimeFrame: {unitPart}")
        };

        return new TimeFrame(number, unit);
    }

    /// <summary>
    /// Try to parse from string
    /// </summary>
    public static bool TryParse(string value, out TimeFrame? timeFrame)
    {
        try
        {
            timeFrame = Parse(value);
            return true;
        }
        catch
        {
            timeFrame = null;
            return false;
        }
    }

    /// <summary>
    /// Get TimeSpan representation
    /// </summary>
    public TimeSpan ToTimeSpan()
    {
        return TimeSpan.FromSeconds(TotalSeconds);
    }

    /// <summary>
    /// Calculate how many candles fit in a period
    /// </summary>
    public int GetCandleCount(TimeSpan period)
    {
        var periodSeconds = (int)period.TotalSeconds;
        return periodSeconds / TotalSeconds;
    }

    /// <summary>
    /// Check if this timeframe is intraday (less than 1 day)
    /// </summary>
    public bool IsIntraday()
    {
        return TotalSeconds < 86400; // 24 hours
    }

    /// <summary>
    /// Compare to another TimeFrame
    /// </summary>
    public int CompareTo(TimeFrame? other)
    {
        if (other is null)
            return 1;

        return TotalSeconds.CompareTo(other.TotalSeconds);
    }

    /// <summary>
    /// Greater than operator
    /// </summary>
    public static bool operator >(TimeFrame left, TimeFrame right)
    {
        return left.TotalSeconds > right.TotalSeconds;
    }

    /// <summary>
    /// Less than operator
    /// </summary>
    public static bool operator <(TimeFrame left, TimeFrame right)
    {
        return left.TotalSeconds < right.TotalSeconds;
    }

    /// <summary>
    /// Greater than or equal operator
    /// </summary>
    public static bool operator >=(TimeFrame left, TimeFrame right)
    {
        return left.TotalSeconds >= right.TotalSeconds;
    }

    /// <summary>
    /// Less than or equal operator
    /// </summary>
    public static bool operator <=(TimeFrame left, TimeFrame right)
    {
        return left.TotalSeconds <= right.TotalSeconds;
    }

    private static int CalculateTotalSeconds(int value, TimeFrameUnit unit)
    {
        return unit switch
        {
            TimeFrameUnit.Tick => 0,
            TimeFrameUnit.Second => value,
            TimeFrameUnit.Minute => value * 60,
            TimeFrameUnit.Hour => value * 3600,
            TimeFrameUnit.Day => value * 86400,
            TimeFrameUnit.Week => value * 604800,
            TimeFrameUnit.Month => value * 2592000, // 30 days
            _ => throw new ArgumentException($"Unknown unit: {unit}")
        };
    }

    private static string GenerateDisplayName(int value, TimeFrameUnit unit)
    {
        var unitStr = unit switch
        {
            TimeFrameUnit.Tick => "tick",
            TimeFrameUnit.Second => "s",
            TimeFrameUnit.Minute => "m",
            TimeFrameUnit.Hour => "h",
            TimeFrameUnit.Day => "d",
            TimeFrameUnit.Week => "w",
            TimeFrameUnit.Month => "M",
            _ => throw new ArgumentException($"Unknown unit: {unit}")
        };

        return $"{value}{unitStr}";
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return DisplayName;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TotalSeconds;
    }
}

/// <summary>
/// Time Frame Unit enumeration
/// </summary>
public enum TimeFrameUnit
{
    Tick = 0,
    Second = 1,
    Minute = 2,
    Hour = 3,
    Day = 4,
    Week = 5,
    Month = 6
}
