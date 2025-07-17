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

// === 新增 Session 服務配置 ===
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // 設定 Session 超時時間，例如 30 分鐘
    options.Cookie.HttpOnly = true; // 設定 Session Cookie 只能透過 HTTP 訪問，增加安全性
    options.Cookie.IsEssential = true; // 設定 Session Cookie 為必要的，以便 Session 能夠工作
});
// =============================




var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession(); // 啟用 Session 中間件
app.UseAuthentication();
app.UseAuthorization();

// ?  FurnitureController �� FurnitureHomePage
app.MapControllerRoute(
    name: "default",
 
    pattern: "{controller=Furniture}/{action=FurnitureHomePage}/{id?}");

    //pattern: "{controller=Home}/{action=Index}/{id?}");
    //pattern: "{controller=Tenant}/{action=Announcement}/{id?}");



app.Run();
