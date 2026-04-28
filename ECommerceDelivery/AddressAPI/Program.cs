
using AddressAPI.Data;
using AddressAPI.Repositories;
using AddressAPI.Services;
using Microsoft.EntityFrameworkCore;
         
namespace AddressAPI
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
            builder.Services.AddScoped<IAddressRepository, AddressRepository>();

            // ── Services ─────────────────────────────────────────────
            builder.Services.AddScoped<IAddressService, AddressService>();

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
