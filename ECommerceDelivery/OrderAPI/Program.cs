
using Microsoft.EntityFrameworkCore;
using OrderAPI.Data;
using OrderAPI.HttpClients;
using OrderAPI.Repositories;
using OrderAPI.Services;

namespace OrderAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // ── Database ─────────────────────────────────────────────
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ── HttpClient → CartAPI ──────────────────────────────────
            builder.Services.AddHttpClient<ICartClient, CartClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["CartAPI:BaseUrl"]
                                             ?? throw new Exception("CartAPI:BaseUrl not configured"));
            });

            // ── HttpClient → AddressAPI ───────────────────────────────
            builder.Services.AddHttpClient<IAddressClient, AddressClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["AddressAPI:BaseUrl"]
                                             ?? throw new Exception("AddressAPI:BaseUrl not configured"));
            });
            //// ── HttpClient → DeliveryAPI ───────────────────────────────
            //builder.Services.AddHttpClient<IDeliveryClient, DeliveryClient>(client =>
            //{
            //    client.BaseAddress = new Uri(builder.Configuration["DeliveryAPI:BaseUrl"]
            //                                 ?? throw new Exception("DeliveryAPI:BaseUrl not configured"));
            //});
            builder.Services.AddHttpClient<IDeliveryClient, DeliveryClient>(client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["DeliveryAPI:BaseUrl"]
                                     ?? "http://localhost:5016/");
            });

            // ── Repositories ─────────────────────────────────────────
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();

            // ── Services ─────────────────────────────────────────────
            builder.Services.AddScoped<IOrderService, OrderService>();

            // ── Controllers / Swagger ────────────────────────────────
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
