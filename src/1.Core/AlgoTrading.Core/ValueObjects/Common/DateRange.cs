namespace AlgoTrading.Core.ValueObjects.Common;

/// <summary>
/// description(설명) : Date Range Value Object representing a period between two dates (두 날짜 사이의 기간을 나타내는 날짜 범위 값 객체)
/// Details(상세설명) : Immutable value object for date range operations with rich domain methods (풍부한 도메인 메서드를 포함한 날짜 범위 연산을 위한 불변 값 객체)
/// Applied technology patterns(적용기술패턴) : Value Object Pattern, Immutability Pattern, Domain-Driven Design (값 객체 패턴, 불변성 패턴, 도메인 주도 설계)
/// </summary>
public sealed class DateRange : ValueObject
{
    /// <summary>
    /// 시작 날짜 (포함)
    /// </summary>
    public DateTime StartDate { get; }

    /// <summary>
    /// 종료 날짜 (포함)
    /// </summary>
    public DateTime EndDate { get; }

    /// <summary>
    /// 범위 기간
    /// </summary>
    public TimeSpan Duration => EndDate - StartDate;

    /// <summary>
    /// 범위 내 총 일수
    /// </summary>
    public int TotalDays => (int)Math.Ceiling(Duration.TotalDays);

    /// <summary>
    /// 범위 내 총 시간
    /// </summary>
    public double TotalHours => Duration.TotalHours;

    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate;
        EndDate = endDate;
    }

    /// <summary>
    /// description(설명) : Create a new date range (새로운 날짜 범위 생성)
    /// Details(상세설명) : Factory method to create date range with validation (검증을 포함한 날짜 범위 생성 팩토리 메서드)
    /// Applied technology patterns(적용기술패턴) : Factory Pattern (팩토리 패턴)
    /// </summary>
    /// <returns>New DateRange instance (새로운 DateRange 인스턴스)</returns>
    public static DateRange Create(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date must be after start date", nameof(endDate));

        return new DateRange(startDate, endDate);
    }

    /// <summary>
    /// Create a date range from today
    /// </summary>
    public static DateRange FromToday(int days)
    {
        var today = DateTime.Today;
        return new DateRange(today, today.AddDays(days));
    }

    /// <summary>
    /// Create a date range for the last N days
    /// </summary>
    public static DateRange LastNDays(int days)
    {
        var today = DateTime.Today;
        return new DateRange(today.AddDays(-days), today);
    }

    /// <summary>
    /// Create a date range for current month
    /// </summary>
    public static DateRange CurrentMonth()
    {
        var today = DateTime.Today;
        var firstDay = new DateTime(today.Year, today.Month, 1);
        var lastDay = firstDay.AddMonths(1).AddDays(-1);
        return new DateRange(firstDay, lastDay);
    }

    /// <summary>
    /// Create a date range for current year
    /// </summary>
    public static DateRange CurrentYear()
    {
        var today = DateTime.Today;
        var firstDay = new DateTime(today.Year, 1, 1);
        var lastDay = new DateTime(today.Year, 12, 31);
        return new DateRange(firstDay, lastDay);
    }

    /// <summary>
    /// Create a date range for current week
    /// </summary>
    public static DateRange CurrentWeek()
    {
        var today = DateTime.Today;
        var diff = (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;
        var monday = today.AddDays(-1 * diff);
        var sunday = monday.AddDays(6);
        return new DateRange(monday, sunday);
    }

    /// <summary>
    /// Check if a date is within the range
    /// </summary>
    public bool Contains(DateTime date)
    {
        return date >= StartDate && date <= EndDate;
    }

    /// <summary>
    /// Check if this range overlaps with another
    /// </summary>
    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    /// <summary>
    /// Get the intersection with another date range
    /// </summary>
    public DateRange? Intersect(DateRange other)
    {
        if (!Overlaps(other))
            return null;

        var start = StartDate > other.StartDate ? StartDate : other.StartDate;
        var end = EndDate < other.EndDate ? EndDate : other.EndDate;

        return new DateRange(start, end);
    }

    /// <summary>
    /// Extend the range by adding days
    /// </summary>
    public DateRange ExtendBy(int days)
    {
        return new DateRange(StartDate, EndDate.AddDays(days));
    }

    /// <summary>
    /// Shift the range by days
    /// </summary>
    public DateRange ShiftBy(int days)
    {
        return new DateRange(StartDate.AddDays(days), EndDate.AddDays(days));
    }

    /// <summary>
    /// Split the range into N equal parts
    /// </summary>
    public List<DateRange> Split(int parts)
    {
        if (parts <= 0)
            throw new ArgumentException("Parts must be positive", nameof(parts));

        var ranges = new List<DateRange>();
        var totalDays = TotalDays;
        var daysPerPart = totalDays / parts;

        for (int i = 0; i < parts; i++)
        {
            var start = StartDate.AddDays(i * daysPerPart);
            var end = i == parts - 1 ? EndDate : StartDate.AddDays((i + 1) * daysPerPart).AddSeconds(-1);
            ranges.Add(new DateRange(start, end));
        }

        return ranges;
    }

    /// <summary>
    /// Check if this range is in the past
    /// </summary>
    public bool IsInPast()
    {
        return EndDate < DateTime.Now;
    }

    /// <summary>
    /// Check if this range is in the future
    /// </summary>
    public bool IsInFuture()
    {
        return StartDate > DateTime.Now;
    }

    /// <summary>
    /// Check if this range includes today
    /// </summary>
    public bool IsCurrently()
    {
        return Contains(DateTime.Now);
    }

    /// <summary>
    /// String representation
    /// </summary>
    public override string ToString()
    {
        return $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
    }

    /// <summary>
    /// String representation with custom format
    /// </summary>
    public string ToString(string format)
    {
        return $"{StartDate.ToString(format)} to {EndDate.ToString(format)}";
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }
}
