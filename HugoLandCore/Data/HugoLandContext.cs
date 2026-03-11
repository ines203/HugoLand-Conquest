using Microsoft.EntityFrameworkCore;
using HugoLandCore.Models;

namespace HugoLandCore.Data;

public class HugoLandContext : DbContext
{
    public DbSet<Game> Games => Set<Game>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Map> Maps => Set<Map>();
    public DbSet<Territory> Territories => Set<Territory>();
    public DbSet<MilitaryDetachment> Detachments => Set<MilitaryDetachment>();
    public DbSet<Installation> Installations => Set<Installation>();
    public DbSet<PlayerAction> PlayerActions => Set<PlayerAction>();
    public DbSet<CombatEvent> CombatEvents => Set<CombatEvent>();
    public DbSet<TurnSnapshot> Snapshots => Set<TurnSnapshot>();

    public HugoLandContext(DbContextOptions<HugoLandContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // Territory → Detachment (1-1)
        model.Entity<Territory>()
            .HasOne(t => t.Detachment)
            .WithOne(d => d.Territory)
            .HasForeignKey<MilitaryDetachment>(d => d.TerritoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Territory → Installation (1-1)
        model.Entity<Territory>()
            .HasOne(t => t.Installation)
            .WithOne(i => i.Territory)
            .HasForeignKey<Installation>(i => i.TerritoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // Game → Map (1-1)
        model.Entity<Game>()
            .HasOne(g => g.Map)
            .WithOne()
            .HasForeignKey<Game>(g => g.MapId);

        // Map → Territories (1-N)
        model.Entity<Map>()
            .HasMany(m => m.Territories)
            .WithOne(t => t.Map)
            .HasForeignKey(t => t.MapId);

        // Player → Detachments (1-N)
        model.Entity<Player>()
            .HasMany(p => p.Detachments)
            .WithOne(d => d.Player)
            .HasForeignKey(d => d.PlayerId);

        // ============================
        // SEED INITIAL (carte 4×4)
        // ============================

        // 1) Map
        var map = new Map { Id = 1, Width = 4, Height = 4 };
        model.Entity<Map>().HasData(map);

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
        var game = new Game
        {
            Id = 1,
            Name = "Partie initiale",
            Status = GameStatus.InProgress,
            CurrentPlayerIndex = 0,
            TurnNumber = 1,
            MapId = 1
        };
        model.Entity<Game>().HasData(game);

        // 4) Players
        var player1 = new Player
        {
            Id = 1,
            Name = "Joueur 1",
            Gold = 50,
            DebtTurns = 0,
            GameId = 1
        };

        var player2 = new Player
        {
            Id = 2,
            Name = "Joueur 2",
            Gold = 50,
            DebtTurns = 0,
            GameId = 1
        };

        model.Entity<Player>().HasData(player1, player2);

        // 5) Installations (fortifications de départ)
        // Joueur 1 → bas-gauche = (0,3) → territoire Id = 13
        // Joueur 2 → haut-droite = (3,0) → territoire Id = 4

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
        var det1 = new MilitaryDetachment
        {
            Id = 1,
            Force = 30,
            Energy = 5,
            PlayerId = 1,
            TerritoryId = fort1.TerritoryId
        };

        var det2 = new MilitaryDetachment
        {
            Id = 2,
            Force = 30,
            Energy = 5,
            PlayerId = 2,
            TerritoryId = fort2.TerritoryId
        };

        model.Entity<MilitaryDetachment>().HasData(det1, det2);
    }
}
