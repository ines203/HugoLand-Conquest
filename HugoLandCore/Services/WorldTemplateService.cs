using HugoLandCore.Data;
using HugoLandCore.Models;
using Microsoft.EntityFrameworkCore;

namespace HugoLandCore.Services;

/// <summary>
/// Service de gestion des modèles de monde (éditeur — Phase 2).
/// </summary>
public class WorldTemplateService
{
    private readonly HugoLandContext _context;

    public WorldTemplateService(HugoLandContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crée un nouveau monde vide avec tous les territoires en Plaine.
    /// </summary>
    public WorldTemplate CreateWorld(string name, string description, int width, int height)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 50)
            throw new ArgumentException("Le nom doit être entre 1 et 50 caractères.");

        if (_context.WorldTemplates.Any(w => w.Name == name))
            throw new InvalidOperationException($"Un monde avec le nom '{name}' existe déjà.");

        if (width < 10 || width > 50 || height < 10 || height > 50)
            throw new ArgumentException("Les dimensions doivent être entre 10×10 et 50×50.");

        var world = new WorldTemplate
        {
            Name = name,
            Description = description ?? "",
            Width = width,
            Height = height,
            IsValid = false,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        // Générer tous les territoires en Plaine par défaut
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                world.Territories.Add(new TerritoryTemplate
                {
                    X = x,
                    Y = y,
                    Terrain = TerrainType.Plaine
                });
            }
        }

        _context.WorldTemplates.Add(world);
        _context.SaveChanges();
        return world;
    }

    /// <summary>
    /// Modifie le terrain d'un territoire du modèle.
    /// </summary>
    public void SetTerrain(int worldId, int x, int y, TerrainType terrain)
    {
        var world = _context.WorldTemplates
            .Include(w => w.Territories)
            .FirstOrDefault(w => w.Id == worldId)
            ?? throw new InvalidOperationException("Monde introuvable.");

        var territory = world.Territories.FirstOrDefault(t => t.X == x && t.Y == y)
            ?? throw new InvalidOperationException($"Territoire ({x},{y}) introuvable.");

        territory.Terrain = terrain;
        world.IsValid = false; // invalider jusqu'à revalidation
        world.UpdatedAt = DateTime.Now;
        _context.SaveChanges();
    }

    /// <summary>
    /// Définit la position de départ d'un joueur (sur terrain Plaine uniquement).
    /// </summary>
    public void SetStartPosition(int worldId, int playerNumber, int x, int y)
    {
        var world = _context.WorldTemplates
            .Include(w => w.Territories)
            .FirstOrDefault(w => w.Id == worldId)
            ?? throw new InvalidOperationException("Monde introuvable.");

        var territory = world.Territories.FirstOrDefault(t => t.X == x && t.Y == y)
            ?? throw new InvalidOperationException($"Territoire ({x},{y}) introuvable.");

        if (territory.Terrain != TerrainType.Plaine)
            throw new InvalidOperationException("La position de départ doit être sur un terrain Plaine.");

        if (playerNumber == 1) { world.StartX1 = x; world.StartY1 = y; }
        else if (playerNumber == 2) { world.StartX2 = x; world.StartY2 = y; }

        world.IsValid = false;
        world.UpdatedAt = DateTime.Now;
        _context.SaveChanges();
    }

    /// <summary>
    /// Valide un monde selon les règles du TP.
    /// Retourne la liste des erreurs (vide = valide).
    /// </summary>
    public List<string> ValidateWorld(int worldId)
    {
        var world = _context.WorldTemplates
            .Include(w => w.Territories)
            .FirstOrDefault(w => w.Id == worldId)
            ?? throw new InvalidOperationException("Monde introuvable.");

        var errors = new List<string>();

        // 1) Position départ J1
        if (world.StartX1 == null || world.StartY1 == null)
            errors.Add("Position de départ du Joueur 1 manquante.");

        // 2) Position départ J2
        if (world.StartX2 == null || world.StartY2 == null)
            errors.Add("Position de départ du Joueur 2 manquante.");

        // 3) Positions sur terrain Plaine
        if (world.StartX1 != null && world.StartY1 != null)
        {
            var t1 = world.Territories.FirstOrDefault(t => t.X == world.StartX1 && t.Y == world.StartY1);
            if (t1?.Terrain != TerrainType.Plaine)
                errors.Add("La position de départ du Joueur 1 doit être sur un terrain Plaine.");
        }
        if (world.StartX2 != null && world.StartY2 != null)
        {
            var t2 = world.Territories.FirstOrDefault(t => t.X == world.StartX2 && t.Y == world.StartY2);
            if (t2?.Terrain != TerrainType.Plaine)
                errors.Add("La position de départ du Joueur 2 doit être sur un terrain Plaine.");
        }

        // 4) Au moins 60% de territoires accessibles (non Mer)
        int total = world.Territories.Count;
        int accessible = world.Territories.Count(t => t.Terrain != TerrainType.Mer);
        double ratio = total > 0 ? (double)accessible / total : 0;
        if (ratio < 0.60)
            errors.Add($"Au moins 60% des territoires doivent être accessibles (non Mer). Actuellement : {ratio:P0}.");

        // Marquer comme valide si aucune erreur
        if (errors.Count == 0)
        {
            world.IsValid = true;
            world.UpdatedAt = DateTime.Now;
            _context.SaveChanges();
        }

        return errors;
    }

    /// <summary>
    /// Retourne tous les mondes avec leur statut.
    /// </summary>
    public List<WorldTemplate> GetAllWorlds()
    {
        return _context.WorldTemplates
            .Include(w => w.Territories)
            .OrderByDescending(w => w.UpdatedAt)
            .ToList();
    }

    /// <summary>
    /// Charge un monde complet par son Id.
    /// </summary>
    public WorldTemplate? GetWorld(int id)
    {
        return _context.WorldTemplates
            .Include(w => w.Territories)
            .FirstOrDefault(w => w.Id == id);
    }

    /// <summary>
    /// Crée une partie à partir d'un monde validé.
    /// </summary>
    public Game CreateGameFromWorld(WorldTemplate world, string player1Name, string player2Name)
    {
        if (!world.IsValid)
            throw new InvalidOperationException("Le monde doit être validé avant de créer une partie.");

        // Créer la carte
        var map = new Map { Width = world.Width, Height = world.Height };
        _context.Maps.Add(map);
        _context.SaveChanges();

        // Copier les territoires
        var territories = new List<Territory>();
        foreach (var tt in world.Territories)
        {
            territories.Add(new Territory
            {
                X = tt.X,
                Y = tt.Y,
                Terrain = tt.Terrain,
                MapId = map.Id
            });
        }
        _context.Territories.AddRange(territories);
        _context.SaveChanges();

        // Créer la partie
        var game = new Game
        {
            Name = $"Partie {DateTime.Now:yyyy-MM-dd HH\\hmm}",
            Status = GameStatus.InProgress,
            CurrentPlayerIndex = 0,
            TurnNumber = 1,
            MapId = map.Id,
            WorldTemplateId = world.Id
        };
        _context.Games.Add(game);
        _context.SaveChanges();

        // Créer les joueurs
        var player1 = new Player { Name = player1Name, Gold = 50, GameId = game.Id };
        var player2 = new Player { Name = player2Name, Gold = 50, GameId = game.Id };
        _context.Players.AddRange(player1, player2);
        _context.SaveChanges();

        // Créer les installations et armées de départ
        var startT1 = territories.Single(t => t.X == world.StartX1 && t.Y == world.StartY1);
        var startT2 = territories.Single(t => t.X == world.StartX2 && t.Y == world.StartY2);

        _context.Installations.AddRange(
            new Installation { Type = InstallationType.Fortification, TerritoryId = startT1.Id, PlayerId = player1.Id },
            new Installation { Type = InstallationType.Fortification, TerritoryId = startT2.Id, PlayerId = player2.Id }
        );

        _context.Detachments.AddRange(
            new MilitaryDetachment { Force = 30, Energy = 5, Actionable = true, PlayerId = player1.Id, TerritoryId = startT1.Id },
            new MilitaryDetachment { Force = 30, Energy = 5, Actionable = true, PlayerId = player2.Id, TerritoryId = startT2.Id }
        );

        _context.SaveChanges();
        return game;
    }
}
