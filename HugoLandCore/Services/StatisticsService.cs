using HugoLandCore.Data;
using HugoLandCore.Models;
using Microsoft.EntityFrameworkCore;

namespace HugoLandCore.Services;

// ─── DTOs de statistiques ──────────────────────────────────────────────────

public record PlayerDashboard(
    string Name,
    int Gold,
    int DebtTurns,
    int ArmyCount,
    int TotalForce,
    int FortCount,
    int Revenue,
    int Maintenance
);

public record ForceSnapshot(int Turn, int ForceP1, int ForceP2);

public record GameReport(
    string GameName,
    int TotalTurns,
    string Winner,
    string Loser,
    // Statistiques finales
    int WinnerGold,
    int WinnerForce,
    int WinnerForts,
    int LoserGold,
    int LoserForce,
    int LoserForts,
    // Actions
    List<(string Action, int Count)> TopActions,
    // Combats par territoire
    List<(int X, int Y, int CombatCount)> CombatHeatmap,
    // Évolution de la force
    List<ForceSnapshot> ForceHistory
);

public record GameHistoryEntry(
    int GameId,
    string GameName,
    string MapSize,
    string Result,
    int TotalTurns,
    DateTime Date
);

public record GlobalStats(
    int TotalGames,
    double WinRate,
    double AvgTurnsPerVictory,
    double AvgGoldPerGame
);

// ─── Service ──────────────────────────────────────────────────────────────

/// <summary>
/// Service de statistiques — Phase 3.
/// Fournit toutes les requêtes LINQ pour le tableau de bord,
/// le rapport post-partie, l'historique et les performances globales.
/// </summary>
public class StatisticsService
{
    private readonly HugoLandContext _context;

    public StatisticsService(HugoLandContext context)
    {
        _context = context;
    }

    // ── 7.1 Tableau de bord temps réel ────────────────────────────────────

    /// <summary>
    /// Calcule les métriques en temps réel pour les deux joueurs d'une partie.
    /// Utilisé pour mettre à jour le panneau comparatif après chaque action.
    /// </summary>
    public (PlayerDashboard p1, PlayerDashboard p2) GetLiveDashboard(int gameId)
    {
        var game = _context.Games
            .Include(g => g.Players)
                .ThenInclude(p => p.Detachments)
            .Include(g => g.Players)
                .ThenInclude(p => p.Installations)
                    .ThenInclude(i => i.Territory)
                        .ThenInclude(t => t!.Detachment)
            .First(g => g.Id == gameId);

        var players = game.Players.OrderBy(p => p.Id).ToList();
        return (BuildDashboard(players[0]), BuildDashboard(players[1]));
    }

    private static PlayerDashboard BuildDashboard(Player p)
    {
        int armies  = p.Detachments.Count(d => d.Force >= 10);
        int force   = p.Detachments.Sum(d => d.Force);
        int forts   = p.Installations.Count(i => i.Type == InstallationType.Fortification);

        // Fortification occupée = fort + armée >= 10 du même joueur sur le territoire
        int revenue = p.Installations.Count(i =>
            i.Type == InstallationType.Fortification
            && i.Territory?.Detachment != null
            && i.Territory.Detachment.PlayerId == p.Id
            && i.Territory.Detachment.Force >= 10) * 5;

        int maint = p.Detachments
            .Where(d => d.Force >= 10)
            .Sum(d => (int)Math.Ceiling(d.Force / 10.0));

        return new PlayerDashboard(p.Name, p.Gold, p.DebtTurns,
                                   armies, force, forts, revenue, maint);
    }

    // ── Graphique d'évolution de la force (TurnSnapshots) ─────────────────

