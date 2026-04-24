namespace DotNetLab.RateLimit.Tests;

using Xunit;

public class SlidingWindowRateLimiterTests
{
    [Fact]
    public void TryAcquire_WithinLimit_ReturnsTrue()
    {
        var limiter = new SlidingWindowRateLimiter(5, TimeSpan.FromMinutes(1));
        var key = "test-user";

        for (int i = 0; i < 5; i++)
        {
            Assert.True(limiter.TryAcquire(key));
        }
    }

    [Fact]
    public void TryAcquire_ExceedsLimit_ReturnsFalse()
    {
        var limiter = new SlidingWindowRateLimiter(3, TimeSpan.FromMinutes(1));
        var key = "test-user";

        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.False(limiter.TryAcquire(key));
    }

    [Fact]
    public void TryAcquire_OldRequestsRemovedFromWindow()
    {
        var limiter = new SlidingWindowRateLimiter(2, TimeSpan.FromMilliseconds(200));
        var key = "test-user";

        Assert.True(limiter.TryAcquire(key));
        Thread.Sleep(150);
        Assert.True(limiter.TryAcquire(key));
        Assert.False(limiter.TryAcquire(key)); // Terceira deve falhar

        Thread.Sleep(100); // Primeira saiu da janela

        Assert.True(limiter.TryAcquire(key)); // Agora deve permitir
    }

    [Fact]
    public void TryAcquireWithResult_ReturnsCorrectMetadata()
    {
        var limiter = new SlidingWindowRateLimiter(5, TimeSpan.FromMinutes(1));
        var key = "test-user";

        var result = limiter.TryAcquireWithResult(key);

        Assert.True(result.IsAllowed);
        Assert.Equal(4, result.Remaining);
        Assert.Equal(5, result.Limit);
    }

    [Fact]
    public void GetStatus_ReflectsCurrentQueueSize()
    {
        var limiter = new SlidingWindowRateLimiter(5, TimeSpan.FromMinutes(1));
        var key = "test-user";

        limiter.TryAcquire(key);
        limiter.TryAcquire(key);

        var status = limiter.GetStatus(key);

        Assert.Equal(2, status.CurrentCount);
        Assert.Equal(5, status.Limit);
    }

    [Fact]
    public void Constructor_ZeroMaxRequests_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SlidingWindowRateLimiter(0, TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void Constructor_ZeroWindowSize_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SlidingWindowRateLimiter(10, TimeSpan.Zero));
    }
}
