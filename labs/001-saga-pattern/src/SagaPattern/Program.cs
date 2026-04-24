using System;
using DotNetLab.Labs.SagaPattern;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// MassTransit Configuration with In-Memory Saga and Consumers
builder.Services.AddMassTransit(x =>
{
    // Register the State Machine and its Saga Repository
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .InMemoryRepository();

    // Register the Consumer
    x.AddConsumer<PaymentConsumer>();

    // Using In-Memory transport for local testing
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

app.MapPost("/orders", async ([FromBody] OrderRequest request, IPublishEndpoint publishEndpoint) =>
{
    var orderId = Guid.NewGuid();
    
    Console.WriteLine($"[API] Order {orderId} received. Publishing OrderSubmitted event...");

    await publishEndpoint.Publish<OrderSubmitted>(new
    {
        OrderId = orderId,
        Amount = request.Amount,
        PaymentMethod = request.PaymentMethod
    });

    return Results.Accepted($"/orders/{orderId}", new { OrderId = orderId });
});

app.MapGet("/", () => "DotNetLab Saga Pattern Demo. Send POST /orders to trigger.");

Console.WriteLine("DotNetLab CLI + Saga Pattern Demo");
Console.WriteLine("API listening on http://localhost:5000");
Console.WriteLine("Use 'curl -X POST http://localhost:5000/orders -H \"Content-Type: application/json\" -d \"{\\\"amount\\\": 100, \\\"paymentMethod\\\": \\\"CreditCard\\\"}\"' to test.");

app.Run("http://localhost:5000");

public record OrderRequest(decimal Amount, string PaymentMethod);
