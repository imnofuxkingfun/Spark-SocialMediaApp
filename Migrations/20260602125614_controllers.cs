using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Spark_SocialMediaApp.Migrations
{
    /// <inheritdoc />
    public partial class controllers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserConnections_AspNetUsers_UserId",
                table: "UserConnections");

            migrationBuilder.DropIndex(
                name: "IX_UserConnections_UserId",
                table: "UserConnections");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserConnections");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "UserConnections",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "PostId",
                table: "GroupchatMessages",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GroupchatMessages_PostId",
                table: "GroupchatMessages",
                column: "PostId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupchatMessages_Posts_PostId",
                table: "GroupchatMessages",
                column: "PostId",
                principalTable: "Posts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupchatMessages_Posts_PostId",
                table: "GroupchatMessages");

            migrationBuilder.DropIndex(
                name: "IX_GroupchatMessages_PostId",
                table: "GroupchatMessages");

            migrationBuilder.DropColumn(
                name: "PostId",
                table: "GroupchatMessages");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "UserConnections",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserConnections",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserConnections_UserId",
                table: "UserConnections",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserConnections_AspNetUsers_UserId",
                table: "UserConnections",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
