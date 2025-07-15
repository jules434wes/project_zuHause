using Microsoft.EntityFrameworkCore;
using zuHause.Models;

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

// Add services to the container.
builder.Services.AddControllersWithViews();



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
app.UseAuthentication();
app.UseAuthorization();

// 亂碼請修正 FurnitureController �� FurnitureHomePage
app.MapControllerRoute(
    name: "default",

    //    pattern: "{controller=Furniture}/{action=FurnitureHomePage}/{id?}");

    //pattern: "{controller=Home}/{action=Index}/{id?}");
    pattern: "{controller=Tenant}/{action=Announcement}/{id?}");



app.Run();
