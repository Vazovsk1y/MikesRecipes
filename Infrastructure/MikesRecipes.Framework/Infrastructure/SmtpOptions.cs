using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Framework.Infrastructure;

public class SmtpOptions
{
    public const string SectionName = nameof(EmailSenderOptions.Smtp);

    [Required]
    public required string SmtpServer { get; init; }

    public required bool UseSsl { get; init; }

    [Required]
    public required string Password { get; init; }

    [Required]
    public required string Username { get; init; }

    [Required]
    [AllowedValues(25, 2525, 587, 465)]
    public required int Port { get; init; }
}