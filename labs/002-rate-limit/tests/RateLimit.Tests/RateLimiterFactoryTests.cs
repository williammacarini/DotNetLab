namespace DotNetLab.RateLimit.Tests;

using Xunit;

public class RateLimiterFactoryTests
{
    [Fact]
    public void Create_FixedWindow_ReturnsCorrectType()
    {
        var limiter = RateLimiterFactory.CreateFixedWindow(10, TimeSpan.FromMinutes(1));
        Assert.IsType<FixedWindowRateLimiter>(limiter);
    }

    [Fact]
    public void Create_SlidingWindow_ReturnsCorrectType()
    {
        var limiter = RateLimiterFactory.CreateSlidingWindow(10, TimeSpan.FromMinutes(1));
        Assert.IsType<SlidingWindowRateLimiter>(limiter);
    }

    [Fact]
    public void Create_TokenBucket_ReturnsCorrectType()
    {
        var limiter = RateLimiterFactory.CreateTokenBucket(10, TimeSpan.FromMinutes(1), 1);
        Assert.IsType<TokenBucketRateLimiter>(limiter);
    }

    [Fact]
    public void Create_LeakyBucket_ReturnsCorrectType()
    {
        var limiter = RateLimiterFactory.CreateLeakyBucket(10, TimeSpan.FromMinutes(1));
        Assert.IsType<LeakyBucketRateLimiter>(limiter);
    }

    [Theory]
    [InlineData(RateLimiterType.FixedWindow, typeof(FixedWindowRateLimiter))]
    [InlineData(RateLimiterType.SlidingWindow, typeof(SlidingWindowRateLimiter))]
    [InlineData(RateLimiterType.TokenBucket, typeof(TokenBucketRateLimiter))]
    [InlineData(RateLimiterType.LeakyBucket, typeof(LeakyBucketRateLimiter))]
    public void Create_WithConfig_ReturnsCorrectType(RateLimiterType type, Type expectedType)
    {
        var config = new RateLimiterConfig
        {
            Type = type,
            Limit = 10,
            Window = TimeSpan.FromMinutes(1)
        };

        var limiter = RateLimiterFactory.Create(config);

        Assert.IsType(expectedType, limiter);
    }

    [Fact]
    public void Create_WithSimplifiedParameters_ReturnsCorrectType()
    {
        var limiter = RateLimiterFactory.Create(RateLimiterType.FixedWindow, 10, TimeSpan.FromMinutes(1));
        Assert.IsType<FixedWindowRateLimiter>(limiter);
    }

    [Fact]
    public void Create_UnknownType_ThrowsArgumentException()
    {
        var config = new RateLimiterConfig
        {
            Type = (RateLimiterType)999, // Tipo inválido
            Limit = 10,
            Window = TimeSpan.FromMinutes(1)
        };

        Assert.Throws<ArgumentException>(() => RateLimiterFactory.Create(config));
    }

    [Fact]
    public void Create_NullConfig_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => RateLimiterFactory.Create(null!));
    }
}
