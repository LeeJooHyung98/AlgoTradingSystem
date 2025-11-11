namespace AlgoTrading.TechnicalIndicators.Indicators;

/// <summary>
/// Simple Moving Average (SMA) indicator
/// </summary>
public class SimpleMovingAverage : IIndicator<decimal, decimal>
{
    private readonly int _period;

    public SimpleMovingAverage(int period)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be greater than 0", nameof(period));

        _period = period;
    }

    public decimal Calculate(IEnumerable<decimal> data)
    {
        var dataList = data.TakeLast(_period).ToList();

        if (dataList.Count < _period)
            throw new InvalidOperationException($"Insufficient data. Required: {_period}, Available: {dataList.Count}");

        return dataList.Average();
    }
}

/// <summary>
/// Exponential Moving Average (EMA) indicator
/// </summary>
public class ExponentialMovingAverage : IIncrementalIndicator<decimal, decimal>
{
    private readonly int _period;
    private readonly decimal _multiplier;
    private decimal? _previousEma;
    private readonly Queue<decimal> _initialData;

    public ExponentialMovingAverage(int period)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be greater than 0", nameof(period));

        _period = period;
        _multiplier = 2m / (period + 1);
        _initialData = new Queue<decimal>();
    }

    public decimal Calculate(IEnumerable<decimal> data)
    {
        var dataList = data.ToList();

        if (dataList.Count < _period)
            throw new InvalidOperationException($"Insufficient data. Required: {_period}, Available: {dataList.Count}");

        // Start with SMA for first value
        var ema = dataList.Take(_period).Average();

        // Calculate EMA for remaining values
        for (int i = _period; i < dataList.Count; i++)
        {
            ema = (dataList[i] - ema) * _multiplier + ema;
        }

        return ema;
    }

    public decimal Update(decimal newValue)
    {
        if (_previousEma == null)
        {
            // Build initial data for SMA
            _initialData.Enqueue(newValue);

            if (_initialData.Count < _period)
                return _initialData.Average(); // Return current average while building

            // Calculate initial EMA from SMA
            _previousEma = _initialData.Average();
            return _previousEma.Value;
        }

        // Calculate EMA incrementally
        _previousEma = (newValue - _previousEma.Value) * _multiplier + _previousEma.Value;
        return _previousEma.Value;
    }

    public void Reset()
    {
        _previousEma = null;
        _initialData.Clear();
    }
}

/// <summary>
/// Weighted Moving Average (WMA) indicator
/// Gives more weight to recent prices
/// </summary>
public class WeightedMovingAverage : IIndicator<decimal, decimal>
{
    private readonly int _period;

    public WeightedMovingAverage(int period)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be greater than 0", nameof(period));

        _period = period;
    }

    public decimal Calculate(IEnumerable<decimal> data)
    {
        var dataList = data.TakeLast(_period).ToList();

        if (dataList.Count < _period)
            throw new InvalidOperationException($"Insufficient data. Required: {_period}, Available: {dataList.Count}");

        decimal weightedSum = 0;
        decimal weightTotal = 0;

        for (int i = 0; i < dataList.Count; i++)
        {
            int weight = i + 1; // Linear weights: 1, 2, 3, ..., period
            weightedSum += dataList[i] * weight;
            weightTotal += weight;
        }

        return weightedSum / weightTotal;
    }
}

/// <summary>
/// Hull Moving Average (HMA) - Faster, more responsive moving average
/// HMA = WMA(2 * WMA(n/2) - WMA(n), sqrt(n))
/// </summary>
public class HullMovingAverage : IIndicator<decimal, decimal>
{
    private readonly int _period;
    private readonly int _sqrtPeriod;
    private readonly WeightedMovingAverage _wmaShort;
    private readonly WeightedMovingAverage _wmaLong;
    private readonly WeightedMovingAverage _wmaFinal;

    public HullMovingAverage(int period)
    {
        if (period <= 0)
            throw new ArgumentException("Period must be greater than 0", nameof(period));

        _period = period;
        _sqrtPeriod = (int)Math.Sqrt(period);

        _wmaShort = new WeightedMovingAverage(period / 2);
        _wmaLong = new WeightedMovingAverage(period);
        _wmaFinal = new WeightedMovingAverage(_sqrtPeriod);
    }

    public decimal Calculate(IEnumerable<decimal> data)
    {
        var dataList = data.ToList();

        if (dataList.Count < _period)
            throw new InvalidOperationException($"Insufficient data. Required: {_period}, Available: {dataList.Count}");

        // Calculate intermediate WMAs
        var rawHMA = new List<decimal>();

        for (int i = _period - 1; i < dataList.Count; i++)
        {
            var subset = dataList.Take(i + 1);
            var wmaShort = _wmaShort.Calculate(subset);
            var wmaLong = _wmaLong.Calculate(subset);

            rawHMA.Add(2 * wmaShort - wmaLong);
        }

        // Apply final WMA to smoothed data
        return _wmaFinal.Calculate(rawHMA);
    }
}
