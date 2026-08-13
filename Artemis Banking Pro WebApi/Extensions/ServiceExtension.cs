using Asp.Versioning;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArtemisBankingApi.Extensions
{
    public static class ServiceExtension
    {
        public static void AddSwaggerExtension(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                List<string> xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                xmlFiles.ForEach(xmlFile => options.IncludeXmlComments(xmlFile));

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1.0",
                    Title = "Artemis Banking Pro API",
                    Description = "API for Artemis Banking Pro",
                    Contact = new OpenApiContact
                    {
                        Name = "Mariely Roa",
                        Email = "corage1920@gmail.com"
                    }
                });

                options.DescribeAllParametersInCamelCase();

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Input your JWT token directly below. Swagger will automatically add the 'Bearer ' prefix."
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "bearer",
                            Name = "Bearer",
                            In = ParameterLocation.Header,
                        },
                        new List<string>()
                    }
                });
            });
        }

        public static void AddAppiVersioningExtension(this IServiceCollection services)
        {
            services.AddApiVersioning(options =>
            {
                 options.DefaultApiVersion = new ApiVersion(1, 0);
                 options.AssumeDefaultVersionWhenUnspecified = true;
                 options.ReportApiVersions = true;

                 options.ApiVersionReader = ApiVersionReader.Combine(
                     new UrlSegmentApiVersionReader(),
                     new HeaderApiVersionReader("X-Api-Version")
                );
            }).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });
        }
    }
}
