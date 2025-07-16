using Microsoft.EntityFrameworkCore;

using zuHause.Models; // ½T«O³o¬O ZuHauseContext ¥¿½Tªº©R¦WªÅ¶¡


var builder = WebApplication.CreateBuilder(args);

// ?ƒå“¡
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
    options.UseSqlServer(builder.Configuration.GetConnectionString("zuhause"))); // ©Î®Ú¾Ú±z¹ê»Úªº¸ê®Æ®w´£¨ÑªÌ¨Ï¥Î UseSqlite, UsePostgreSQL µ¥


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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
