using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Settings;
using ABP.Infrastructure.Shared.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ABP.Infrastructure.Shared
{
    public static class ServicesRegistration
    {
        public static void AddSharedLayerIoc(this IServiceCollection services, IConfiguration config)
        {
            #region Configurations
            services.Configure<EmailSettings>(config.GetSection("EmailSettings"));
            #endregion

            #region Services IOC
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IImageStorageService, ImageStorageService>();
            #endregion
        }

    }
}
