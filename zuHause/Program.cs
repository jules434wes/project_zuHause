using Microsoft.EntityFrameworkCore;

using zuHause.Models; // 確保這是 ZuHauseContext 正確的命名空間


var builder = WebApplication.CreateBuilder(args);

// ?
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

builder.Services.AddDbContext<ZuHauseContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("zuhause"))); // 或根據您實際的資料庫提供者使用 UseSqlite, UsePostgreSQL 等


var app = builder.Build();

// 銝凋�撅方身摰�
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

// ? �身擐�頝舐嚗��� FurnitureController �� FurnitureHomePage
app.MapControllerRoute(
    name: "default",
 
    pattern: "{controller=Furniture}/{action=FurnitureHomePage}/{id?}");

    //pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{controller=Tenant}/{action=Announcement}/{id?}");



app.Run();
