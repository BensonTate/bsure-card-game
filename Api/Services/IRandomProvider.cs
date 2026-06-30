namespace Api.Services;

// Abstraction over randomness so DealingService is testable.
public interface IRandomProvider
{
    // Returns a non-negative random integer less than maxExclusive.
    int Next(int maxExclusive);
}