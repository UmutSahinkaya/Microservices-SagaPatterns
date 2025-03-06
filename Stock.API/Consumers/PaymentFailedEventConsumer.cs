using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class PaymentFailedEventConsumer(MongoDBService mongoDBService) : IConsumer<PaymentFailedEvent>
{
    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var stocks = mongoDBService.GetCollection<Models.Stock>();
        foreach (var orderItem in context.Message.OrderItems)
        {
            var stock = await stocks.Find(s => s.ProductId == orderItem.ProductId).FirstOrDefaultAsync();
            if (stock != null)
            {
                stock.Count += orderItem.Count;
                await stocks.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
            }
                

        }
    }
}
