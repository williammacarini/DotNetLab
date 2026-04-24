namespace DotNetLab.Labs.SagaPattern;

using System;

public record OrderSubmitted
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
}

public record ProcessPayment
{
    public Guid OrderId { get; init; }
    public decimal Amount { get; init; }
    public string PaymentMethod { get; init; } = string.Empty;
}

public record PaymentProcessed
{
    public Guid OrderId { get; init; }
    public string TransactionId { get; init; } = string.Empty;
}

public record PaymentFailed
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
