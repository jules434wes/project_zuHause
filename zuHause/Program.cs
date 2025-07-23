using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using zuHause.Data;
using zuHause.Helpers;
using zuHause.Models;
using zuHause.Services;

var builder = WebApplication.CreateBuilder(args);


var context = new CustomAssemblyLoadContext();

var path = Path.Combine(Directory.GetCurrentDirectory(), "DinkToPdfLib", "libwkhtmltox.dll");
context.LoadUnmanagedLibrary(path);

builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    //options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic,
                UnicodeRanges.CjkUnifiedIdeographs);
    options.JsonSerializerOptions.WriteIndented = true;
});

// 會員
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "MemberCookieAuth";
})
.AddCookie("MemberCookieAuth", options =>
{
    options.LoginPath = "/Member/Login";
    options.AccessDeniedPath = "/Member/AccessDenied";
})
.AddCookie("AdminCookies", options =>// 管理員登入驗證
{
    options.LoginPath = "/Auth/Login"; // 登入路徑
    options.LogoutPath = "/Auth/Logout"; // 登出路徑
    options.ExpireTimeSpan = TimeSpan.FromHours(8); // Cookie 有效時間 8 小時
    options.SlidingExpiration = true; // 滑動期限（有活動就延長）
});



builder.Services.AddDbContext<ZuHauseContext>(
            options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddMemoryCache();

// 註冊 RealDataSeeder
builder.Services.AddScoped<RealDataSeeder>();

// 註冊圖片處理服務
builder.Services.AddScoped<zuHause.Interfaces.IImageProcessor, zuHause.Services.ImageSharpProcessor>();

// 註冊統一圖片管理系統服務
builder.Services.AddScoped<zuHause.Interfaces.IEntityExistenceChecker, zuHause.Services.EntityExistenceChecker>();
builder.Services.AddScoped<zuHause.Interfaces.IDisplayOrderManager, zuHause.Services.DisplayOrderManager>();
builder.Services.AddScoped<zuHause.Interfaces.IImageQueryService, zuHause.Services.ImageQueryService>();
builder.Services.AddScoped<zuHause.Interfaces.IImageUploadService, zuHause.Services.ImageUploadService>();
builder.Services.AddScoped<zuHause.Services.Interfaces.IImageValidationService, zuHause.Services.ImageValidationService>();

// 註冊房源圖片 Facade 服務
builder.Services.AddScoped<zuHause.Interfaces.IPropertyImageService, zuHause.Services.PropertyImageService>();

// === 註冊管理員權限檢查服務 ===
// 用於 AdminController 的後端權限驗證
// 配合 RequireAdminPermissionAttribute 使用，防止直接 URL 存取無權限功能
builder.Services.AddScoped<zuHause.Services.Interfaces.IPermissionService, zuHause.Services.PermissionService>();

// === 註冊訊息模板服務 ===
// 用於參數化模板的解析和替換
builder.Services.AddScoped<zuHause.Services.MessageTemplateService>();

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


builder.Services.AddScoped<IPasswordHasher<Member>, PasswordHasher<Member>>();
builder.Services.AddScoped<MemberService>();

var app = builder.Build();

// 在開發環境確保基礎資料存在
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<RealDataSeeder>();
        try
        {
            await seeder.EnsureDataAsync();
        }
        catch (Exception ex)
        {
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "確保基礎資料失敗");
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
app.UseSession(); // 啟用 Session 中間件
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.MapControllerRoute(
    name: "default",

//家具首頁路由
//pattern: "{controller=Furniture}/{action=FurnitureHomePage}/{id?}");

//pattern: "{controller=Home}/{action=Index}/{id?}");
//pattern: "{controller=Dashboard}/{action=Index}/{id?}");

//租屋首頁路由
pattern: "{controller=Tenant}/{action=FrontPage}/{id?}");




app.Run();
