using MassTransit;
using Order.API.Models.Context;
using Shared.Events;

namespace Order.API.Consumers;

public class PaymentFailedEventConsumer(OrderAPIDbContext _context) : IConsumer<PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var order = await _context.Orders.FindAsync(context.Message.OrderId);
        if (order is null)
            throw new NullReferenceException();
        order.Statu = Models.OrderStatus.Failed;
        await _context.SaveChangesAsync();
    }
}
