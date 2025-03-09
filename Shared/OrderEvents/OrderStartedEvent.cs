using Shared.Messages;

namespace Shared.OrderEvents;

public class OrderStartedEvent
{
    public int OrderId { get; set; }
    public int BuyerId { get; set; }
    public List<OrderItemMessage> OrderItems { get; set; }
    public decimal TotalPrice { get; set; }
}
