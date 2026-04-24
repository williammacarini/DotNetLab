namespace DotNetLab.Labs.SagaPattern;

using MassTransit;
using System;
using System.Threading.Tasks;

public class PaymentConsumer : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        Console.WriteLine($"[PaymentConsumer] Processing payment of {context.Message.Amount:C} for Order {context.Message.OrderId} using {context.Message.PaymentMethod}...");

        // Simulate some processing delay
        await Task.Delay(2000);

        // Success logic: process if amount <= 1000
        if (context.Message.Amount <= 1000)
        {
            Console.WriteLine($"[PaymentConsumer] Payment for Order {context.Message.OrderId} processed successfully.");
            await context.Publish<PaymentProcessed>(new
            {
                OrderId = context.Message.OrderId,
                TransactionId = Guid.NewGuid().ToString("N")
            });
        }
        else
        {
            Console.WriteLine($"[PaymentConsumer] Payment for Order {context.Message.OrderId} failed: Amount too high.");
            await context.Publish<PaymentFailed>(new
            {
                OrderId = context.Message.OrderId,
                Reason = "Amount exceeds the limit of $1000."
            });
        }
    }
}
