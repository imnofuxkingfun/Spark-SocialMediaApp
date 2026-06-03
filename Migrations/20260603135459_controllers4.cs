using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spark_SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class controllers4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Posts",
                newName: "Spark_Media");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Spark_Media",
                table: "Posts",
                newName: "Description");
        }
    }
}
