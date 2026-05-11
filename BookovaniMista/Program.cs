using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;
using Business.BookovaniMista.Interfaces;
using Serilog;
using WebMarkupMin.AspNetCore6;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// LOGGING CONFIGURATION (Serilog)
// ============================================================

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console()
    .WriteTo.File(
        path: Path.Combine("logs", "app-.txt"),
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("🚀 Aplikace se spouští...");

    // ============================================================
    // RESPONSE COMPRESSION CONFIGURATION
    // ============================================================
    builder.Services.AddResponseCompression(options =>
    {
        options.Providers.Add<GzipCompressionProvider>();
        options.Providers.Add<BrotliCompressionProvider>();
        options.MimeTypes = new[]
        {
            "text/html",
            "text/plain",
            "text/css",
            "text/javascript",
            "application/json",
            "application/javascript",
            "image/svg+xml"
        };
        options.EnableForHttps = true;
    });

    // Add services to the container.
    builder.Services.AddControllersWithViews().AddRazorOptions(options => {
            options.ViewLocationFormats.Add("/Views/PartialViews/{0}.cshtml");
        });

builder.Services.AddDbContext<BookovaniMistaDbContext>(options =>
    options.UseInMemoryDatabase("BookovaniMistaDb"));

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddMemoryCache();
builder.Services.AddRazorPages();
builder.Services.AddScoped<IMapaBusiness, Business.BookovaniMista.MapaBusiness>();
builder.Services.AddScoped<IRezervaceBusiness, Business.BookovaniMista.RezervaceBusiness>();
builder.Services.AddScoped<ICommonBusiness, Business.BookovaniMista.CommonBusiness>();

var app = builder.Build();

// ============================================================
// RESPONSE COMPRESSION MIDDLEWARE
// ============================================================
app.UseResponseCompression();
Log.Information("📦 Response Compression (GZIP/Brotli) aktivován");

// ============================================================
// GLOBAL EXCEPTION HANDLING
// ============================================================
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    Log.Information("📊 Development režim aktivován - Detailní error stránky");
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseStatusCodePagesWithReExecute("/Error/{0}");
    Log.Information("🔒 Production režim aktivován - Bezpečné error handling");

    // ✅ HTML Minification v production
    app.UseWebMarkupMin();
    Log.Information("🔍 HTML Minification (WebMarkupMin) aktivován");
}

// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
app.UseHsts();

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
        .WithStaticAssets();

    // Spuštění seedování v rámci scope před spuštěním aplikace
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<BookovaniMistaDbContext>();
        await SeedData.InitializeAsync(db);
    }

    Log.Information("✅ Aplikace úspěšně spuštěna!");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "❌ Aplikace se selhala při startu");
}
finally
{
    Log.Information("🛑 Aplikace se vypíná...");
    await Log.CloseAndFlushAsync();
}
