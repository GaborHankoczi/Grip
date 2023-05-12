using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Grip.DAL.Migrations
{
    /// <inheritdoc />
    public partial class ClassStationRelationAdded : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StationId",
                table: "Class",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Class_StationId",
                table: "Class",
                column: "StationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Class_Station_StationId",
                table: "Class",
                column: "StationId",
                principalTable: "Station",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Class_Station_StationId",
                table: "Class");

            migrationBuilder.DropIndex(
                name: "IX_Class_StationId",
                table: "Class");

            migrationBuilder.DropColumn(
                name: "StationId",
                table: "Class");
        }
    }
}
