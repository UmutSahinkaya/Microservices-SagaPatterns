using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models.Context;
using Order.API.ViewModels;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMassTransit(configurator =>
{
    configurator.UsingRabbitMq((context, _configure) =>
    {
        _configure.Host(builder.Configuration["RabbitMQ"]);
    });
});

builder.Services.AddDbContext<OrderAPIDbContext>(opt =>
{
    opt.UseSqlServer(builder.Configuration.GetConnectionString("SqlSERVER"));
});

var app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI();

app.MapPost("/create-order", async (CreateOrderVM model,OrderAPIDbContext context) =>
{
    Order.API.Models.Order order = new()
    {
        BuyerId = Guid.TryParse(model.BuyerId, out Guid _buyerId) ? _buyerId : Guid.NewGuid(),
        OrderItems = model.OrderItems.Select(oi => new Order.API.Models.OrderItem()
        {
            Count = oi.Count,
            Price = oi.Price,
            ProductId = Guid.Parse(oi.ProductId)
        }).ToList(),
        Statu=Order.API.Models.OrderStatus.Suspend,
        CreatedDate = DateTime.UtcNow,
        TotalPrice=model.OrderItems.Sum(oi => oi.Price*oi.Count),
    };
   await context.Orders.AddAsync(order);
    await context.SaveChangesAsync();
});


app.Run();
