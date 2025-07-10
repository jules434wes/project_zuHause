using zuHause.Data; // ← 加這行
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);
// 加入資料庫連線字串
builder.Services.AddDbContext<ZuhauseDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ZuhauseDb"))
);
// Add services to the container.
builder.Services.AddControllersWithViews();

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
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.Run();
