using Microsoft.EntityFrameworkCore;
using CartAPI.Data;
using CartAPI.Repositories;
using CartAPI.Repositories.Interfaces;
using CartAPI.Services;
           


namespace CartAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            

            var builder = WebApplication.CreateBuilder(args);

            // ── Database ─────────────────────────────────────────────
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // ── Repositories ─────────────────────────────────────────
            builder.Services.AddScoped<ICartRepository, CartRepository>();

            // ── Services ─────────────────────────────────────────────
            builder.Services.AddScoped<ICartService, CartService>();

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
