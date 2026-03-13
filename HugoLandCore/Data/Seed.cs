using HugoLandCore.Models;
using Microsoft.EntityFrameworkCore;

namespace HugoLandCore.Data;

public static class Seed
{
    public static void SeedData(this ModelBuilder model)
    {
        // 1) Map
        model.Entity<Map>().HasData(new Map
        {
            Id = 1,
            Width = 4,
            Height = 4
        });

        // 2) Territories 4×4
        var territories = new List<Territory>();
        int id = 1;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                territories.Add(new Territory
                {
                    Id = id++,
                    X = x,
                    Y = y,
                    Terrain = TerrainType.Plaine,
                    MapId = 1
                });
            }
        }

        model.Entity<Territory>().HasData(territories);

        // 3) Game
        model.Entity<Game>().HasData(new Game
        {
            Id = 1,
            Name = "Partie initiale",
            Status = GameStatus.InProgress,
            CurrentPlayerIndex = 0,
            TurnNumber = 1,
            MapId = 1
        });

        // 4) Players
        model.Entity<Player>().HasData(
            new Player
            {
                Id = 1,
                Name = "Joueur 1",
                Gold = 50,
                DebtTurns = 0,
                GameId = 1
            },
            new Player
            {
                Id = 2,
                Name = "Joueur 2",
                Gold = 50,
                DebtTurns = 0,
                GameId = 1
            }
        );

        // 5) Installations (fortifications de départ)
        var fort1 = new Installation
        {
            Id = 1,
            Type = InstallationType.Fortification,
            TerritoryId = territories.Single(t => t.X == 0 && t.Y == 3).Id
        };

        var fort2 = new Installation
        {
            Id = 2,
            Type = InstallationType.Fortification,
            TerritoryId = territories.Single(t => t.X == 3 && t.Y == 0).Id
        };

        model.Entity<Installation>().HasData(fort1, fort2);

        // 6) Détachements militaires initiaux
        model.Entity<MilitaryDetachment>().HasData(
            new MilitaryDetachment
            {
                Id = 1,
                Force = 30,
                Energy = 5,
                PlayerId = 1,
                TerritoryId = fort1.TerritoryId
            },
            new MilitaryDetachment
            {
                Id = 2,
                Force = 30,
                Energy = 5,
                PlayerId = 2,
                TerritoryId = fort2.TerritoryId
            }
        );
    }
}
