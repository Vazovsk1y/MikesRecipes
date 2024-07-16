using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;

namespace MikesRecipes.Framework.Infrastructure;

public class EmailSenderOptions
{
    public const string SectionName = "EmailSender";

    [Required]
    [EmailAddress]
    public required string DefaultFromEmail { get; init; }

    public string? DefaultFromName { get; init; }

    [Required]
    [ValidateObjectMembers]
    public required SmtpOptions Smtp { get; init; }
}