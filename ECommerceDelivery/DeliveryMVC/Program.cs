using DeliveryMVC.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddScoped<DeliveryApiService>();

builder.Services.AddHttpClient("DeliveryApi", c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:DeliveryApi"]
                   ?? "http://localhost:5016/");
    c.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// ── Route order matters ───────────────────────────────────────────────────
app.MapControllerRoute(
    name: "livemap",
    pattern: "Delivery/LiveMap",
    defaults: new { controller = "Delivery", action = "LiveMap" });

app.MapControllerRoute(
    name: "tracking",
    pattern: "Delivery/Tracking/{trackingId}",
    defaults: new { controller = "Delivery", action = "Tracking" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Delivery}/{action=Index}/{id?}");

app.Run();