namespace UserDashboardMVC
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // ── ProductAPI ────────────────────────────────────────
            builder.Services.AddHttpClient("ProductAPI", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["ProductAPI:BaseUrl"] ?? "https://localhost:7000/");
            });

            // ── CartAPI ───────────────────────────────────────────
            builder.Services.AddHttpClient("CartAPI", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["CartAPI:BaseUrl"] ?? "https://localhost:7011/");
            });

            // ── AddressAPI ────────────────────────────────────────
            builder.Services.AddHttpClient("AddressAPI", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["AddressAPI:BaseUrl"] ?? "https://localhost:7012/");
            });

            // ── OrderAPI ──────────────────────────────────────────
            builder.Services.AddHttpClient("OrderAPI", client =>
            {
                client.BaseAddress = new Uri(builder.Configuration["OrderAPI:BaseUrl"] ?? "https://localhost:7013/");
            });

            // ── PostalAPI ─────────────────────────────────────────
            builder.Services.AddHttpClient("PostalAPI", client =>
            {
                client.BaseAddress = new Uri("https://api.postalpincode.in/");
                client.Timeout = TimeSpan.FromSeconds(8);
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

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();


        }
    }
}
