using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grip.DAL.Migrations
{
    /// <inheritdoc />
    public partial class DbContextFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exempt_AspNetUsers_IssuedById",
                table: "Exempt");

            migrationBuilder.DropForeignKey(
                name: "FK_Exempt_AspNetUsers_IssuedToId",
                table: "Exempt");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exempt",
                table: "Exempt");

            migrationBuilder.RenameTable(
                name: "Exempt",
                newName: "Exempts");

            migrationBuilder.RenameIndex(
                name: "IX_Exempt_IssuedToId",
                table: "Exempts",
                newName: "IX_Exempts_IssuedToId");

            migrationBuilder.RenameIndex(
                name: "IX_Exempt_IssuedById",
                table: "Exempts",
                newName: "IX_Exempts_IssuedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exempts",
                table: "Exempts",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exempts_AspNetUsers_IssuedById",
                table: "Exempts",
                column: "IssuedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exempts_AspNetUsers_IssuedToId",
                table: "Exempts",
                column: "IssuedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exempts_AspNetUsers_IssuedById",
                table: "Exempts");

            migrationBuilder.DropForeignKey(
                name: "FK_Exempts_AspNetUsers_IssuedToId",
                table: "Exempts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Exempts",
                table: "Exempts");

            migrationBuilder.RenameTable(
                name: "Exempts",
                newName: "Exempt");

            migrationBuilder.RenameIndex(
                name: "IX_Exempts_IssuedToId",
                table: "Exempt",
                newName: "IX_Exempt_IssuedToId");

            migrationBuilder.RenameIndex(
                name: "IX_Exempts_IssuedById",
                table: "Exempt",
                newName: "IX_Exempt_IssuedById");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Exempt",
                table: "Exempt",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Exempt_AspNetUsers_IssuedById",
                table: "Exempt",
                column: "IssuedById",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Exempt_AspNetUsers_IssuedToId",
                table: "Exempt",
                column: "IssuedToId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
