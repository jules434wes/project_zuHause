using Microsoft.EntityFrameworkCore;
using zuHause.Models; // ← Scaffold 出來的 DbContext 命名空間

var builder = WebApplication.CreateBuilder(args);

// ? 註冊 Scaffold 出來的資料庫連線
builder.Services.AddDbContext<ZuHauseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ZuhauseDb")));

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

app.UseAuthorization();

// ? 預設首頁路由：導向 FurnitureController 的 FurnitureHomePage
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Furniture}/{action=FurnitureHomePage}/{id?}");

app.Run();
