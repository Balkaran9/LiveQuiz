using ITEC275LiveQuiz.Data;
using ITEC275LiveQuiz.Seed;
using ITEC275LiveQuiz.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database configuration - Use SQLite for Railway/Production, SQL Server for local dev
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Check if we're on Railway or other cloud platform (no SQL Server available)
if (string.IsNullOrEmpty(connectionString) || builder.Environment.IsProduction())
{
    // Use SQLite for cloud deployment
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=livequiz.db"));
    builder.Logging.LogInformation("Using SQLite database for production/cloud deployment");
}
else
{
    // Use SQL Server LocalDB for local development
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));
    builder.Logging.LogInformation("Using SQL Server LocalDB for local development");
}

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None;
});

// Caching
builder.Services.AddMemoryCache();

// Services
builder.Services.AddScoped<JoinCodeService>();
builder.Services.AddScoped<LeaderboardService>();
builder.Services.AddScoped<CacheService>();

// SignalR for real-time updates
builder.Services.AddSignalR();

// Controllers for API
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// CORS for API access
builder.Services.AddCors(options =>
{
    options.AddPolicy("ApiPolicy", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Enhanced Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Database initialization
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        logger.LogInformation("Initializing database...");
        await db.Database.EnsureCreatedAsync();
        logger.LogInformation("Database created successfully");
        
        await SeedData.InitializeAsync(db);
        logger.LogInformation("Seed data loaded successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during database initialization");
        throw;
    }
}

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Remove HTTPS redirection for Railway
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

// Enable CORS
app.UseCors("ApiPolicy");

app.UseSession();
app.UseAuthorization();

// Map endpoints
app.MapRazorPages();
app.MapControllers();
app.MapHub<ITEC275LiveQuiz.Hubs.GameHub>("/gameHub");

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new 
{ 
    status = "healthy", 
    timestamp = DateTime.UtcNow,
    version = "2.0.0"
}));

// Railway sets PORT environment variable
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Urls.Clear();
app.Urls.Add($"http://0.0.0.0:{port}");

app.Logger.LogInformation("?? LiveQuiz application started successfully!");
app.Logger.LogInformation("?? SignalR enabled at /gameHub");
app.Logger.LogInformation("?? API available at /api/*");
app.Logger.LogInformation("?? Listening on port {Port}", port);

app.Run();
