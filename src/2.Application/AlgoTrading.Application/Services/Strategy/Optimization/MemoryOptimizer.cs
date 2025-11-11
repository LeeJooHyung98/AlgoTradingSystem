using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime;

namespace AlgoTrading.Application.Services.Strategy.Optimization;

/// <summary>
/// description(설명) : Memory Optimizer for large-scale optimizations (대규모 최적화를 위한 메모리 최적화기)
/// Details(상세설명) : Monitors memory usage and provides optimization strategies
/// Applied technology patterns(적용기술패턴) : Resource Management, Performance Optimization
/// </summary>
public class MemoryOptimizer
{
    private readonly long _memoryThresholdBytes;
    private readonly double _memoryThresholdPercent;

    /// <summary>
    /// Initialize memory optimizer
    /// </summary>
    /// <param name="memoryThresholdMB">Memory threshold in MB (default: 1024 MB)</param>
    /// <param name="memoryThresholdPercent">Memory threshold as percentage of total (default: 75%)</param>
    public MemoryOptimizer(long memoryThresholdMB = 1024, double memoryThresholdPercent = 0.75)
    {
        _memoryThresholdBytes = memoryThresholdMB * 1024 * 1024;
        _memoryThresholdPercent = memoryThresholdPercent;
    }

    /// <summary>
    /// Get current memory usage information
    /// </summary>
    public MemoryInfo GetMemoryInfo()
    {
        var process = Process.GetCurrentProcess();

        return new MemoryInfo
        {
            WorkingSetBytes = process.WorkingSet64,
            PrivateMemoryBytes = process.PrivateMemorySize64,
            VirtualMemoryBytes = process.VirtualMemorySize64,
            ManagedMemoryBytes = GC.GetTotalMemory(forceFullCollection: false),
            Gen0Collections = GC.CollectionCount(0),
            Gen1Collections = GC.CollectionCount(1),
            Gen2Collections = GC.CollectionCount(2)
        };
    }

    /// <summary>
    /// Check if memory usage exceeds threshold
    /// </summary>
    public bool IsMemoryPressureHigh()
    {
        var memInfo = GetMemoryInfo();

        // Check absolute threshold
        if (memInfo.ManagedMemoryBytes > _memoryThresholdBytes)
            return true;

        // Check percentage threshold (if available)
        var totalMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes;
        if (totalMemory > 0)
        {
            var usagePercent = (double)memInfo.ManagedMemoryBytes / totalMemory;
            if (usagePercent > _memoryThresholdPercent)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Optimize memory usage when threshold is exceeded
    /// </summary>
    public void OptimizeMemory(bool aggressive = false)
    {
        if (aggressive)
        {
            // Aggressive cleanup
            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        }
        else
        {
            // Normal cleanup
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Optimized, blocking: false);
        }
    }

    /// <summary>
    /// Trim working set to release unused memory back to OS
    /// </summary>
    public void TrimWorkingSet()
    {
        GC.Collect(GC.MaxGeneration, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        GC.WaitForPendingFinalizers();

        // Optionally trim the process working set
        // This is Windows-specific
        if (OperatingSystem.IsWindows())
        {
            try
            {
                var process = Process.GetCurrentProcess();
                NativeMethods.EmptyWorkingSet(process.Handle);
            }
            catch
            {
                // Ignore if not available
            }
        }
    }

    /// <summary>
    /// Estimate memory usage for optimization run
    /// </summary>
    public long EstimateMemoryUsage(int parameterCount, int evaluationCount, int backtestDays)
    {
        // Rough estimates (in bytes)
        const long bytesPerParameter = 8; // double precision
        const long bytesPerEvaluation = 1024; // approximate overhead
        const long bytesPerBacktestDay = 100; // approximate market data per day

        long estimatedBytes =
            (parameterCount * bytesPerParameter * evaluationCount) +
            (bytesPerEvaluation * evaluationCount) +
            (bytesPerBacktestDay * backtestDays * evaluationCount);

        return estimatedBytes;
    }

    /// <summary>
    /// Suggest optimal batch size based on available memory
    /// </summary>
    public int SuggestBatchSize(int totalItems, long bytesPerItem)
    {
        var memInfo = GetMemoryInfo();
        var availableMemory = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes - memInfo.ManagedMemoryBytes;

        // Use 50% of available memory for batch
        var batchMemory = availableMemory / 2;
        var suggestedBatch = (int)(batchMemory / bytesPerItem);

        // Clamp to reasonable values
        return Math.Clamp(suggestedBatch, 1, Math.Min(totalItems, 100));
    }
}

/// <summary>
/// Memory information snapshot
/// </summary>
public class MemoryInfo
{
    public long WorkingSetBytes { get; set; }
    public long PrivateMemoryBytes { get; set; }
    public long VirtualMemoryBytes { get; set; }
    public long ManagedMemoryBytes { get; set; }
    public int Gen0Collections { get; set; }
    public int Gen1Collections { get; set; }
    public int Gen2Collections { get; set; }

    public double WorkingSetMB => WorkingSetBytes / (1024.0 * 1024.0);
    public double PrivateMemoryMB => PrivateMemoryBytes / (1024.0 * 1024.0);
    public double ManagedMemoryMB => ManagedMemoryBytes / (1024.0 * 1024.0);

    public override string ToString()
    {
        return $"Working Set: {WorkingSetMB:F2} MB, Managed: {ManagedMemoryMB:F2} MB, " +
               $"GC: Gen0={Gen0Collections}, Gen1={Gen1Collections}, Gen2={Gen2Collections}";
    }
}

/// <summary>
/// Object pool for reusing expensive objects
/// </summary>
public class ObjectPool<T> where T : class, new()
{
    private readonly ConcurrentQueue<T> _objects = new();
    private readonly int _maxSize;
    private int _currentSize;

    public ObjectPool(int maxSize = 100)
    {
        _maxSize = maxSize;
    }

    public T Rent()
    {
        if (_objects.TryDequeue(out var obj))
        {
            Interlocked.Decrement(ref _currentSize);
            return obj;
        }

        return new T();
    }

    public void Return(T obj)
    {
        if (_currentSize < _maxSize)
        {
            _objects.Enqueue(obj);
            Interlocked.Increment(ref _currentSize);
        }
    }

    public void Clear()
    {
        _objects.Clear();
        _currentSize = 0;
    }
}

/// <summary>
/// Native methods for Windows-specific optimizations
/// </summary>
internal static class NativeMethods
{
    [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
    [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
    internal static extern bool EmptyWorkingSet(IntPtr hProcess);
}
