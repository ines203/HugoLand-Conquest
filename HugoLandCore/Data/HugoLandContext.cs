using HugoLandCore.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

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
    public DbSet<TerritoryVisibility> TerritoryVisibilities => Set<TerritoryVisibility>();

    // TP2
    public DbSet<WorldTemplate> WorldTemplates => Set<WorldTemplate>();
    public DbSet<TerritoryTemplate> TerritoryTemplates => Set<TerritoryTemplate>();

    public HugoLandContext(DbContextOptions<HugoLandContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);

        // Territory -> MilitaryDetachment (1 - 0..1)
        model.Entity<Territory>()
            .HasOne(t => t.Detachment)
            .WithOne(d => d.Territory)
            .HasForeignKey<MilitaryDetachment>(d => d.TerritoryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Territory -> Installation (1 - 0..1)
        model.Entity<Territory>()
            .HasOne(t => t.Installation)
            .WithOne(i => i.Territory)
            .HasForeignKey<Installation>(i => i.TerritoryId)
            .OnDelete(DeleteBehavior.NoAction);

        // Game -> Map (1 - 1)
        model.Entity<Game>()
            .HasOne(g => g.Map)
            .WithOne()
            .HasForeignKey<Game>(g => g.MapId);

        // Map -> Territories (1 - N)
        model.Entity<Map>()
            .HasMany(m => m.Territories)
            .WithOne(t => t.Map)
            .HasForeignKey(t => t.MapId);

        // Player -> Detachments (1 - N)
        model.Entity<Player>()
            .HasMany(p => p.Detachments)
            .WithOne(d => d.Player)
            .HasForeignKey(d => d.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // TP2 : WorldTemplate -> TerritoryTemplate (1 - N)
        model.Entity<WorldTemplate>()
            .HasMany(w => w.Territories)
            .WithOne(t => t.WorldTemplate)
            .HasForeignKey(t => t.WorldTemplateId)
            .OnDelete(DeleteBehavior.Cascade);

        // TP2 : Game -> WorldTemplate (N - 0..1)
        model.Entity<Game>()
            .HasOne(g => g.WorldTemplate)
            .WithMany(w => w.Games)
            .HasForeignKey(g => g.WorldTemplateId)
            .OnDelete(DeleteBehavior.SetNull);

        // Permet de sauvegarder la visibilité des territoires par joueur
        model.Entity<TerritoryVisibility>()
            .HasOne(v => v.Territory)
            .WithMany(t => t.Visibility)
            .HasForeignKey(v => v.TerritoryId)
            .OnDelete(DeleteBehavior.Restrict);

        model.Entity<TerritoryVisibility>()
            .HasOne(v => v.Player)
            .WithMany(p => p.TerritoryVisibilities)
            .HasForeignKey(v => v.PlayerId)
            .OnDelete(DeleteBehavior.Restrict);

    }
}