using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spark_SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class highlight : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHighlighted",
                table: "Posts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHighlighted",
                table: "Posts");
        }
    }
}
