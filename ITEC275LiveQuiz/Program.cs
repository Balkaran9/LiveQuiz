using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Seed;
using ITEC275LiveQuiz.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args;

// Configure Railway port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddRazorPages();

// Database configuration with Railway PostgreSQL support
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // Railway PostgreSQL connection
        // Railway format: postgresql://user:password@host:port/database
        var uri = new Uri(databaseUrl);
        var npgsqlConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
        options.UseNpgsql(npgsqlConnectionString);
    }
    else
    {
        // Local SQL Server
        options.UseSqlServer(connectionString);
    }
    
    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
}, ServiceLifetime.Scoped);

builder.Services.AddScoped<JoinCodeService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<CacheService>();
builder.Services.AddMemoryCache();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(4);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = builder.Environment.IsProduction() 
        ? CookieSecurePolicy.Always 
        : CookieSecurePolicy.SameAsRequest;
});

var app = builder.Build();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    try
    {
        if (Environment.GetEnvironmentVariable("DATABASE_URL") != null)
        {
            // Railway - use migrations or ensure created
            await db.Database.EnsureCreatedAsync();
        }
        else
        {
            // Local development
            await db.Database.EnsureCreatedAsync();
        }
        await SeedData.InitializeAsync(db);
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while initializing the database.");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Railway needs HTTP, not HTTPS redirect
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.UseStaticFiles();
app.MapRazorPages();

app.Run();
