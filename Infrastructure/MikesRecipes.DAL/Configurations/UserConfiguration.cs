using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.DAL.Configurations;

internal class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasMany(e => e.Roles)
            .WithOne(e => e.User)
            .HasForeignKey(ur => ur.UserId);

        builder
            .HasIndex(e => e.NormalizedEmail)
            .IsUnique();
    }
}