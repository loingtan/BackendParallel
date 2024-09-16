using System.Collections.Concurrent;

namespace BackendParallel;

public class PrimeNumberFinder(ConcurrentQueue<(int, string)> inputQueue, ConcurrentQueue<(int, string)> outputQueue)
{   private const int DelayMilliseconds = 1; 
    public async Task FindPrimes(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            if (inputQueue.TryDequeue(out var val))
            {
                if (IsPrime(val.Item1))
                {
                    outputQueue.Enqueue(val);
                }
            }
            else
            {
                await Task.Delay(DelayMilliseconds, cancellationToken);
            }
        }
    }

    private static bool IsPrime(int number)
    {
        switch (number)
        {
            case <= 1:
                return false;
            case 2:
                return true;
            case 3:
                return true;
        }
        if (number % 2 == 0 || number % 3 == 0) return false;
        for (var i = 5; i * i <= number; i += 6)
        {
            if (number % i == 0 || number % (i + 2) == 0) return false;
        }
        return true;
    }
}