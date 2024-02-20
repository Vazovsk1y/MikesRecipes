using MikesRecipes.DAL;
using MikesRecipes.Services.Implementations;
using MikesRecipes.WebApi;
using MikesRecipes.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationLayer();
builder.Services.AddDataAccessLayer(builder.Configuration);

builder.Services.AddExceptionHandler<ExceptionsHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MigrateDatabase();
app.SeedDatabase();

app.Run();
