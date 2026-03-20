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

        // Territory → MilitaryDetachment (1 – 0..1)
        model.Entity<Territory>()
            .HasOne(t => t.Detachment)
            .WithOne(d => d.Territory)
            .HasForeignKey<MilitaryDetachment>(d => d.TerritoryId)
            .OnDelete(DeleteBehavior.SetNull); // évite les crashs en cascade si un détachement est supprimé

        // Territory → Installation (1 – 0..1)
        model.Entity<Territory>()
            .HasOne(t => t.Installation)
            .WithOne(i => i.Territory)
            .HasForeignKey<Installation>(i => i.TerritoryId)
            .OnDelete(DeleteBehavior.SetNull);

        // Game → Map (1 – 1)
        model.Entity<Game>()
            .HasOne(g => g.Map)
            .WithOne()
            .HasForeignKey<Game>(g => g.MapId);

        // Map → Territories (1 – N)
        model.Entity<Map>()
            .HasMany(m => m.Territories)
            .WithOne(t => t.Map)
            .HasForeignKey(t => t.MapId);

        // Player → Detachments (1 – N)
        model.Entity<Player>()
            .HasMany(p => p.Detachments)
            .WithOne(d => d.Player)
            .HasForeignKey(d => d.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        model.SeedData();
    }

}
