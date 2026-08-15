using Asp.Versioning;

namespace Artemis_Banking_Pro_WebApi.Extensions
{
    public static class AppExtension
    {
        public static void UseSwaggerExtension(this IApplicationBuilder app, IEndpointRouteBuilder routeBuilder)
        {
            app.UseSwagger();
            app.UseSwaggerUI(opt =>
            {
                var versionDescription = routeBuilder.DescribeApiVersions();
                if(versionDescription != null && versionDescription.Any())
                {
                    foreach(var version in versionDescription)
                    {
                        var url = $"/swagger/{version.GroupName}/swagger.json";
                        var name = $"Artemis Banking API - {version.GroupName.ToUpperInvariant()}";
                        opt.SwaggerEndpoint(url, name);
                    }
                }
            });
        }
    }
}
