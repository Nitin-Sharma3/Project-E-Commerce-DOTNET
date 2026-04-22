using DeliveryService.Data;
using DeliveryService.Repositories;
using DeliveryService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// EF InMemory (swap to UseSqlServer for production)
builder.Services.AddDbContext<DeliveryDbContext>(o =>
    o.UseInMemoryDatabase("DeliveryDb"));

// DI
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();
builder.Services.AddScoped<IDeliveryService, DeliveryServices>();

// HttpClient for future Order Service integration
builder.Services.AddHttpClient("OrderService", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:OrderService"] ?? "http://localhost:5013/");
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// Seed DB
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.Run();