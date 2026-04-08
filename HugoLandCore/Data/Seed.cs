using Microsoft.EntityFrameworkCore;
using HugoLandCore.Models;

namespace HugoLandCore.Data;

public static class Seed
{
    public static void SeedData(this ModelBuilder model)
    {
        // 1) Map 6x6
        model.Entity<Map>().HasData(new Map
        {
            Id = 1,
            Width = 6,
            Height = 6
        });

        // 2) Territories 6x6
        var territories = new List<Territory>();
        int id = 1;

        for (int y = 0; y < 6; y++)
        {
            for (int x = 0; x < 6; x++)
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

        // 3) Game — nom generé automatiquement avec date/heure
        string gameName = $"Partie {DateTime.Now:yyyy-MM-dd HH\\hmm}";

        model.Entity<Game>().HasData(
            new Game
            {
                Id = 1,
                Name = gameName,
                Status = GameStatus.InProgress,
                CurrentPlayerIndex = 0,
                TurnNumber = 1,
                MapId = 1
            }
        );

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
        // Joueur 1 : coin bas-gauche (0,5)
        // Joueur 2 : coin haut-droit (5,0)
        var fort1 = new Installation
        {
            Id = 1,
            Type = InstallationType.Fortification,
            TerritoryId = territories.Single(t => t.X == 0 && t.Y == 5).Id,
            PlayerId = 1
        };

        var fort2 = new Installation
        {
            Id = 2,
            Type = InstallationType.Fortification,
            TerritoryId = territories.Single(t => t.X == 5 && t.Y == 0).Id,
            PlayerId = 2
        };

        model.Entity<Installation>().HasData(fort1, fort2);

        // 6) Détachements militaires initiaux
        model.Entity<MilitaryDetachment>().HasData(
            new MilitaryDetachment
            {
                Id = 1,
                Force = 30,
                Energy = 5,
                Actionable = true,
                PlayerId = 1,
                TerritoryId = fort1.TerritoryId
            },
            new MilitaryDetachment
            {
                Id = 2,
                Force = 30,
                Energy = 5,
                Actionable = true,
                PlayerId = 2,
                TerritoryId = fort2.TerritoryId
            }
        );
    }
}