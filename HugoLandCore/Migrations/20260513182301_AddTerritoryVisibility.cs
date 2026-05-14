using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HugoLandCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTerritoryVisibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TerritoryVisibility",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TerritoryId = table.Column<int>(type: "int", nullable: false),
                    Explored = table.Column<bool>(type: "bit", nullable: false),
                    Visible = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerritoryVisibility", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerritoryVisibility_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TerritoryVisibility_Territories_TerritoryId",
                        column: x => x.TerritoryId,
                        principalTable: "Territories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TerritoryVisibility_PlayerId",
                table: "TerritoryVisibility",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_TerritoryVisibility_TerritoryId",
                table: "TerritoryVisibility",
                column: "TerritoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TerritoryVisibility");
        }
    }
}
