using Microsoft.EntityFrameworkCore;
using zuHause.Models; // 確保這是 ZuHauseContext 正確的命名空間

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ZuHauseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("zuhause"))); // 或根據您實際的資料庫提供者使用 UseSqlite, UsePostgreSQL 等


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    //pattern: "{controller=Tenant}/{action=Index}/{id?}");
    //pattern: "{controller=Tenant}/{action=Index}/{id?}");
    //pattern: "{controller=Tenant}/{action=Index}/{id?}");
    pattern: "{controller=Tenant}/{action=Announcement}/{id?}");


app.Run();
