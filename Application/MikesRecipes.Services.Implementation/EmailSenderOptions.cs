using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

namespace MikesRecipes.Services.Implementation;

public class EmailSenderOptions
{
    public const string SectionName = "Mailing";

    [Required]
    [EmailAddress]
    public required string DefaultFromEmail { get; init; }

    public string? DefaultFromName { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required SmtpOptions SmtpOptions { get; init; }
}

public class SmtpOptions
{
    public const string SectionName = nameof(EmailSenderOptions.SmtpOptions);

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