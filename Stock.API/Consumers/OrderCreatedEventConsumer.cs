using MassTransit;
using MongoDB.Driver;
using Shared;
using Shared.OrderEvents;
using Shared.StockEvents;
using Stock.API.Services;

namespace Stock.API.Consumers;

public class OrderCreatedEventConsumer(MongoDBService mongoDBService,ISendEndpointProvider sendEndpointProvider) : IConsumer<OrderCreatedEvent>
{
    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        List<bool> stockResults = new();
        var stockCollection = mongoDBService.GetCollection<Models.Stock>();

        foreach (var orderItem in context.Message.OrderItems)
            stockResults.Add(await (await stockCollection.FindAsync(x => x.ProductId == orderItem.ProductId && x.Count >= orderItem.Count)).AnyAsync());
        var sendEndpoint = await sendEndpointProvider.GetSendEndpoint(new Uri($"queue:{RabbitMQSettings.StateMachineQueue}"));

        if (stockResults.TrueForAll(x => x.Equals(true)))
        {
            foreach(var orderItem in context.Message.OrderItems)
            {
                var stock = await(await stockCollection.FindAsync(x => x.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                stock.Count -= orderItem.Count;

                await stockCollection.FindOneAndReplaceAsync(x => x.ProductId == orderItem.ProductId, stock);
            }
            StockReservedEvent stockReservedEvent = new(context.Message.CorrelationId)
            {
                OrderItems = context.Message.OrderItems
            };

            await sendEndpoint.Send(stockReservedEvent);
        }
        else
        {
            StockNotReservedEvent stockNotReservedEvent = new(context.Message.CorrelationId)
            {
                Message = "Stock yetersiz."
            };
            await sendEndpoint.Send(stockNotReservedEvent);
        }
    }
}
