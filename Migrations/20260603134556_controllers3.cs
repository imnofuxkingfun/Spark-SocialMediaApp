using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spark_SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class controllers3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "Posts",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Media",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Spark_Text",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Text",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Posts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Media",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Spark_Text",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Text",
                table: "Posts");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Posts");
        }
    }
}
