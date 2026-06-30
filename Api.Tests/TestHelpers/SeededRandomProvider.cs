using Api.Services;

namespace Api.Tests.TestHelpers;

public class SeededRandomProvider : IRandomProvider
{
    private readonly Random _random;

    public SeededRandomProvider(int seed)
    {
        _random = new Random(seed);
    }

    public int Next(int maxExclusive) => _random.Next(maxExclusive);
}