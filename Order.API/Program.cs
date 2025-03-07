using MassTransit;
using Microsoft.EntityFrameworkCore;
using Order.API.Models.Context;

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




app.Run();
