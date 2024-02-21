using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using MikesRecipes.WebApi.Filters;

namespace MikesRecipes.WebApi.Extensions;

public static class Registrator
{
    private static readonly string Description =
        $"JWT Authorization header using the {JwtBearerDefaults.AuthenticationScheme} scheme. \r\n\r\n Enter '{JwtBearerDefaults.AuthenticationScheme}' [space] [your token value].";

    public static IServiceCollection AddSwaggerWithJwt(this IServiceCollection collection)
    {
        return
        collection.AddSwaggerGen(swagger =>
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
