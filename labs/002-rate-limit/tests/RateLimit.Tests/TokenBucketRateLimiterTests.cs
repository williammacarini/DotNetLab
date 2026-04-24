namespace DotNetLab.RateLimit.Tests;

using Xunit;

public class TokenBucketRateLimiterTests
{
    [Fact]
    public void TryAcquire_WithinCapacity_ReturnsTrue()
    {
        var limiter = new TokenBucketRateLimiter(5, TimeSpan.FromSeconds(1), 5);
        var key = "test-user";

        for (int i = 0; i < 5; i++)
        {
            Assert.True(limiter.TryAcquire(key));
        }
    }

    [Fact]
    public void TryAcquire_ExceedsCapacity_ReturnsFalse()
    {
        var limiter = new TokenBucketRateLimiter(3, TimeSpan.FromSeconds(1), 3);
        var key = "test-user";

        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.False(limiter.TryAcquire(key));
    }

    [Fact]
    public void TryAcquire_AfterRefill_AllowsNewRequests()
    {
        var limiter = new TokenBucketRateLimiter(2, TimeSpan.FromMilliseconds(100), 2);
        var key = "test-user";

        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.False(limiter.TryAcquire(key));

        Thread.Sleep(150); // Aguarda refill

        Assert.True(limiter.TryAcquire(key));
    }

    [Fact]
    public void TryAcquire_Burst_AllowsRapidRequests()
    {
        // Bucket com capacidade 10, permite burst de 10 requisições imediatas
        var limiter = new TokenBucketRateLimiter(10, TimeSpan.FromSeconds(1), 1);
        var key = "test-user";

        for (int i = 0; i < 10; i++)
        {
            Assert.True(limiter.TryAcquire(key));
        }

        Assert.False(limiter.TryAcquire(key));
    }

    [Fact]
    public void TryAcquireWithResult_ReturnsRemainingTokens()
    {
        var limiter = new TokenBucketRateLimiter(5, TimeSpan.FromSeconds(1), 1);
        var key = "test-user";

        var result = limiter.TryAcquireWithResult(key);

        Assert.True(result.IsAllowed);
        Assert.Equal(4, result.Remaining);
        Assert.Equal(5, result.Limit);
    }

    [Fact]
    public void TryAcquireWithResult_WhenEmpty_ReturnsRetryAfter()
    {
        var limiter = new TokenBucketRateLimiter(1, TimeSpan.FromSeconds(2), 1);
        var key = "test-user";

        limiter.TryAcquire(key); // Consome único token
        var result = limiter.TryAcquireWithResult(key);

        Assert.False(result.IsAllowed);
        Assert.NotNull(result.RetryAfter);
    }

    [Fact]
    public void GetStatus_ReturnsAvailableTokens()
    {
        var limiter = new TokenBucketRateLimiter(5, TimeSpan.FromSeconds(1), 1);
        var key = "test-user";

        limiter.TryAcquire(key);
        limiter.TryAcquire(key);

        var status = limiter.GetStatus(key);

        // CurrentCount representa tokens consumidos
        Assert.True(status.CurrentCount >= 2);
        Assert.Equal(5, status.Limit);
    }

    [Fact]
    public void Constructor_ZeroCapacity_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TokenBucketRateLimiter(0, TimeSpan.FromSeconds(1), 1));
    }

    [Fact]
    public void Constructor_ZeroRefillPeriod_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TokenBucketRateLimiter(10, TimeSpan.Zero, 1));
    }

    [Fact]
    public void Constructor_ZeroRefillAmount_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new TokenBucketRateLimiter(10, TimeSpan.FromSeconds(1), 0));
    }

    [Fact]
    public void Constructor_WithRatePerSecond()
    {
        // 10 tokens por segundo
        var limiter = new TokenBucketRateLimiter(10, 10.0);
        var key = "test-user";

        // Permite burst inicial
        for (int i = 0; i < 10; i++)
        {
            Assert.True(limiter.TryAcquire(key));
        }

        Assert.False(limiter.TryAcquire(key));
    }
}
