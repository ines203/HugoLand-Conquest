using HugoLand.Core.Services;
using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;
using Microsoft.EntityFrameworkCore;

namespace HugoLandConsole;

/// <summary>
/// Description : Point d'entrée de l'application console pour HugoLand.
/// Date : 2026-03-10
/// </summary>
internal class Program
{
    static void Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<HugoLandContext>()
            .UseInMemoryDatabase("HugoLandDB")
            .Options;

        var context = new HugoLandContext(options);

        // Vider et recréer la BD
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Seeder manuellement si la BD est vide
        if (!context.Games.Any())
            SeedManual(context);

        // Services
        var installationService = new InstallationService(context);
        var economyService = new EconomyService(context);
        var combatService = new CombatService(context, installationService);
        var armyService = new ArmyService(context, combatService, installationService);
        combatService.SetArmyService(armyService);
        var display = new GameDisplay(context);

        var menu = new MenuManager(
            context,
            display,
            armyService,
            installationService,
            combatService,
            economyService
        );

        menu.Start();
    }

    private static void SeedManual(HugoLandContext context)
    {
        // Map 6x6
        var map = new Map { Width = 6, Height = 6 };
        context.Maps.Add(map);
        context.SaveChanges();

        // Territoires
        for (int y = 0; y < 6; y++)
            for (int x = 0; x < 6; x++)
                context.Territories.Add(new Territory
                {
                    X = x,
                    Y = y,
                    Terrain = TerrainType.Plaine,
                    MapId = map.Id
                });
        context.SaveChanges();

        // Partie
        var game = new Game
        {
            Name = "New Game",
            Status = GameStatus.InProgress,
            CurrentPlayerIndex = 0,
            TurnNumber = 1,
            MapId = map.Id
        };
        context.Games.Add(game);
        context.SaveChanges();

        // Joueurs
        var p1 = new Player { Name = "Player 1", Gold = 50, DebtTurns = 0, GameId = game.Id };
        var p2 = new Player { Name = "Player 2", Gold = 50, DebtTurns = 0, GameId = game.Id };
        context.Players.AddRange(p1, p2);
        context.SaveChanges();

        // Fortifications aux coins opposés
        var startT1 = context.Territories.Single(t => t.X == 0 && t.Y == 5 && t.MapId == map.Id);
        var startT2 = context.Territories.Single(t => t.X == 5 && t.Y == 0 && t.MapId == map.Id);

        var fort1 = new Installation { Type = InstallationType.Fortification, TerritoryId = startT1.Id, PlayerId = p1.Id };
        var fort2 = new Installation { Type = InstallationType.Fortification, TerritoryId = startT2.Id, PlayerId = p2.Id };
        context.Installations.AddRange(fort1, fort2);
        context.SaveChanges();

        // Armées
        context.Detachments.AddRange(
            new MilitaryDetachment { Force = 30, Energy = 5, Actionable = true, PlayerId = p1.Id, TerritoryId = startT1.Id },
            new MilitaryDetachment { Force = 30, Energy = 5, Actionable = true, PlayerId = p2.Id, TerritoryId = startT2.Id }
        );
        context.SaveChanges();
    }
}