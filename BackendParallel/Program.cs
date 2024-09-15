using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace BackendParallel;
internal abstract class Program
{
    private const int Batch = 1000;

    private static async Task Main(string[] args)
    {
        var sorterQueue = new BlockingCollection<(int, string)>(Batch);
        var primeFinderQueue = new ConcurrentQueue<(int, string)>();
        var writerQueue = new ConcurrentQueue<(int, string)>();
        var primeWriterQueue = new ConcurrentQueue<(int, string)>();
        var cts = new CancellationTokenSource();
        var monitor = new SystemMonitor(sorterQueue, primeFinderQueue, writerQueue, primeWriterQueue);
        var gen1 = Task.Run(() => new RandomNumberGenerator(sorterQueue, 1).GenerateNumbers(cts.Token), cts.Token);
        var gen2 = Task.Run(() => new RandomNumberGenerator(sorterQueue, 2).GenerateNumbers(cts.Token), cts.Token);
        var gen3 = Task.Run(() => new RandomNumberGenerator(sorterQueue, 3).GenerateNumbers(cts.Token), cts.Token);
        var sorter = Task.Run(() => new Sorter(sorterQueue, primeFinderQueue, writerQueue).SortNumbers(cts.Token),
            cts.Token);
        var primeFinder =
            Task.Run(() => new PrimeNumberFinder(primeFinderQueue, primeWriterQueue).FindPrimes(cts.Token),
                cts.Token);
        var sortedWriter = Task.Factory.StartNew(
            () => new FileWriter(writerQueue, "sorted.txt", "sorted", monitor).WriteNumbersAsync(cts.Token),
            cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        var primeSorted = Task.Factory.StartNew(
            () => new FileWriter(primeWriterQueue, "primes.txt", "prime", monitor).WriteNumbersAsync(cts.Token),
            cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        var monitorTask = Task.Run(() => monitor.MonitorSystem(cts.Token), cts.Token);
        await Task.Delay(30000, cts.Token);
        await cts.CancelAsync();
        
        var allTasks = new[] { gen1, gen2, gen3, sorter, primeFinder, sortedWriter, primeSorted, monitorTask };
        await Task.WhenAll(allTasks);
    }
}