    /// <summary>
    /// Retourne l'évolution de la force des deux joueurs sur les N derniers tours.
    /// Requête LINQ sur TurnSnapshots — utilisée pour le graphique temps réel.
    /// </summary>
    public List<ForceSnapshot> GetForceHistory(int gameId, int lastNTurns = 20)
    {
        var players = _context.Games
            .Where(g => g.Id == gameId)
            .SelectMany(g => g.Players)
            .OrderBy(p => p.Id)
            .Select(p => p.Id)
            .ToList();

        if (players.Count < 2) return new();

        int p1Id = players[0], p2Id = players[1];

        // Récupérer les snapshots des deux joueurs, regroupés par tour
        var snapshots = _context.Snapshots
            .Where(s => s.GameId == gameId)
            .OrderBy(s => s.TurnNumber)
            .ToList();

        var byTurn = snapshots
            .GroupBy(s => s.TurnNumber)
            .OrderBy(g => g.Key)
            .TakeLast(lastNTurns)
            .Select(g => new ForceSnapshot(
                g.Key,
                g.FirstOrDefault(s => s.PlayerId == p1Id)?.TotalSoldiers ?? 0,
                g.FirstOrDefault(s => s.PlayerId == p2Id)?.TotalSoldiers ?? 0
            ))
            .ToList();

        return byTurn;
    }

    // ── 7.2 Rapport post-partie ────────────────────────────────────────────

