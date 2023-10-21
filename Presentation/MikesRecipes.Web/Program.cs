using MikesRecipes.Services.Implementations.Extensions;
using MikesRecipes.DAL.Extensions;
using MikesRecipes.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddAppServices();
builder.Services.AddDAL(builder.Configuration);
builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

var scope = app.Services.CreateScope();
var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
await dbInitializer.InitializeAsync();
if (app.Environment.IsDevelopment())
{
	var dataSeeder = scope.ServiceProvider.GetRequiredService<IDataSeeder>();
	await dataSeeder.SeedDataAsync();
}
scope.Dispose();

app.Run();
