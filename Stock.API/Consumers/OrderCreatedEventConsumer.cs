using MassTransit;
using MongoDB.Driver;
using Shared.Events;
using Stock.API.Services;

namespace Stock.API.Consumers
{
    public class OrderCreatedEventConsumer(MongoDBService _mongoDbService) : IConsumer<OrderCreatedEvent>
    { 
        public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
        {
            List<bool> StockResult = new();
            IMongoCollection<Models.Stock> collection=_mongoDbService.GetCollection<Models.Stock>();

            foreach(var orderItem in context.Message.OrderItems)
            {
                StockResult.Add(await(await collection.FindAsync(s=>s.ProductId==orderItem.ProductId && s.Count >= orderItem.Count)).AnyAsync());
            }
            if (StockResult.TrueForAll(s=>s.Equals(true)))
            {
                //Stock Güncellemesi
                foreach(var orderItem in context.Message.OrderItems)
                {
                    Models.Stock stock = await (await collection.FindAsync(s => s.ProductId == orderItem.ProductId)).FirstOrDefaultAsync();
                    stock.Count-=orderItem.Count;
                    await collection.FindOneAndReplaceAsync(x=>x.ProductId==orderItem.ProductId,stock);
                }

                //Payment uyarılacak event fırlatılması

            }
            else
            {
                //Stock işlemi Başarısız
                //Order'ı Uyaracak eventini at.
            }

        }
    }
}
