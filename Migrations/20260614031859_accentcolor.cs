using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spark_SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class accentcolor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccentColor",
                table: "UserProfiles",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccentColor",
                table: "UserProfiles");
        }
    }
}
