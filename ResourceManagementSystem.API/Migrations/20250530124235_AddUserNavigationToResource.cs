using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ResourceManagementSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUserNavigationToResource : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Resources_CreatedById",
                table: "Resources",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Resources_LastUpdatedById",
                table: "Resources",
                column: "LastUpdatedById");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Users_CreatedById",
                table: "Resources",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_Users_LastUpdatedById",
                table: "Resources",
                column: "LastUpdatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Users_CreatedById",
                table: "Resources");

            migrationBuilder.DropForeignKey(
                name: "FK_Resources_Users_LastUpdatedById",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_CreatedById",
                table: "Resources");

            migrationBuilder.DropIndex(
                name: "IX_Resources_LastUpdatedById",
                table: "Resources");
        }
    }
}
