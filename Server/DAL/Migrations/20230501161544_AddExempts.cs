using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Grip.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddExempts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupUser_AspNetUsers_UserId",
                table: "GroupUser");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "GroupUser",
                newName: "UsersId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupUser_UserId",
                table: "GroupUser",
                newName: "IX_GroupUser_UsersId");

            migrationBuilder.CreateTable(
                name: "Exempt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    IssuedById = table.Column<int>(type: "integer", nullable: false),
                    IssuedToId = table.Column<int>(type: "integer", nullable: false),
                    ValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Exempt_AspNetUsers_IssuedById",
                        column: x => x.IssuedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Exempt_AspNetUsers_IssuedToId",
                        column: x => x.IssuedToId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Station_StationNumber",
                table: "Station",
                column: "StationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exempt_IssuedById",
                table: "Exempt",
                column: "IssuedById");

            migrationBuilder.CreateIndex(
                name: "IX_Exempt_IssuedToId",
                table: "Exempt",
                column: "IssuedToId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupUser_AspNetUsers_UsersId",
                table: "GroupUser",
                column: "UsersId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GroupUser_AspNetUsers_UsersId",
                table: "GroupUser");

            migrationBuilder.DropTable(
                name: "Exempt");

            migrationBuilder.DropIndex(
                name: "IX_Station_StationNumber",
                table: "Station");

            migrationBuilder.RenameColumn(
                name: "UsersId",
                table: "GroupUser",
                newName: "UserId");

            migrationBuilder.RenameIndex(
                name: "IX_GroupUser_UsersId",
                table: "GroupUser",
                newName: "IX_GroupUser_UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_GroupUser_AspNetUsers_UserId",
                table: "GroupUser",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
