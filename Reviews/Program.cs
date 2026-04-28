using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Reviews.MongoDBSettings;
using Reviews.Repositories;
using Reviews.Services;
using System.Text;

namespace Reviews
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure MongoDB Settings
            builder.Services.Configure<MongoDbSettings>(
                builder.Configuration.GetSection("MongoDbSettings"));

            // Add Fluent Validation
            builder.Services.AddFluentValidationAutoValidation();
            builder.Services.AddValidatorsFromAssemblyContaining<Program>();

            // Add Razor Pages
            builder.Services.AddRazorPages();

            // Add HTTP Client for Order Service with configuration
            var orderServiceUrl = builder.Configuration["Services:OrderServiceUrl"] ?? "https://localhost:7013";
            builder.Services.AddHttpClient<IOrderService, OrderService>(client =>
            {
                client.BaseAddress = new Uri(orderServiceUrl);
                client.Timeout = TimeSpan.FromSeconds(10);
            })
            .ConfigureHttpClient(client =>
            {
                client.DefaultRequestHeaders.Add("User-Agent", "ReviewService/1.0");
            });

            builder.Services.AddHttpClient();

            // Add Services
            builder.Services.AddScoped<IReviewSummaryService, ReviewSummaryService>();

            // Add Repositories
            builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
            builder.Services.AddScoped<IReviewVoteRepository, ReviewVoteRepository>();
            builder.Services.AddScoped<IReviewReportRepository, ReviewReportRepository>();

            // Configure JWT Authentication
            var key = builder.Configuration["Jwt:SecretKey"] ??
                "ECOMMERCE_Q_W_E_R_T_Y_U_I_O_P_A_S_D_F_G_H_J_K_L_Z_X_C_V_B_N_M_WEBSITE";

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = builder.Environment.IsProduction();
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
                };
            });

            builder.Services.AddAuthorization();

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowLocalhost", policy =>
                {
                    policy
                        .WithOrigins("https://localhost:7000", "http://localhost:3000", "https://localhost:3000")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowCredentials();
                });

                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });

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
            app.UseStaticFiles();

            // Use CORS
            app.UseCors("AllowLocalhost");

            app.MapRazorPages();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapGet("/", () => Results.Redirect("/Reviews"));
            app.MapControllers();

            app.Run();
        }
    }
}