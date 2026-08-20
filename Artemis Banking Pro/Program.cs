using ABP.Core.Application;
using ABP.Infrastructure.Identity;
using ABP.Infrastructure.Persistence;
using ABP.Infrastructure.Shared;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext().CreateLogger();

builder.Host.UseSerilog(Log.Logger);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddSession(opt =>
{
    opt.IdleTimeout = TimeSpan.FromMinutes(60);
    opt.Cookie.HttpOnly = true;    
});

builder.Services.AddPersistenceLayerIoc(builder.Configuration);
builder.Services.AddApplicationLayerIoc();
builder.Services.AddIdentityLayerIocForWebApp(builder.Configuration);
builder.Services.AddSharedLayerIoc(builder.Configuration);
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

var app = builder.Build();

await app.Services.RunIdentitySeedAsync();

// Ensure all seeded clients have a main saving account
await EnsureSeededClientAccountsAsync(app.Services);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days; you may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// Middleware: Redirect authenticated users away from login, and redirect / to home
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value?.ToLower() ?? "";
    var user = context.User;

    // Redirect authenticated users away from login page
    if (user.Identity?.IsAuthenticated == true && (path == "/" || path == "/account" || path == "/account/index"))
    {
        if (user.IsInRole("Admin"))
        {
            context.Response.Redirect("/Admin");
            return;
        }
        if (user.IsInRole("Cashier"))
        {
            context.Response.Redirect("/Cashier/Home");
            return;
        }
        if (user.IsInRole("Client"))
        {
            context.Response.Redirect("/Client");
            return;
        }
    }

    // Role-based URL access restriction
    if (user.Identity?.IsAuthenticated == true)
    {
        bool isForbidden = false;
        if (user.IsInRole("Admin") && (path.StartsWith("/cashier") || path.StartsWith("/client")))
            isForbidden = true;
        if (user.IsInRole("Cashier") && (path.StartsWith("/admin") || path.StartsWith("/client") || path.StartsWith("/user") || path.StartsWith("/creditcard") || path.StartsWith("/loan") || path.StartsWith("/savingaccount") || path.StartsWith("/transaction") || path.StartsWith("/loaninstallment")))
            isForbidden = true;
        if (user.IsInRole("Client") && (path.StartsWith("/admin") || path.StartsWith("/cashier") || path.StartsWith("/user") || path.StartsWith("/creditcard") || path.StartsWith("/loan") || path.StartsWith("/savingaccount") || path.StartsWith("/loaninstallment")))
            isForbidden = true;

        if (isForbidden)
        {
            context.Response.Redirect("/Account/AccessDenied");
            return;
        }
    }

    await next();
});

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

await app.RunAsync();

// Helper method to ensure all clients have a main saving account
static async Task EnsureSeededClientAccountsAsync(IServiceProvider services)
{
    using var scope = services.CreateScope();
    var scopedServices = scope.ServiceProvider;

    var userManager = scopedServices.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<ABP.Infrastructure.Identity.Entities.AppUser>>();
    var context = scopedServices.GetRequiredService<ABP.Infrastructure.Persistence.Contexts.ArtemisBankingAppContext>();

    // Find all users with Client role who don't have a main account
    var allClients = await userManager.GetUsersInRoleAsync("Client");
    foreach (var user in allClients)
    {
        var hasMainAccount = await context.SavingAccounts
            .AnyAsync(a => a.ClientId == user.Id && a.AccountType == ABP.Core.Domain.Common.Enums.SavingAccountType.Main);

        if (!hasMainAccount)
        {
            var rnd = new Random();
            string accountNumber = rnd.Next(100000000, 999999999).ToString();
            var mainAccount = new ABP.Core.Domain.Entities.SavingAccount
            {
                ClientId = user.Id,
                AccountNumber = accountNumber,
                Balance = 0,
                AccountType = ABP.Core.Domain.Common.Enums.SavingAccountType.Main,
                Status = ABP.Core.Domain.Common.Enums.SavingAccountStatus.Active
            };
            context.SavingAccounts.Add(mainAccount);
        }
    }
    await context.SaveChangesAsync();
}
