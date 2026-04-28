
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using OrderAPI.Data;
using OrderAPI.HttpClients;
using OrderAPI.Mapping;
using OrderAPI.MiddleWare;
using OrderAPI.Repositories;
using OrderAPI.Services;
using OrderAPI.Validators;
using Serilog;

namespace OrderAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
     .WriteTo.Console().CreateBootstrapLogger(); 

            try
            {
                Log.Information("Starting OrderAPI...");

                var builder = WebApplication.CreateBuilder(args);

                // ── 1. SERILOG ────────────────────────────────────────
                builder.Host.UseSerilog((ctx, lc) =>
                    lc.ReadFrom.Configuration(ctx.Configuration)
                      .Enrich.FromLogContext()
                      .Enrich.WithProperty("Application", "OrderAPI"));

                // ── 2. DATABASE ───────────────────────────────────────
                builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseSqlServer(
                        builder.Configuration.GetConnectionString("DefaultConnection"),
                        sql => sql.EnableRetryOnFailure(
                            maxRetryCount: 3,
                            maxRetryDelay: TimeSpan.FromSeconds(5),
                            errorNumbersToAdd: null)));

                // ── 3. HTTP CLIENTS ───────────────────────────────────
                builder.Services.AddHttpClient<ICartClient, CartClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["CartAPI:BaseUrl"]
                                                 ?? throw new Exception("CartAPI:BaseUrl not configured"));
                });

                builder.Services.AddHttpClient<IAddressClient, AddressClient>(client =>
                {
                    client.BaseAddress = new Uri(builder.Configuration["AddressAPI:BaseUrl"]
                                                 ?? throw new Exception("AddressAPI:BaseUrl not configured"));
                });

                // ── 4. REPOSITORY PATTERN ─────────────────────────────
                builder.Services.AddScoped<IOrderRepository, OrderRepository>();

                // ── 5. AUTOMAPPER ─────────────────────────────────────
                builder.Services.AddAutoMapper(typeof(OrderMappingProfile));   

                // ── 6. FLUENTVALIDATION ───────────────────────────────
                builder.Services.AddValidatorsFromAssemblyContaining<PlaceOrderDtoValidator>();

                // ── 7. SERVICE LAYER ──────────────────────────────────
                builder.Services.AddScoped<IOrderService, OrderService>();

                // ── 8. CONTROLLERS ────────────────────────────────────
                builder.Services.AddControllers()
                    .ConfigureApiBehaviorOptions(options =>
                    {
                        // Suppress default 400 filter — our middleware + service layer handles it
                        options.SuppressModelStateInvalidFilter = true;
                    });

                builder.Services.AddEndpointsApiExplorer();
                builder.Services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new() { Title = "Order API", Version = "v1" });
                });

                // ─────────────────────────────────────────────────────
                var app = builder.Build();
                // ─────────────────────────────────────────────────────

                // Exception Middleware MUST be first — catches everything below it
                app.UseExceptionMiddleware();

                // Serilog request logging — logs every HTTP request with timing
                app.UseSerilogRequestLogging(opts =>
                {
                    opts.MessageTemplate =
                        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
                });

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
            catch (Exception ex)
            {
                Log.Fatal(ex, "OrderAPI terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }
    }
}
