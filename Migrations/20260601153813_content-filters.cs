using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spark_SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class contentfilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GoreContentFilterEnabled",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "NudityContentFilterEnabled",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "SensitiveContentFilterEnabled",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "SuggestiveContentFilterEnabled",
                table: "UserSettings");

            migrationBuilder.AddColumn<string>(
                name: "ContentFilters",
                table: "UserSettings",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContentFilters",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Privacy",
                table: "Posts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentFilters",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "ContentFilters",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Privacy",
                table: "Posts");

            migrationBuilder.AddColumn<bool>(
                name: "GoreContentFilterEnabled",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NudityContentFilterEnabled",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SensitiveContentFilterEnabled",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "SuggestiveContentFilterEnabled",
                table: "UserSettings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
