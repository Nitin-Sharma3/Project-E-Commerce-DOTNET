using DeliveryService.Data;
using DeliveryService.HttpClients;
using DeliveryService.Repositories;
using DeliveryService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ── Database — SQL Server ─────────────────────────────────────────────────
builder.Services.AddDbContext<DeliveryDbContext>(o =>
    o.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── Repositories ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IDeliveryRepository, DeliveryRepository>();

// ── Services ──────────────────────────────────────────────────────────────
builder.Services.AddScoped<IDeliveryService, DeliveryService.Services.DeliveryService>();

// ── Typed HttpClients ─────────────────────────────────────────────────────

// OrderAPI: GET api/users/{userId}/orders/{orderId}
//           PATCH api/orders/{orderId}/status
builder.Services.AddHttpClient<IOrderClient, OrderClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["OrderAPI:BaseUrl"]
                   ?? throw new Exception("OrderAPI:BaseUrl not configured"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ProductAPI: GET api/product/{id}
builder.Services.AddHttpClient<IProductClient, ProductClient>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["ProductAPI:BaseUrl"]
                   ?? throw new Exception("ProductAPI:BaseUrl not configured"));
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

// ── Swagger ───────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Delivery Service API", Version = "v1" });
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p =>
    p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ── Auto migration on startup ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DeliveryDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.MapControllers();
app.Run();