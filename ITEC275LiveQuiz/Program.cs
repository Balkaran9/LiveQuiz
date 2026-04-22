using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Seed;
using ITEC275LiveQuiz.Services;
using Microsoft.EntityFrameworkCore;

// Force Production environment when DATABASE_URL exists (Railway deployment)
var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
if (!string.IsNullOrEmpty(databaseUrl))
{
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production");
}

var builder = WebApplication.CreateBuilder(args);

// Configure Railway port
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddRazorPages();

// Database configuration with Railway PostgreSQL support
builder.Services.AddDbContext<AppDbContext>(options =>
{
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        // Railway PostgreSQL connection
        // Railway format: postgresql://user:password@host:port/database
        var uri = new Uri(databaseUrl);
        var npgsqlConnectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={uri.UserInfo.Split(':')[0]};Password={uri.UserInfo.Split(':')[1]};SSL Mode=Require;Trust Server Certificate=true";
        options.UseNpgsql(npgsqlConnectionString);
        Console.WriteLine($"Using PostgreSQL: {uri.Host}");
    }
    else
    {
        // Local SQL Server - only use if DATABASE_URL is not set
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(connectionString))
        {
            options.UseSqlServer(connectionString);
            Console.WriteLine("Using SQL Server LocalDB");
        }
        else
        {
            throw new InvalidOperationException("No database connection configured. DATABASE_URL or DefaultConnection must be set.");
        }
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
    options.Cookie.Name = ".LiveQuiz.Session";
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

var app = builder.Build();

// Configure forwarded headers for Railway proxy
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | 
                      Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Initializing database...");
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("Database initialized successfully");
        
        await SeedData.InitializeAsync(db);
        logger.LogInformation("Seed data initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while initializing the database.");
        // Don't throw - let the app start even if database init fails
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
