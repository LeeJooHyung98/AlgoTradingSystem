using System.Collections.Concurrent;

namespace AlgoTrading.Application.Services.Strategy.Optimization;

/// <summary>
/// description(설명) : Batch Processor for parallel optimization (병렬 최적화를 위한 배치 프로세서)
/// Details(상세설명) : Processes multiple parameter evaluations in parallel with memory management
/// Applied technology patterns(적용기술패턴) : Parallel Processing, Task-based Asynchrony, Resource Management
/// </summary>
public class BatchProcessor<TInput, TOutput>
{
    private readonly int _maxDegreeOfParallelism;
    private readonly int _batchSize;
    private readonly SemaphoreSlim _throttler;

    /// <summary>
    /// Initialize batch processor with parallelism configuration
    /// </summary>
    /// <param name="maxDegreeOfParallelism">Maximum number of parallel tasks (default: CPU count)</param>
    /// <param name="batchSize">Size of each processing batch (default: 10)</param>
    public BatchProcessor(int? maxDegreeOfParallelism = null, int batchSize = 10)
    {
        _maxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount;
        _batchSize = batchSize;
        _throttler = new SemaphoreSlim(_maxDegreeOfParallelism);
    }

    /// <summary>
    /// Process items in parallel with batching and throttling
    /// </summary>
    /// <param name="items">Items to process</param>
    /// <param name="processor">Processing function</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of results</returns>
    public async Task<List<TOutput>> ProcessAsync(
        IEnumerable<TInput> items,
        Func<TInput, Task<TOutput>> processor,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<TOutput>();
        var itemsList = items.ToList();

        // Process in batches to manage memory
        for (int batchStart = 0; batchStart < itemsList.Count; batchStart += _batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var batch = itemsList
                .Skip(batchStart)
                .Take(_batchSize)
                .ToList();

            // Process batch in parallel with throttling
            var batchTasks = batch.Select(async item =>
            {
                await _throttler.WaitAsync(cancellationToken);
                try
                {
                    var result = await processor(item);
                    results.Add(result);
                }
                finally
                {
                    _throttler.Release();
                }
            });

            await Task.WhenAll(batchTasks);

            // Trigger garbage collection after each batch (optional, for memory-intensive operations)
            if (batchStart % (_batchSize * 5) == 0)
            {
                GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: false);
            }
        }

        return results.ToList();
    }

    /// <summary>
    /// Process items in parallel with progress reporting
    /// </summary>
    public async Task<List<TOutput>> ProcessWithProgressAsync(
        IEnumerable<TInput> items,
        Func<TInput, Task<TOutput>> processor,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<TOutput>();
        var itemsList = items.ToList();
        int completed = 0;

        for (int batchStart = 0; batchStart < itemsList.Count; batchStart += _batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var batch = itemsList
                .Skip(batchStart)
                .Take(_batchSize)
                .ToList();

            var batchTasks = batch.Select(async item =>
            {
                await _throttler.WaitAsync(cancellationToken);
                try
                {
                    var result = await processor(item);
                    results.Add(result);

                    var currentCompleted = Interlocked.Increment(ref completed);
                    progress?.Report(currentCompleted);
                }
                finally
                {
                    _throttler.Release();
                }
            });

            await Task.WhenAll(batchTasks);
        }

        return results.ToList();
    }

    /// <summary>
    /// Process items in parallel and return results in order
    /// </summary>
    public async Task<List<TOutput>> ProcessOrderedAsync(
        IList<TInput> items,
        Func<TInput, Task<TOutput>> processor,
        CancellationToken cancellationToken = default)
    {
        var results = new TOutput[items.Count];

        for (int batchStart = 0; batchStart < items.Count; batchStart += _batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            var batchEnd = Math.Min(batchStart + _batchSize, items.Count);
            var batchTasks = new List<Task>();

            for (int i = batchStart; i < batchEnd; i++)
            {
                var index = i; // Capture for closure
                var item = items[index];

                var task = Task.Run(async () =>
                {
                    await _throttler.WaitAsync(cancellationToken);
                    try
                    {
                        results[index] = await processor(item);
                    }
                    finally
                    {
                        _throttler.Release();
                    }
                }, cancellationToken);

                batchTasks.Add(task);
            }

            await Task.WhenAll(batchTasks);
        }

        return results.ToList();
    }

    /// <summary>
    /// Process items and filter results based on predicate
    /// Useful for early termination optimization
    /// </summary>
    public async Task<List<TOutput>> ProcessAndFilterAsync(
        IEnumerable<TInput> items,
        Func<TInput, Task<TOutput>> processor,
        Func<TOutput, bool> filter,
        int? maxResults = null,
        CancellationToken cancellationToken = default)
    {
        var results = new ConcurrentBag<TOutput>();
        var itemsList = items.ToList();

        for (int batchStart = 0; batchStart < itemsList.Count; batchStart += _batchSize)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (maxResults.HasValue && results.Count >= maxResults.Value)
                break;

            var batch = itemsList
                .Skip(batchStart)
                .Take(_batchSize)
                .ToList();

            var batchTasks = batch.Select(async item =>
            {
                await _throttler.WaitAsync(cancellationToken);
                try
                {
                    var result = await processor(item);
                    if (filter(result))
                    {
                        results.Add(result);
                    }
                }
                finally
                {
                    _throttler.Release();
                }
            });

            await Task.WhenAll(batchTasks);
        }

        return results.Take(maxResults ?? int.MaxValue).ToList();
    }

    public void Dispose()
    {
        _throttler?.Dispose();
    }
}

/// <summary>
/// Batch processing result with statistics
/// </summary>
public class BatchProcessingResult<T>
{
    public List<T> Results { get; set; } = new();
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public TimeSpan Duration { get; set; }
    public double ItemsPerSecond => Duration.TotalSeconds > 0
        ? TotalProcessed / Duration.TotalSeconds
        : 0;
}
