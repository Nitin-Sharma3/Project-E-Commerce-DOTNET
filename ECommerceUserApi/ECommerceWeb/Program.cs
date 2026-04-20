//using ECommerceWeb.Services;

//var builder = WebApplication.CreateBuilder(args);

//// ✅ Add MVC
//builder.Services.AddControllersWithViews();

//// ✅ Add HttpClient for API
//builder.Services.AddHttpClient<ApiService>();

//// ✅ Add Session
//builder.Services.AddSession();

//var app = builder.Build();

//// ✅ Middleware
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();

//// ✅ Enable Session
//app.UseSession();

//app.UseAuthorization();

//// ✅ Routing
//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();