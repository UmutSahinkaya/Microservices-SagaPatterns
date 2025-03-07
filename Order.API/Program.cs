using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models;
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

app.MapPost("/create-order", async (OrderVM model,OrderAPIDbContext _context) =>
{
    Order.API.Models.Order order = new()
    {
        BuyerId = model.BuyerId,
        CreatedDate = DateTime.UtcNow,
        Status = OrderStatus.Suspend,
        TotalPrice = model.OrderItems.Sum(x => x.Price * x.Count),
        OrderItems = model.OrderItems.Select(oi => new OrderItem
        {
            Price= oi.Price,
            Count=oi.Count,
            ProductId=oi.ProductId
        }).ToList()
    };

    await _context.Orders.AddAsync(order);
    await _context.SaveChangesAsync();
});


app.Run();
