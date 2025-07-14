using Microsoft.EntityFrameworkCore;
using zuHause.Models;

var builder = WebApplication.CreateBuilder(args);

// 會員
builder.Services.AddAuthentication("MemberCookieAuth").AddCookie("MemberCookieAuth", options =>
{
    options.LoginPath = "/Member/Login";
    options.AccessDeniedPath = "/Member/AccessDenied";
});


builder.Services.AddDbContext<ZuHauseContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("zuHauseDBConnstring")));


builder.Services.AddMemoryCache();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// 中介層設定
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

// ? 預設首頁路由：導向 FurnitureController 的 FurnitureHomePage
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Furniture}/{action=FurnitureHomePage}/{id?}");

app.Run();
