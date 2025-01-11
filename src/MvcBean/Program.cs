using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using MvcBean.Areas.Identity.Services;
using MvcBean.Data;
using MvcBean.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Configure Logging
builder.Logging.ClearProviders(); // Remove default logging providers
builder.Logging.AddConsole(); // Add console logging
builder.Logging.SetMinimumLevel(LogLevel.Information); // Log Information and above

// Register the DbContext with SQL Server connection
builder.Services.AddDbContext<MvcBeanContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MvcBeanContext") ??
    throw new InvalidOperationException("Connection string 'MvcBeanContext' not found.")));

// Add Identity services with roles
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = true;
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<MvcBeanContext>()
.AddDefaultTokenProviders(); // Ensure default token providers are registered

// Register custom password hasher
builder.Services.AddScoped<IPasswordHasher<IdentityUser>, Argon2PasswordHasher<IdentityUser>>();

// Configure cookie settings for authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login"; // Redirect for unauthenticated users
    options.AccessDeniedPath = "/Home/AccessDenied"; // Redirect for unauthorized users
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60); // Set cookie lifetime
    options.SlidingExpiration = true; // Renew cookie on activity
});

// Register additional services
builder.Services.AddScoped<BeanService>(); // Dependency injection for BeanService
builder.Services.AddScoped<AccountService>(); // Dependency injection for AccountService
builder.Services.AddControllersWithViews(); // Add MVC support
builder.Services.AddRazorPages(); // Add Razor Pages support
builder.Services.AddTransient<IEmailSender, MockEmailSender>(); // Mock email sender for testing

// Configure Identity options
builder.Services.Configure<IdentityOptions>(options =>
{
    options.SignIn.RequireConfirmedEmail = true; // Ensure email confirmation
});

// Build the application
var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error"); // Error handler for production
    app.UseHsts(); // Enforce HTTPS
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// Configure route mappings
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

// Seed roles and users during app startup
await SeedRolesAndUsersAsync(app);

// Temporary test email
var emailSender = app.Services.GetRequiredService<IEmailSender>();
await emailSender.SendEmailAsync("test@example.com", "Test Subject", "This is a test email.");

app.Run();

// Seed roles and users into the database
static async Task SeedRolesAndUsersAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var passwordHasher = new Argon2PasswordHasher<IdentityUser>(); // Use the Argon2 hasher directly

    // Seed roles
    string[] roles = { "Admin", "User" };
    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    // Seed Admin user
    var adminEmail = "admin@example.com";
    var adminPassword = "Admin@123";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        // Hash password with Argon2
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminPassword);

        await userManager.CreateAsync(adminUser); // Create user with pre-hashed password
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Seed Normal User
    var userEmail = "user@example.com";
    var userPassword = "User@123";
    var normalUser = await userManager.FindByEmailAsync(userEmail);
    if (normalUser == null)
    {
        normalUser = new IdentityUser
        {
            UserName = userEmail,
            Email = userEmail,
            EmailConfirmed = true
        };

        // Hash password with Argon2
        normalUser.PasswordHash = passwordHasher.HashPassword(normalUser, userPassword);

        await userManager.CreateAsync(normalUser); // Create user with pre-hashed password
        await userManager.AddToRoleAsync(normalUser, "User");
    }
}
