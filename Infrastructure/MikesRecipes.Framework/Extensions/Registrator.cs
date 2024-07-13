using FluentEmail.MailKitSmtp;
using FluentValidation;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MikesRecipes.Framework.Interfaces;
using System.Reflection;
using MikesRecipes.Framework.Infrastructure;

namespace MikesRecipes.Framework.Extensions;

public static class Registrator
{
    public static void AddFramework(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<IClock, Clock>();
        services.AddScoped<ICurrentUserProvider, CurrentUserProvider>();

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddScoped<IEmailSender, EmailSender>();

        services.AddOptions<EmailSenderOptions>()
            .BindConfiguration(EmailSenderOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // TODO: How to obtain email sender options in Production mode?
        var emailOptions = configuration.GetSection(EmailSenderOptions.SectionName).Get<EmailSenderOptions>()!;

        services.AddFluentEmail(emailOptions.DefaultFromEmail, emailOptions.DefaultFromName)
            .AddRazorRenderer()
            .AddMailKitSender(new SmtpClientOptions
            {
                Server = emailOptions.Smtp.SmtpServer,
                UseSsl = emailOptions.Smtp.UseSsl,
                Port = emailOptions.Smtp.Port,
                RequiresAuthentication = true,
                SocketOptions = SecureSocketOptions.Auto,
                Password = emailOptions.Smtp.Password,
                User = emailOptions.Smtp.Username,
            });
    }
}
