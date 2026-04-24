namespace DotNetLab.RateLimit.Tests;

using Xunit;

public class RateLimiterExtensionsTests
{
    [Fact]
    public void ExecuteIfAllowed_WhenAllowed_ExecutesAction()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(5, TimeSpan.FromMinutes(1));
        var executed = false;

        var result = limiter.ExecuteIfAllowed("test-key", () =>
        {
            executed = true;
        });

        Assert.True(result);
        Assert.True(executed);
    }

    [Fact]
    public void ExecuteIfAllowed_WhenDenied_DoesNotExecuteAction()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(1, TimeSpan.FromMinutes(1));
        limiter.TryAcquire("test-key"); // Consome o único permitido
        var executed = false;

        var result = limiter.ExecuteIfAllowed("test-key", () =>
        {
            executed = true;
        });

        Assert.False(result);
        Assert.False(executed);
    }

    [Fact]
    public void ExecuteIfAllowed_WithResult_WhenAllowed_ReturnsValue()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(5, TimeSpan.FromMinutes(1));

        var result = limiter.ExecuteIfAllowed("test-key", () => "success");

        Assert.Equal("success", result);
    }

    [Fact]
    public void ExecuteIfAllowed_WithResult_WhenDenied_ReturnsDefault()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(1, TimeSpan.FromMinutes(1));
        limiter.TryAcquire("test-key");

        var result = limiter.ExecuteIfAllowed("test-key", () => "success");

        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteIfAllowedAsync_WhenAllowed_ExecutesAction()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(5, TimeSpan.FromMinutes(1));
        var executed = false;

        var result = await limiter.ExecuteIfAllowedAsync("test-key", async () =>
        {
            await Task.Delay(1);
            executed = true;
        });

        Assert.True(result);
        Assert.True(executed);
    }

    [Fact]
    public async Task ExecuteIfAllowedAsync_WithResult_WhenAllowed_ReturnsValue()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(5, TimeSpan.FromMinutes(1));

        var result = await limiter.ExecuteIfAllowedAsync("test-key", async () =>
        {
            await Task.Delay(1);
            return "success";
        });

        Assert.Equal("success", result);
    }

    [Fact]
    public void IsAllowed_WhenWithinLimit_ReturnsTrue()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(5, TimeSpan.FromMinutes(1));

        Assert.True(limiter.IsAllowed("test-key"));
    }

    [Fact]
    public void IsAllowed_WhenExceeded_ReturnsFalse()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(1, TimeSpan.FromMinutes(1));
        limiter.TryAcquire("test-key");

        Assert.False(limiter.IsAllowed("test-key"));
    }

    [Fact]
    public void GetRemaining_ReturnsCorrectCount()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(10, TimeSpan.FromMinutes(1));
        limiter.TryAcquire("test-key");
        limiter.TryAcquire("test-key");

        var remaining = limiter.GetRemaining("test-key");

        Assert.Equal(8, remaining);
    }

    [Fact]
    public void GetRateLimitHeaders_ReturnsExpectedHeaders()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(10, TimeSpan.FromMinutes(1));

        var headers = limiter.GetRateLimitHeaders("test-key");

        Assert.True(headers.ContainsKey("X-RateLimit-Limit"));
        Assert.True(headers.ContainsKey("X-RateLimit-Remaining"));
        Assert.True(headers.ContainsKey("X-RateLimit-Reset"));
        Assert.Equal("10", headers["X-RateLimit-Limit"]);
    }

    [Fact]
    public void GetRateLimitHeaders_WhenLimited_IncludesRetryAfter()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(1, TimeSpan.FromMinutes(1));
        limiter.TryAcquire("test-key");

        var headers = limiter.GetRateLimitHeaders("test-key");

        Assert.True(headers.ContainsKey("Retry-After"));
    }

    [Fact]
    public void Builder_FixedWindow_CreatesCorrectLimiter()
    {
        var builder = new RateLimiterBuilder()
            .WithType(RateLimiterType.FixedWindow)
            .WithLimit(100)
            .WithWindow(TimeSpan.FromMinutes(1));

        var limiter = builder.Build();

        Assert.IsType<FixedWindowRateLimiter>(limiter);
        Assert.True(limiter.TryAcquire("test"));
    }

    [Fact]
    public void Builder_PerMinute_SetsCorrectWindow()
    {
        var limiter = new RateLimiterBuilder()
            .PerMinute(60)
            .Build();

        var status = limiter.GetStatus("test");
        Assert.Equal(60, status.Limit);
    }

    [Fact]
    public void Builder_PerHour_SetsCorrectWindow()
    {
        var limiter = new RateLimiterBuilder()
            .PerHour(1000)
            .Build();

        var status = limiter.GetStatus("test");
        Assert.Equal(1000, status.Limit);
    }

    [Fact]
    public void Builder_PerSecond_SetsCorrectWindow()
    {
        var limiter = new RateLimiterBuilder()
            .PerSecond(10)
            .Build();

        var status = limiter.GetStatus("test");
        Assert.Equal(10, status.Limit);
    }
}
