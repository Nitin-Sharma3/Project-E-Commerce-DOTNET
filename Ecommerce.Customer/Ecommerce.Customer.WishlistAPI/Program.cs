using Ecommerce.Customer.WishlistAPI.Data;
using Ecommerce.Customer.WishlistAPI.Repositories;
using Ecommerce.Customer.WishlistAPI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Ecommerce.Customer.WishlistAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<EcommerceCustomerWishlistAPIContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("EcommerceCustomerWishlistAPIContext") ?? throw new InvalidOperationException("Connection string 'EcommerceCustomerWishlistAPIContext' not found.")));

            // Add services to the container.
            builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
            builder.Services.AddHttpClient<ICartApiClient, CartApiClient>(client =>
            {
                client.BaseAddress = new Uri("https://localhost:7011"); // your Cart API 
            });
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddScoped<WishlistService>();
            var app = builder.Build();

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
