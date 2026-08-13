using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ArtemisBankingApi.Extensions
{
    public static class AppExtension
    {
        public static void UseSwaggerExtension(this IApplicationBuilder app, IEndpointRouteBuilder routeBuilder)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var versionDescriptions = routeBuilder.DescribeApiVersions();

                if (versionDescriptions != null && versionDescriptions.Any())
                {
                    foreach (var apiVersion in versionDescriptions)
                    {
                        var url = $"/swagger/{apiVersion.GroupName}/swagger.json";
                        var name = $"Artemis Banking API {apiVersion.GroupName.ToUpperInvariant()}";
                        options.SwaggerEndpoint(url, name);
                    }
                }
            });
        }
    }
}
