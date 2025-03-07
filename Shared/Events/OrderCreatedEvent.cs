using Shared.Messages;

namespace Shared.Events;

public class OrderCreatedEvent:IEvent
{
    public Guid OrderId { get; set; }
    public Guid BuyerId { get; set; }
    public decimal TotalPrice { get; set; }
    public List<OrderItemMessage> OrderItems { get; set; }
}
