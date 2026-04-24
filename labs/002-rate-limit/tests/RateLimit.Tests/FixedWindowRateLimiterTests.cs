namespace DotNetLab.RateLimit.Tests;

using Xunit;

public class FixedWindowRateLimiterTests
{
    [Fact]
    public void TryAcquire_WithinLimit_ReturnsTrue()
    {
        var limiter = new FixedWindowRateLimiter(5, TimeSpan.FromMinutes(1));
        var key = "test-user";

        for (int i = 0; i < 5; i++)
        {
            Assert.True(limiter.TryAcquire(key));
        }
    }

    [Fact]
    public void TryAcquire_ExceedsLimit_ReturnsFalse()
    {
        var limiter = new FixedWindowRateLimiter(3, TimeSpan.FromMinutes(1));
        var key = "test-user";

        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.False(limiter.TryAcquire(key));
    }

    [Fact]
    public void TryAcquire_NewWindow_ResetsCount()
    {
        var limiter = new FixedWindowRateLimiter(2, TimeSpan.FromMilliseconds(100));
        var key = "test-user";

        Assert.True(limiter.TryAcquire(key));
        Assert.True(limiter.TryAcquire(key));
        Assert.False(limiter.TryAcquire(key));

        // Aguarda nova janela
        Thread.Sleep(150);

        Assert.True(limiter.TryAcquire(key));
    }

    [Fact]
    public void TryAcquireWithResult_ReturnsCorrectMetadata()
    {
        var limiter = new FixedWindowRateLimiter(10, TimeSpan.FromMinutes(1));
        var key = "test-user";

        var result = limiter.TryAcquireWithResult(key);

        Assert.True(result.IsAllowed);
        Assert.Equal(9, result.Remaining);
        Assert.Equal(10, result.Limit);
        Assert.Null(result.RetryAfter);
    }

    [Fact]
    public void TryAcquireWithResult_WhenLimited_ReturnsRetryAfter()
    {
        var limiter = new FixedWindowRateLimiter(1, TimeSpan.FromMinutes(1));
        var key = "test-user";

        limiter.TryAcquire(key); // Primeira requisição
        var result = limiter.TryAcquireWithResult(key); // Segunda deve ser negada

        Assert.False(result.IsAllowed);
        Assert.NotNull(result.RetryAfter);
        Assert.True(result.RetryAfter.Value > TimeSpan.Zero);
    }

    [Fact]
    public void GetStatus_ReturnsCurrentCount()
    {
        var limiter = new FixedWindowRateLimiter(5, TimeSpan.FromMinutes(1));
        var key = "test-user";

        limiter.TryAcquire(key);
        limiter.TryAcquire(key);

        var status = limiter.GetStatus(key);

        Assert.Equal(2, status.CurrentCount);
        Assert.Equal(5, status.Limit);
    }

    [Fact]
    public void Constructor_ZeroLimit_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FixedWindowRateLimiter(0, TimeSpan.FromMinutes(1)));
    }

    [Fact]
    public void Constructor_ZeroWindow_ThrowsException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new FixedWindowRateLimiter(10, TimeSpan.Zero));
    }

    [Fact]
    public void TryAcquire_DifferentKeys_TrackedIndependently()
    {
        var limiter = new FixedWindowRateLimiter(2, TimeSpan.FromMinutes(1));

        Assert.True(limiter.TryAcquire("user-1"));
        Assert.True(limiter.TryAcquire("user-1"));
        Assert.False(limiter.TryAcquire("user-1"));

        Assert.True(limiter.TryAcquire("user-2"));
        Assert.True(limiter.TryAcquire("user-2"));
        Assert.False(limiter.TryAcquire("user-2"));
    }
}
