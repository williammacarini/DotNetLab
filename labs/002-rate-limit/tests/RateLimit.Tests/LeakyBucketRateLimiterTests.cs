namespace DotNetLab.RateLimit.Tests;

using Xunit;

public class LeakyBucketRateLimiterTests
{
    [Fact]
    public void TryAcquire_WithinCapacity_ReturnsTrue()
    {
        var limiter = new LeakyBucketRateLimiter(5, TimeSpan.FromMilliseconds(50));
        var key = "test-user";

        for (int i = 0; i < 5; i++)
        {
            Assert.True(limiter.TryAcquire(key));
        }
    }

    [Fact]
    public void TryAcquire_ExceedsCapacity_ReturnsFalse()
    {
        var limiter = new LeakyBucketRateLimiter(3, TimeSpan.FromMilliseconds(100));
        var key = "test-user";

        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.False(limiter.TryAcquire(key));
    }

    [Fact]
    public void TryAcquire_AfterLeak_AllowsNewRequests()
    {
        var limiter = new LeakyBucketRateLimiter(2, TimeSpan.FromMilliseconds(100));
        var key = "test-user";

        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.False(limiter.TryAcquire(key));

        Thread.Sleep(120); // Aguarda leak

        Assert.True(limiter.TryAcquire(key));
    }

    [Fact]
    public void TryAcquireWithResult_ReturnsRemainingCapacity()
    {
        var limiter = new LeakyBucketRateLimiter(5, TimeSpan.FromMilliseconds(100));
        var key = "test-user";

        var result = limiter.TryAcquireWithResult(key);

        Assert.True(result.IsAllowed);
        Assert.Equal(4, result.Remaining);
        Assert.Equal(5, result.Limit);
    }

    [Fact]
    public void TryAcquireWithResult_WhenFull_ReturnsRetryAfter()
    {
        var limiter = new LeakyBucketRateLimiter(1, TimeSpan.FromMilliseconds(200));
        var key = "test-user";

        limiter.TryAcquire(key);
        var result = limiter.TryAcquireWithResult(key);

        Assert.False(result.IsAllowed);
        Assert.NotNull(result.RetryAfter);
        Assert.True(result.RetryAfter.Value > TimeSpan.Zero);
    }

    [Fact]
    public void GetStatus_ReturnsQueueSize()
    {
        var limiter = new LeakyBucketRateLimiter(5, TimeSpan.FromMilliseconds(100));
        var key = "test-user";

        limiter.TryAcquire(key);
        limiter.TryAcquire(key);

        var status = limiter.GetStatus(key);

        Assert.True(status.CurrentCount >= 2);
        Assert.Equal(5, status.Limit);
    }

    [Fact]
    public void Constructor_ZeroCapacity_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LeakyBucketRateLimiter(0, TimeSpan.FromMilliseconds(100)));
    }

    [Fact]
    public void Constructor_ZeroLeakRate_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LeakyBucketRateLimiter(10, TimeSpan.Zero));
    }

    [Fact]
    public void TryAcquire_MultipleItemsLeakOverTime()
    {
        var leakRate = TimeSpan.FromMilliseconds(50);
        var limiter = new LeakyBucketRateLimiter(3, leakRate);
        var key = "test-user";

        // Enche o bucket
        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));

        // Aguarda 2 itens vazarem (aproximadamente)
        Thread.Sleep(120);

        // Deve permitir 2 novos
        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
    }
}
