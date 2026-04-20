using Ecommerce.Customer.CartAPI.Data;
using Ecommerce.Customer.CartAPI.ExceptionHandling;
using Ecommerce.Customer.CartAPI.Repositories;
using Ecommerce.Customer.CartAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Customer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<EcommerceCustomerCartAPIContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("EcommerceCustomerCartAPIContext") ?? throw new InvalidOperationException("Connection string 'EcommerceCustomerCartAPIContext' not found.")));

            // Add services to the container.
            builder.Services.AddScoped<ICartRepository, CartRepository>();
            builder.Services.AddScoped<ICartService, CartService>();
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseMiddleware<GlobalExceptionMiddleware>();
            // Configure the HTTP request pipeline.
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
