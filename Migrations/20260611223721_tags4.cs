using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spark_SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class tags4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Tags_UserId",
                table: "UserTags");

            migrationBuilder.CreateIndex(
                name: "IX_UserTags_TagId",
                table: "UserTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Tags_TagId",
                table: "UserTags",
                column: "TagId",
                principalTable: "Tags",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTags_Tags_TagId",
                table: "UserTags");

            migrationBuilder.DropIndex(
                name: "IX_UserTags_TagId",
                table: "UserTags");

            migrationBuilder.AddForeignKey(
                name: "FK_UserTags_Tags_UserId",
                table: "UserTags",
                column: "UserId",
                principalTable: "Tags",
                principalColumn: "Id");
        }
    }
}
