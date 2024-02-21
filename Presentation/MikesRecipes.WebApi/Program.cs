using Asp.Versioning;
using Microsoft.Extensions.Options;
using MikesRecipes.DAL.Extensions;
using MikesRecipes.Services.Implementations;
using MikesRecipes.WebApi;
using MikesRecipes.WebApi.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerWithJwt();

builder.Services.AddApplicationLayer(builder.Configuration);
builder.Services.AddDataAccessLayer(builder.Configuration);

builder.Services.AddExceptionHandler<ExceptionsHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddApiVersioning(e =>
{
    e.DefaultApiVersion = new ApiVersion(Constants.ApiVersions.V1Dot0);
    e.AssumeDefaultVersionWhenUnspecified = true;
    e.ReportApiVersions = true;
    e.ApiVersionReader = new UrlSegmentApiVersionReader();
})
.AddApiExplorer(e =>
{
    e.GroupNameFormat = "'v'VVV";
    e.SubstituteApiVersionInUrl = true;
});

builder.Services.AddCors();

// App build
var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var descriptions = app.DescribeApiVersions();

    foreach (var description in descriptions)
    {
        var url = $"/swagger/{description.GroupName}/swagger.json";
        var name = description.GroupName.ToUpperInvariant();
        options.SwaggerEndpoint(url, name);
    }
});

app.UseHttpsRedirection();

app.UseCors(e => e.AllowAnyHeader().AllowAnyOrigin().AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MigrateDatabase();
app.SeedDatabase();

app.Run();
