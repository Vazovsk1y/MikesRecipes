using MikesRecipes.DAL;
using MikesRecipes.Web.Extensions;
using MikesRecipes.Services.Implementations;
using MikesRecipes.Domain.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddApplicationLayer();
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

builder.Services
	.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
	.AddEntityFrameworkStores<MikesRecipesDbContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

if (app.Environment.IsDevelopment())
{
	app.MigrateDatabase();
	app.SeedDatabase();
}

app.Run();
