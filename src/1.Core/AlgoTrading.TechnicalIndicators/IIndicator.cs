namespace AlgoTrading.TechnicalIndicators;

/// <summary>
/// Base interface for all technical indicators
/// </summary>
/// <typeparam name="TInput">Input data type (e.g., decimal for price)</typeparam>
/// <typeparam name="TOutput">Output result type</typeparam>
public interface IIndicator<in TInput, out TOutput>
{
    /// <summary>
    /// Calculate indicator value from input data
    /// </summary>
    TOutput Calculate(IEnumerable<TInput> data);
}

/// <summary>
/// Interface for indicators that can calculate incrementally
/// </summary>
public interface IIncrementalIndicator<in TInput, TOutput> : IIndicator<TInput, TOutput>
{
    /// <summary>
    /// Add new data point and get updated indicator value
    /// </summary>
    TOutput Update(TInput newValue);

    /// <summary>
    /// Reset indicator state
    /// </summary>
    void Reset();
}
