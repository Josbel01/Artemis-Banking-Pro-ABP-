using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Settings;
using ABP.Infrastructure.Identity.Contexts;
using ABP.Infrastructure.Identity.Entities;
using ABP.Infrastructure.Identity.Seeds;
using ABP.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace ABP.Infrastructure.Identity
{
    public static class ServicesRegistration
    {
        public static void AddIdentityLayerIocForWebApp(this IServiceCollection services, IConfiguration config)
        {
            GeneralConfiguration(services, config);

            #region Identity

            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = false;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = true;

                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.AllowedForNewUsers = true;

                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
            });

            services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole>()
                .AddSignInManager()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(12);
            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            }).AddCookie(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(180);
                opt.SlidingExpiration = true;
                opt.LoginPath = "/Account/Index";
                opt.AccessDeniedPath = "/Account/AccessDenied";
            });
            #endregion

            #region Services WebApp
            services.AddScoped<IAccountServiceWebApp, AccountServiceWebApp>();
            services.AddScoped<IBaseAccountService, AccountServiceWebApp>();
            #endregion
        }

        public static void AddIdentityLayerIocForWebApi(this IServiceCollection services, IConfiguration config)
        {
            GeneralConfiguration(services, config);

            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

            #region Identity

            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = false;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireLowercase = false;
                opt.Password.RequireUppercase = true;

                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                opt.Lockout.MaxFailedAccessAttempts = 5;
                opt.Lockout.AllowedForNewUsers = true;

                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
            });

            services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(12);
            });

            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = false;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidIssuer = config["JWTSettings:Issuer"],
                    ValidAudience = config["JWTSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["JWTSettings:SecurityKey"] ?? ""))
                };
                opt.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = c =>
                    {
                        c.NoResult();
                        if (!c.Response.HasStarted)
                        {
                            c.Response.StatusCode = 500;
                            c.Response.ContentType = "text/plain";
                            return c.Response.WriteAsync(c.Exception.ToString());
                        }
                        return Task.CompletedTask;
                    },
                    OnChallenge = c =>
                    {
                        c.HandleResponse();
                        if (!c.Response.HasStarted)
                        {
                            c.Response.StatusCode = 401;
                            c.Response.ContentType = "application/json";
                            var result = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                HasError = true,
                                Error = "No está autorizado para acceder a este recurso."
                            });
                            return c.Response.WriteAsync(result);
                        }
                        return Task.CompletedTask;
                    },
                    OnForbidden = c =>
                    {
                        if (!c.Response.HasStarted)
                        {
                            c.Response.StatusCode = 403;
                            c.Response.ContentType = "application/json";
                            var result = System.Text.Json.JsonSerializer.Serialize(new
                            {
                                HasError = true,
                                Error = "No tiene permisos para acceder a este recurso."
                            });
                            return c.Response.WriteAsync(result);
                        }
                        return Task.CompletedTask;
                    }
                };
            });
            #endregion

            #region Services WebApi
            services.AddScoped<IAccountServiceWebApi, AccountServiceWebApi>();
            services.AddScoped<IBaseAccountService, AccountServiceWebApi>();
            #endregion
        }

        public static async Task RunIdentitySeedAsync(this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var services = scope.ServiceProvider;

            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

            await DefaultUserRoles.SeedAsync(roleManager);
            await DefaultAdminUser.SeedAsync(userManager);
            await DefaultCashierUser.SeedAsync(userManager);
            await DefaultClientUser.SeedAsync(userManager);
            await DefaultCommerceUser.SeedAsync(userManager);
        }

        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            #region Contexts
            if (config.GetValue<bool>("UseInMemoryDatabase"))
            {
                services.AddDbContext<IdentityContext>(opt =>
                opt.UseInMemoryDatabase("ArtemisBankingAppDb"));
            }
            else
            {
                var connectionString = config.GetConnectionString("IdentityConnection");
                services.AddDbContext<IdentityContext>(

                    (serviceProvider, opt) =>
                    {
                        opt.UseSqlServer(connectionString,
                        m => {
                            m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName);
                            m.EnableRetryOnFailure();
                        });
                    },
                    contextLifetime: ServiceLifetime.Scoped,
                    optionsLifetime: ServiceLifetime.Scoped
                    );
            }
            #endregion
        }
    }
}
