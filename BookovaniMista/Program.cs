using Microsoft.AspNetCore.Authentication.Negotiate;
using Microsoft.EntityFrameworkCore;
using Entities.BookovaniMista;
using Business.BookovaniMista.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<BookovaniMistaDbContext>(options =>
    options.UseInMemoryDatabase("BookovaniMistaDb")); //UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))); // UseInMemoryDatabase for testing purposes

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
   .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    options.FallbackPolicy = options.DefaultPolicy;
});
builder.Services.AddRazorPages();
builder.Services.AddScoped<IMapaBusiness, Business.BookovaniMista.MapaBusiness>();
builder.Services.AddScoped<IRezervaceBusiness, Business.BookovaniMista.RezervaceBusiness>();
builder.Services.AddScoped<ICommonBusiness, Business.BookovaniMista.CommonBusiness>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// Spuštìní seedování v rámci scope pøed spuštìním aplikace
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BookovaniMistaDbContext>();
    await SeedData.InitializeAsync(db);
}

app.Run();
