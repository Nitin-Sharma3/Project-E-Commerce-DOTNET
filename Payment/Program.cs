using RazorpayApi.Models;
using RazorpayApi.Services;

var builder = WebApplication.CreateBuilder(args);

// ── Configuration ────────────────────────────────────────────────────────────
builder.Services.Configure<RazorpaySettings>(
    builder.Configuration.GetSection("Razorpay"));

// ── Services ─────────────────────────────────────────────────────────────────
builder.Services.AddScoped<IRazorpayService, RazorpayService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();

// ── Session Configuration (✅ ADDED) ─────────────────────────────────────────
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader());
});

// ── Web API + Razor Pages ────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddRazorPages();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Razorpay API with Invoice", Version = "v1" });
});

var app = builder.Build();

// ── Middleware pipeline ──────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable CORS
app.UseCors("AllowAll");

// ── Session Middleware (✅ ADDED HERE - VERY IMPORTANT POSITION) ─────────────
app.UseSession();

app.UseAuthorization();

app.MapControllers();
app.MapRazorPages();
app.MapGet("/", () => Results.Redirect("/Payment"));

app.Run();