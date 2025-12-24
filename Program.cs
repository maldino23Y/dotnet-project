using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using SuiviEntrainementSportif.Data;
using SuiviEntrainementSportif.Models;
using SuiviEntrainementSportif.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Configure EF Core (SQL Server) and Identity
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? "Server=(localdb)\\MSSQLLocalDB;Database=SuiviEntrainementDB;Trusted_Connection=True;MultipleActiveResultSets=true";
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// register IEmailSender, etc.
builder.Services.AddSingleton<IEmailSender, SuiviEntrainementSportif.Services.DummyEmailSender>();
// register AI fitness service
builder.Services.AddScoped<SuiviEntrainementSportif.Services.IAiFitnessService, SuiviEntrainementSportif.Services.AiFitnessService>();

var app = builder.Build();

// Ensure database is created and migrations are applied, then seed admin role/user
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

        try
        {
            var db = services.GetRequiredService<ApplicationDbContext>();
            // Log which connection is being used for diagnostics
            var conn = db.Database.GetDbConnection();
            try
            {
                logger.LogInformation("Using database: {Database}; DataSource: {DataSource}", conn.Database, conn.DataSource);
            }
            catch { }

            var pending = await db.Database.GetPendingMigrationsAsync();
            if (pending != null && pending.Any())
            {
                // Apply any pending migrations. In some dev environments the Identity tables may exist
                // but lack newer columns; ensure migrations are applied so EF queries match the schema.
                try
                {
                    await db.Database.MigrateAsync();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Automatic migration failed. You can run 'dotnet ef database update' manually to apply migrations.");
                    throw;
                }
            }
            else
            {
                // No migrations found in the project. Do not call EnsureCreated here to avoid creating schema
                // outside of migrations. Log diagnostic and verify Identity table presence.
                try
                {
                    await conn.OpenAsync();
                    using var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT OBJECT_ID(N'dbo.AspNetUsers')";
                    var obj = await cmd.ExecuteScalarAsync();
                    if (obj == null || obj == DBNull.Value)
                    {
                        logger.LogWarning("AspNetUsers table was not found. If you expect migrations, run 'dotnet ef migrations add InitialCreate' and 'dotnet ef database update', or create the schema manually.");
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to verify AspNetUsers existence.");
                }
                finally
                {
                    try { await conn.CloseAsync(); } catch { }
                }
            }

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        var adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        var adminEmail = configuration["AdminUser:Email"] ?? "admin@example.com";
        var adminPassword = configuration["AdminUser:Password"] ?? "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                Nom = "Admin"
            };

            var result = await userManager.CreateAsync(admin, adminPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(admin, adminRole);
            }
        }
    }
    catch (Exception ex)
    {
        var logger2 = scope.ServiceProvider.GetService<ILogger<Program>>();
        logger2?.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
