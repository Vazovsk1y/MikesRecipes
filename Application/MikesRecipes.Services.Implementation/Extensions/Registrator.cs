using FluentEmail.MailKitSmtp;
using FluentValidation;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace MikesRecipes.Services.Implementation;

public static class Registrator
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IRecipeService, RecipeService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IEmailSender, EmailSender>();

        services.AddOptions<EmailSenderOptions>()
            .BindConfiguration(EmailSenderOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var emailOptions = configuration.GetSection(EmailSenderOptions.SectionName).Get<EmailSenderOptions>()!;

        services.AddFluentEmail(emailOptions.DefaultFromEmail, emailOptions.DefaultFromName)
            .AddRazorRenderer()
            .AddMailKitSender(new SmtpClientOptions
            {
                Server = emailOptions.SmtpOptions.SmtpServer,
                UseSsl = emailOptions.SmtpOptions.UseSsl,
                Port = emailOptions.SmtpOptions.Port,
                RequiresAuthentication = true,
                SocketOptions = SecureSocketOptions.Auto,
                Password = emailOptions.SmtpOptions.Password,
                User = emailOptions.SmtpOptions.Username,
            });

		return services;
    }
}
