using System.Collections.Concurrent;
using System.Diagnostics;

namespace BackendParallel;

public class SystemMonitor(
    BlockingCollection<(int, string)> sorterQueue,
    ConcurrentQueue<(int, string)> primeFinderQueue,
    ConcurrentQueue<(int, string)> writerQueue,
    ConcurrentQueue<(int, string)> primeWriterQueue)
{
    private long _sortedWriteCount = 0;
    private long _primeWriteCount = 0;

    public void IncrementSortedWriteCount()
    {
        Interlocked.Increment(ref _sortedWriteCount);
    }

    public void IncrementPrimeWriteCount()
    {
        Interlocked.Increment(ref _primeWriteCount);
    }

    public async Task MonitorSystem(CancellationToken cancellationToken)
    {
        var startTime = Stopwatch.GetTimestamp();
        var timeCount = Stopwatch.GetElapsedTime(startTime);
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(1000, cancellationToken); 
            var currentTime = Stopwatch.GetElapsedTime(startTime);
            var elapsedSeconds = Stopwatch.GetElapsedTime(startTime).TotalSeconds - timeCount.TotalSeconds;
            var sortedWriteSpeed = _sortedWriteCount / elapsedSeconds;
            var primeWriteSpeed = _primeWriteCount / elapsedSeconds;
            Console.Clear();
            Console.WriteLine($"System Monitor - Running for {currentTime.TotalSeconds:F0} seconds");
            Console.WriteLine($"Queue Sizes:");
            Console.WriteLine($"  Sorter Queue: {sorterQueue.Count}");
            // Console.WriteLine($"  Prime Finder Queue: {primeFinderQueue.Count}");
            Console.WriteLine($"  Sorted Writer Queue: {writerQueue.Count}");
            Console.WriteLine($"  Prime Sorted Writer Queue: {primeWriterQueue.Count}");
            Console.WriteLine($"Memory Usage: {Process.GetCurrentProcess().WorkingSet64 / 1024 / 1024} MB");
            Console.WriteLine($"Write Speeds:");
            Console.WriteLine($"  Sorted Numbers: {sortedWriteSpeed:F2} numbers/second");
            Console.WriteLine($"  Prime Numbers: {primeWriteSpeed:F2} numbers/second");
            timeCount = currentTime;
            _sortedWriteCount = 0;
            _primeWriteCount = 0;
        }
    }
}