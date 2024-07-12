using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using MikesRecipes.WebApi.Infrastructure;
using MikesRecipes.WebApi.Infrastructure.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MikesRecipes.WebApi.Extensions;

public static class Registrator
{
    private static readonly string SwaggerAuthDescription =
        $"JWT Authorization header using the {JwtBearerDefaults.AuthenticationScheme} scheme. \r\n\r\n Enter '{JwtBearerDefaults.AuthenticationScheme}' [space] [your token value].";

    public static void AddWebApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
        services.AddEndpointsApiExplorer();

        services.AddExceptionHandler<ExceptionsHandler>();
        services.AddProblemDetails();
        services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

        services.AddApiVersioning(e =>
        {
            e.DefaultApiVersion = new ApiVersion(Constants.WebApi.Version);
            e.AssumeDefaultVersionWhenUnspecified = true;
            e.ReportApiVersions = true;
            e.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(e =>
        {
            e.GroupNameFormat = "'v'VVV";
            e.SubstituteApiVersionInUrl = true;
        });

        services.AddCors();
        services.AddSwaggerWithJwtAndVersioning();
    }

    private static void AddSwaggerWithJwtAndVersioning(this IServiceCollection collection)
    {
        collection.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();

        collection.AddSwaggerGen(swagger =>
        {
            swagger.OperationFilter<SwaggerDefaultValuesFilter>();

            swagger.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = JwtBearerDefaults.AuthenticationScheme,
                BearerFormat = "Jwt",
                In = ParameterLocation.Header,
                Description = SwaggerAuthDescription
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
