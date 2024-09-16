using System.Collections.Concurrent;

namespace BackendParallel;
internal abstract class Program
{
    private const int Batch = 1000;
    private const int Timeout = 30000;
    private static async Task Main(string[] args)
    {
        var sorterQueue = new BlockingCollection<(int, string)>(Batch);
        var primeFinderQueue = new ConcurrentQueue<(int, string)>();
        var writerQueue = new ConcurrentQueue<(int, string)>();
        var primeWriterQueue = new ConcurrentQueue<(int, string)>();
         var cts = new CancellationTokenSource();
        var monitor = new SystemMonitor(sorterQueue, primeFinderQueue, writerQueue, primeWriterQueue);

        var tasks = new[]
        {
            Task.Factory.StartNew(() => new RandomNumberGenerator(sorterQueue, "RNG1").GenerateNumbers(cts.Token), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default),
            Task.Factory.StartNew(() => new RandomNumberGenerator(sorterQueue, "RNG2").GenerateNumbers(cts.Token), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default),
            Task.Factory.StartNew(() => new RandomNumberGenerator(sorterQueue, "RNG3").GenerateNumbers(cts.Token), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default),
            Task.Factory.StartNew(() => new Sorter(sorterQueue, primeFinderQueue, writerQueue).SortNumbers(cts.Token), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default),
            Task.Factory.StartNew(() => new PrimeNumberFinder(primeFinderQueue, primeWriterQueue).FindPrimes(cts.Token), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default),
            Task.Factory.StartNew(() => new FileWriter(writerQueue, "sorted.txt", "sorted", monitor).WriteNumbersAsync(cts.Token), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default),
            Task.Factory.StartNew(() => new FileWriter(primeWriterQueue, "primes.txt", "prime", monitor).WriteNumbersAsync(cts.Token), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default),
            Task.Factory.StartNew(() => monitor.MonitorSystem(cts.Token), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default)
        };

        await Task.Delay(Timeout, cts.Token);
        await cts.CancelAsync();
        await Task.WhenAll(tasks);
    }
}