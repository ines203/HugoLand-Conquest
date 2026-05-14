using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HugoLandCore.Migrations
{
    /// <inheritdoc />
    public partial class FixTerritoryVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerritoryVisibility_Players_PlayerId",
                table: "TerritoryVisibility");

            migrationBuilder.DropForeignKey(
                name: "FK_TerritoryVisibility_Territories_TerritoryId",
                table: "TerritoryVisibility");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TerritoryVisibility",
                table: "TerritoryVisibility");

            migrationBuilder.RenameTable(
                name: "TerritoryVisibility",
                newName: "TerritoryVisibilities");

            migrationBuilder.RenameIndex(
                name: "IX_TerritoryVisibility_TerritoryId",
                table: "TerritoryVisibilities",
                newName: "IX_TerritoryVisibilities_TerritoryId");

            migrationBuilder.RenameIndex(
                name: "IX_TerritoryVisibility_PlayerId",
                table: "TerritoryVisibilities",
                newName: "IX_TerritoryVisibilities_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TerritoryVisibilities",
                table: "TerritoryVisibilities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TerritoryVisibilities_Players_PlayerId",
                table: "TerritoryVisibilities",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TerritoryVisibilities_Territories_TerritoryId",
                table: "TerritoryVisibilities",
                column: "TerritoryId",
                principalTable: "Territories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TerritoryVisibilities_Players_PlayerId",
                table: "TerritoryVisibilities");

            migrationBuilder.DropForeignKey(
                name: "FK_TerritoryVisibilities_Territories_TerritoryId",
                table: "TerritoryVisibilities");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TerritoryVisibilities",
                table: "TerritoryVisibilities");

            migrationBuilder.RenameTable(
                name: "TerritoryVisibilities",
                newName: "TerritoryVisibility");

            migrationBuilder.RenameIndex(
                name: "IX_TerritoryVisibilities_TerritoryId",
                table: "TerritoryVisibility",
                newName: "IX_TerritoryVisibility_TerritoryId");

            migrationBuilder.RenameIndex(
                name: "IX_TerritoryVisibilities_PlayerId",
                table: "TerritoryVisibility",
                newName: "IX_TerritoryVisibility_PlayerId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TerritoryVisibility",
                table: "TerritoryVisibility",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TerritoryVisibility_Players_PlayerId",
                table: "TerritoryVisibility",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TerritoryVisibility_Territories_TerritoryId",
                table: "TerritoryVisibility",
                column: "TerritoryId",
                principalTable: "Territories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
