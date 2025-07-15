using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.Data;

var builder = WebApplication.CreateBuilder(args);

// 登入驗證
builder.Services.AddAuthentication("MemberCookieAuth").AddCookie("MemberCookieAuth", options =>
{
    options.LoginPath = "/Member/Login";
    options.AccessDeniedPath = "/Member/AccessDenied";
});


builder.Services.AddDbContext<ZuHauseContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddMemoryCache();

// 註冊 RealDataSeeder
builder.Services.AddScoped<RealDataSeeder>();

// Add services to the container.
builder.Services.AddControllersWithViews();



var app = builder.Build();

// 在開發環境自動執行資料重置和播種
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<RealDataSeeder>();
        try
        {
            await seeder.ResetTestDataAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "資料播種失敗");
        }
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// 亂碼請修正 FurnitureController �� FurnitureHomePage
app.MapControllerRoute(
    name: "default",

    //    pattern: "{controller=Furniture}/{action=FurnitureHomePage}/{id?}");

    //pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{controller=Tenant}/{action=Announcement}/{id?}");



app.Run();
