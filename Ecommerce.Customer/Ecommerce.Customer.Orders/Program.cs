using System;
using Ecommerce.Customer.OrderAPI.Data;
using Ecommerce.Customer.OrderAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Ecommerce.Customer.OrderAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            Console.WriteLine("CartApi: " + builder.Configuration["Services:CartApi"]);
            Console.WriteLine("Env: " + builder.Environment.EnvironmentName);
            // Database
            builder.Services.AddDbContext<OrderDbContext>(opt =>
                opt.UseSqlServer(builder.Configuration.GetConnectionString("OrderDb")));

            // Application services
            builder.Services.AddScoped<IOrderService, OrderService>();
            Console.WriteLine(builder.Configuration["Services:CartApi"]);
            // Typed HTTP clients pointing to the other microservices
            builder.Services.AddHttpClient<ICartServiceClient, CartServiceClient>(c =>
            {
                var baseAddr = builder.Configuration["Services:CartApi"];
                if (string.IsNullOrWhiteSpace(baseAddr))
                    throw new InvalidOperationException("Configuration key 'Services:CartApi' is not set.");
                c.BaseAddress = new Uri(baseAddr);
            });

            builder.Services.AddHttpClient<IAddressServiceClient, AddressServiceClient>(c =>
            {
                var baseAddr = builder.Configuration["Services:AddressApi"];
                if (string.IsNullOrWhiteSpace(baseAddr))
                    throw new InvalidOperationException("Configuration key 'Services:AddressApi' is not set.");
                c.BaseAddress = new Uri(baseAddr);
            });

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();
        }
    }
}
