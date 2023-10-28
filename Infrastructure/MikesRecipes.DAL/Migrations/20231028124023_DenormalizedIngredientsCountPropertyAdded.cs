using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MikesRecipes.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DenormalizedIngredientsCountPropertyAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IngredientsCount",
                table: "Recipes",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IngredientsCount",
                table: "Recipes");
        }
    }
}
