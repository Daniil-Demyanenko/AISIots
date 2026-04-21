using AISIots.DAL;
using AISIots.Interfaces;
using AISIots.Services;
using AISIots.Middleware;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add SQLite
var basePath = AppDomain.CurrentDomain.BaseDirectory;
var connectionString = builder.Configuration.GetConnectionString("SQLiteConnectionString")?.Replace("[DataDirectory]", basePath);
if (!string.IsNullOrEmpty(connectionString))
{
    connectionString += ";Cache=Shared";
}

builder.Services.AddDbContext<SqliteContext>(options =>
    options.UseSqlite(connectionString));

// Регистрация сервисов и репозитория
builder.Services.AddScoped<IDbRepository, DbRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPlanService, PlanService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IParserFactory, ParserFactory>();
builder.Services.AddScoped<IActionLogService, ActionLogService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFileProcessingService, FileProcessingService>();
builder.Services.AddScoped<ITemplateGeneratorService, TemplateGeneratorService>();

// Add Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = new PathString("/Main/Login");
        options.LogoutPath = new PathString("/Main/Logout");
    });


builder.Services.AddControllersWithViews();

builder.Services.ConfigureApplicationCookie(options => { options.LoginPath = "/Main/Login"; });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Main/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseUserExistenceCheck();
app.UseAuthorization();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Main}/{action=Index}/");

app.Run();