﻿using MassTransit;
using Order.API.Models.Context;
using Shared.OrderEvents;

namespace Order.API.Consumers;

public class OrderFailedEventConsumer(OrderAPIDbContext _context) : IConsumer<OrderFailedEvent>
{
    public async Task Consume(ConsumeContext<OrderFailedEvent> context)
    {
        Order.API.Models.Order order = await _context.Orders.FindAsync(context.Message.OrderId);
        if (order is not null)
        {
            order.Status = Models.OrderStatus.Completed;
            await _context.SaveChangesAsync();
        }
    }
}