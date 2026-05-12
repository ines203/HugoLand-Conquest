using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HugoLandCore.Migrations
{
    /// <inheritdoc />
    public partial class HugoLandDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Maps",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maps", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "WorldTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Width = table.Column<int>(type: "int", nullable: false),
                    Height = table.Column<int>(type: "int", nullable: false),
                    IsValid = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartX1 = table.Column<int>(type: "int", nullable: true),
                    StartY1 = table.Column<int>(type: "int", nullable: true),
                    StartX2 = table.Column<int>(type: "int", nullable: true),
                    StartY2 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorldTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Territories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Terrain = table.Column<int>(type: "int", nullable: false),
                    MapId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Territories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Territories_Maps_MapId",
                        column: x => x.MapId,
                        principalTable: "Maps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Games",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentPlayerIndex = table.Column<int>(type: "int", nullable: false),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    MapId = table.Column<int>(type: "int", nullable: false),
                    WorldTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Games", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Games_Maps_MapId",
                        column: x => x.MapId,
                        principalTable: "Maps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Games_WorldTemplates_WorldTemplateId",
                        column: x => x.WorldTemplateId,
                        principalTable: "WorldTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TerritoryTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    X = table.Column<int>(type: "int", nullable: false),
                    Y = table.Column<int>(type: "int", nullable: false),
                    Terrain = table.Column<int>(type: "int", nullable: false),
                    WorldTemplateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TerritoryTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TerritoryTemplates_WorldTemplates_WorldTemplateId",
                        column: x => x.WorldTemplateId,
                        principalTable: "WorldTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CombatEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttackerPlayerId = table.Column<int>(type: "int", nullable: false),
                    DefenderPlayerId = table.Column<int>(type: "int", nullable: false),
                    TerritoryId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Result = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CombatEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CombatEvents_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    DebtTurns = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Snapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TurnNumber = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    Gold = table.Column<int>(type: "int", nullable: false),
                    ArmyCount = table.Column<int>(type: "int", nullable: false),
                    TotalSoldiers = table.Column<int>(type: "int", nullable: false),
                    FortificationCount = table.Column<int>(type: "int", nullable: false),
                    GameId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Snapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Snapshots_Games_GameId",
                        column: x => x.GameId,
                        principalTable: "Games",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Detachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Force = table.Column<int>(type: "int", nullable: false),
                    Energy = table.Column<int>(type: "int", nullable: false),
                    Actionable = table.Column<bool>(type: "bit", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false),
                    TerritoryId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Detachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Detachments_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Detachments_Territories_TerritoryId",
                        column: x => x.TerritoryId,
                        principalTable: "Territories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Installations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<int>(type: "int", nullable: false),
                    IsOccupied = table.Column<bool>(type: "bit", nullable: false),
                    TerritoryId = table.Column<int>(type: "int", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Installations_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Installations_Territories_TerritoryId",
                        column: x => x.TerritoryId,
                        principalTable: "Territories",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PlayerActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PlayerId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerActions_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CombatEvents_GameId",
                table: "CombatEvents",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Detachments_PlayerId",
                table: "Detachments",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Detachments_TerritoryId",
                table: "Detachments",
                column: "TerritoryId",
                unique: true,
                filter: "[TerritoryId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Games_MapId",
                table: "Games",
                column: "MapId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Games_WorldTemplateId",
                table: "Games",
                column: "WorldTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_Installations_PlayerId",
                table: "Installations",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Installations_TerritoryId",
                table: "Installations",
                column: "TerritoryId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerActions_PlayerId",
                table: "PlayerActions",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_GameId",
                table: "Players",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Snapshots_GameId",
                table: "Snapshots",
                column: "GameId");

            migrationBuilder.CreateIndex(
                name: "IX_Territories_MapId",
                table: "Territories",
                column: "MapId");

            migrationBuilder.CreateIndex(
                name: "IX_TerritoryTemplates_WorldTemplateId",
                table: "TerritoryTemplates",
                column: "WorldTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CombatEvents");

            migrationBuilder.DropTable(
                name: "Detachments");

            migrationBuilder.DropTable(
                name: "Installations");

            migrationBuilder.DropTable(
                name: "PlayerActions");

            migrationBuilder.DropTable(
                name: "Snapshots");

            migrationBuilder.DropTable(
                name: "TerritoryTemplates");

            migrationBuilder.DropTable(
                name: "Territories");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Games");

            migrationBuilder.DropTable(
                name: "Maps");

            migrationBuilder.DropTable(
                name: "WorldTemplates");
        }
    }
}
