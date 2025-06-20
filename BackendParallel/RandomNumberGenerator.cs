﻿using System.Collections.Concurrent;

namespace BackendParallel;

public class RandomNumberGenerator(BlockingCollection<(int, string)> outputQueue, string generatorId)
{
    private readonly Random _random = new(Guid.NewGuid().GetHashCode());

    public Task GenerateNumbers(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var number = _random.Next();

            outputQueue.Add((number, generatorId), cancellationToken);
        }

        return Task.CompletedTask;
    }
}