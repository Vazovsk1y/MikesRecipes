using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MikesRecipes.Domain.Models;

namespace MikesRecipes.DAL.PostgreSQL.Configurations;

internal class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
		builder.HasMany(e => e.Users)
			   .WithOne(e => e.Role)
			   .HasForeignKey(ur => ur.RoleId);
    }
}