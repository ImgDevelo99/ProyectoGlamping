using EcoGlam.DataBase;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {

        option.LoginPath = "/Inicio/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(50);
        option.AccessDeniedPath = "/Inicio/Error";

    });

builder.Services.AddDbContext<EcoGlamContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("CadenaSql")));

builder.Services.AddControllers().AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

var permisos = await LoadPermissionsAsync(builder.Services);

builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in permisos)
    {
        options.AddPolicy($"{permiso.Replace(" ", "")}Permiso", policy =>
            policy.RequireClaim("Permiso", permiso));
    }
});

QuestPDF.Settings.License = LicenseType.Community;

var app = builder.Build();

static async Task<List<string>> LoadPermissionsAsync(IServiceCollection services)
{
    using (var scope = services.BuildServiceProvider().CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<EcoGlamContext>();
        return await dbContext.Permisos.Select(p => p.NomPermiso).ToListAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Dashboard/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
