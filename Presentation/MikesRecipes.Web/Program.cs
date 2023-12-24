using MikesRecipes.Services.Implementations.Extensions;
using MikesRecipes.DAL;
using MikesRecipes.Web.Extensions;

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

app.MigrateDatabase();
app.SeedDatabase();

app.Run();
