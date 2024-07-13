using MikesRecipes.Auth.Implementation.Extensions;
using MikesRecipes.Framework.Extensions;
using MikesRecipes.Application.Implementation.Extensions;
using MikesRecipes.DAL.PostgreSQL.Extensions;
using MikesRecipes.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplicationLayer(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddFramework(builder.Configuration);
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddWebApi();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

if (app.Environment.IsDevelopment())
{
    // TODO: How to migrate database in Production environment?
    app.MigrateDatabase();
}

app.Run();
