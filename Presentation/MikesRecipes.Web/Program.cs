using MikesRecipes.DAL;
using MikesRecipes.Web.Extensions;
using MikesRecipes.Services.Implementations;

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

if (app.Environment.IsDevelopment())
{
	app.MigrateDatabase();
	app.SeedDatabase();
}

app.Run();
