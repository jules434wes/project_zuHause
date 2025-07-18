using Microsoft.EntityFrameworkCore;
using zuHause.Models;
using zuHause.Data;
using Microsoft.AspNetCore.Identity;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using zuHause.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = null;
    options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic,
                UnicodeRanges.CjkUnifiedIdeographs);
    options.JsonSerializerOptions.WriteIndented = true;
});

// 會員
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

builder.Services.AddScoped<IPasswordHasher<Member>, PasswordHasher<Member>>();
builder.Services.AddScoped<MemberService>();



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

app.MapControllerRoute(
    name: "default",

    //pattern: "{controller=Furniture}/{action=FurnitureHomePage}/{id?}");
    //pattern: "{controller=Home}/{action=Index}/{id?}");
    //pattern: "{controller=Admin}/{action=Index}/{id?}");
    pattern: "{controller=Tenant}/{action=Announcement}/{id?}");



app.Run();
