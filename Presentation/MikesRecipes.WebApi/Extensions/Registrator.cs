using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MikesRecipes.WebApi.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MikesRecipes.WebApi.Extensions;

public static class Registrator
{
    private static readonly string Description =
        $"JWT Authorization header using the {JwtBearerDefaults.AuthenticationScheme} scheme. \r\n\r\n Enter '{JwtBearerDefaults.AuthenticationScheme}' [space] [your token value].";

    public static IServiceCollection AddSwaggerWithJwtAndVersioning(this IServiceCollection collection)
    {
        collection.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        return collection.AddSwaggerGen(swagger =>
        {
            swagger.OperationFilter<SwaggerDefaultValuesFilter>();

            swagger.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = Description
            });

            swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = JwtBearerDefaults.AuthenticationScheme
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });
    }
}