    /// <summary>
    /// Génère le rapport complet d'une partie terminée.
    /// Inclut : durée, statistiques finales, top 5 actions, carte des combats.
    /// </summary>
    public GameReport? GetGameReport(int gameId)
    {
        var game = _context.Games
            .Include(g => g.Map).ThenInclude(m => m.Territories)
            .Include(g => g.Players)
                .ThenInclude(p => p.Detachments)
            .Include(g => g.Players)
                .ThenInclude(p => p.Installations)
            .FirstOrDefault(g => g.Id == gameId);

        if (game == null) return null;

        var players = game.Players.OrderBy(p => p.Id).ToList();
        if (players.Count < 2) return null;

        // Dernier snapshot pour les stats finales
        var lastSnapP1 = _context.Snapshots
            .Where(s => s.GameId == gameId && s.PlayerId == players[0].Id)
            .OrderByDescending(s => s.TurnNumber)
            .FirstOrDefault();

        var lastSnapP2 = _context.Snapshots
            .Where(s => s.GameId == gameId && s.PlayerId == players[1].Id)
            .OrderByDescending(s => s.TurnNumber)
            .FirstOrDefault();

        // Déterminer le gagnant : celui qui a encore des armées (≥10 soldats)
        var p1 = players[0];
        var p2 = players[1];
        bool p1HasArmies = p1.Detachments.Any(d => d.Force >= 10);
        Player winner = p1HasArmies ? p1 : p2;
        Player loser  = p1HasArmies ? p2 : p1;

        // Top 5 types d'actions (requête sur PlayerAction, groupé par mot-clé)
        var topActions = _context.PlayerActions
            .Where(a => a.PlayerId == p1.Id || a.PlayerId == p2.Id)
            .ToList()
            .GroupBy(a => ExtractActionType(a.Description))
            .OrderByDescending(g => g.Count())
            .Take(5)
            .Select(g => (g.Key, g.Count()))
            .ToList();

        // Carte des combats : combats groupés par territoire
        var gameMapId = game.Map?.Id ?? 0;
        var combatHeatmap = _context.CombatEvents
            .Where(c => c.GameId == gameId)
            .Join(_context.Territories.Where(t => t.MapId == gameMapId),
                  c => c.TerritoryId,
                  t => t.Id,
                  (c, t) => new { t.X, t.Y })
            .GroupBy(x => new { x.X, x.Y })
            .Select(g => new { g.Key.X, g.Key.Y, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList()
            .Select(x => (x.X, x.Y, x.Count))
            .ToList();

        // Évolution de la force
        var forceHistory = GetForceHistory(gameId);

        return new GameReport(
            game.Name,
            game.TurnNumber,
            winner.Name, loser.Name,
            lastSnapP1?.Gold ?? p1.Gold,
            lastSnapP1?.TotalSoldiers ?? p1.Detachments.Sum(d => d.Force),
            lastSnapP1?.FortificationCount ?? p1.Installations.Count(i => i.Type == InstallationType.Fortification),
            lastSnapP2?.Gold ?? p2.Gold,
            lastSnapP2?.TotalSoldiers ?? p2.Detachments.Sum(d => d.Force),
            lastSnapP2?.FortificationCount ?? p2.Installations.Count(i => i.Type == InstallationType.Fortification),
            topActions,
            combatHeatmap,
            forceHistory
        );
    }

    /// <summary>Extrait le type d'action depuis la description textuelle.</summary>
    private static string ExtractActionType(string description)
    {
        if (description.StartsWith("Move"))       return "Move";
        if (description.StartsWith("Split"))      return "Split";
        if (description.StartsWith("Build camp")) return "Build Camp";
        if (description.StartsWith("Upgrade"))    return "Fortify";
        if (description.StartsWith("Reinforce"))  return "Reinforce";
        if (description.StartsWith("Pass"))       return "Pass";
        if (description.Contains("IA"))           return "AI Action";
        return "Other";
    }

    // ── 7.3 Historique des parties ────────────────────────────────────────

    /// <summary>
    /// Retourne la liste des parties avec filtres, tri et métadonnées.
    /// </summary>
    public List<GameHistoryEntry> GetGameHistory(
        string? filterResult = null,   // "Victory", "Defeat", null = tout
        string? filterMapSize = null,  // ex. "15×10"
        string sortBy = "date",        // "date" | "turns"
        bool descending = true)
    {
        var query = _context.Games
            .Include(g => g.Map)
            .Include(g => g.Players)
                .ThenInclude(p => p.Detachments)
            .AsQueryable();

        // Seules les parties terminées sont dans l'historique
        query = query.Where(g => g.Status == GameStatus.Finished);

        // Filtre carte
        if (!string.IsNullOrEmpty(filterMapSize))
        {
            query = query.Where(g =>
                (g.Map!.Width + "×" + g.Map.Height) == filterMapSize);
        }

        var games = query.ToList();

        var entries = games.Select(g =>
        {
            var players = g.Players.OrderBy(p => p.Id).ToList();
            string result = "N/A";
            if (players.Count >= 2)
            {
                bool p1HasArmies = players[0].Detachments.Any(d => d.Force >= 10);
                result = p1HasArmies
                    ? $"{players[0].Name} wins"
                    : $"{players[1].Name} wins";
            }
            string mapSize = g.Map != null
                ? $"{g.Map.Width}×{g.Map.Height}"
                : "?×?";

            return new GameHistoryEntry(
                g.Id,
                g.Name,
                mapSize,
                result,
                g.TurnNumber,
                DateTime.Now
            );
        }).ToList();

        // Filtre résultat
        if (!string.IsNullOrEmpty(filterResult))
            entries = entries.Where(e => e.Result.Contains(filterResult)).ToList();

        // Tri
        entries = sortBy switch
        {
            "turns" => descending
                ? entries.OrderByDescending(e => e.TotalTurns).ToList()
                : entries.OrderBy(e => e.TotalTurns).ToList(),
            _ => descending
                ? entries.OrderByDescending(e => e.GameId).ToList()
                : entries.OrderBy(e => e.GameId).ToList()
        };

        return entries;
    }

    /// <summary>
    /// Performances globales — agrégations sur Game + TurnSnapshot.
    /// </summary>
    public GlobalStats GetGlobalStats()
    {
        var finishedGames = _context.Games
            .Include(g => g.Players).ThenInclude(p => p.Detachments)
            .Where(g => g.Status == GameStatus.Finished)
            .ToList();

        int total = finishedGames.Count;
        if (total == 0)
            return new GlobalStats(0, 0, 0, 0);

        double winRate = 100.0;

        // Tours moyen par partie terminée
        double avgTurns = finishedGames.Average(g => (double)g.TurnNumber);

        // Or moyen — sécurisé si aucun snapshot
        var finishedIds = finishedGames.Select(g => g.Id).ToList();
        var lastSnaps = _context.Snapshots
            .Where(s => finishedIds.Contains(s.GameId))
            .ToList()
            .GroupBy(s => new { s.GameId, s.PlayerId })
            .Select(g => g.OrderByDescending(s => s.TurnNumber).First())
            .ToList();

        double avgGold = lastSnaps.Any()
            ? lastSnaps.Average(s => (double)s.Gold)
            : 0;

        return new GlobalStats(total, winRate, avgTurns, avgGold);
    }
}
