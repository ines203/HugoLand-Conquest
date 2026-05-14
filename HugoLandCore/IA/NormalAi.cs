using HugoLand.Core.Services;
using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;
using Microsoft.EntityFrameworkCore;

namespace HugoLandCore.IA;

/// <summary>
/// IA normale — comportement simple mais cohérent :
///   1. Renforce si sur installation avec suffisamment d'or
///   2. Construit un camp si l'or le permet et aucune installation présente
///   3. Améliore camp en fort si or suffisant
///   4. Se déplace vers l'ennemi le plus proche
///   5. Mouvement aléatoire valide si aucun ennemi trouvé
///   6. Passe sinon
/// Toutes les actions sont enregistrées dans PlayerAction.
/// </summary>
public class NormalAi : IAI
{
    private readonly HugoLandContext _context;
    private readonly Player _player;
    private readonly ArmyService _armyService;
    private readonly InstallationService _installationService;
    private readonly Random _rng;

    // Log des actions effectuées ce tour (affiché en fin de tour dans le WPF)
    public List<string> ActionLog { get; } = new();

    public NormalAi(HugoLandContext context, Player player,
                    ArmyService armyService, InstallationService installationService)
    {
        _context = context;
        _player  = player;
        _armyService = armyService;
        _installationService = installationService;
        _rng = new Random();
    }

    public void PlayTurn()
    {
        ActionLog.Clear();

        // Recharger le joueur avec ses relations pour avoir les données fraîches
        var player = _context.Players
            .Include(p => p.Detachments).ThenInclude(d => d.Territory)
                .ThenInclude(t => t!.Installation)
            .Include(p => p.Installations).ThenInclude(i => i.Territory)
            .Include(p => p.Game).ThenInclude(g => g!.Map).ThenInclude(m => m!.Territories)
            .First(p => p.Id == _player.Id);

        var enemies = _context.Players
            .Include(p => p.Detachments).ThenInclude(d => d.Territory)
            .Where(p => p.GameId == player.GameId && p.Id != player.Id)
            .ToList();

        var armies = player.Detachments
            .Where(d => d.Force >= 10 && d.Actionable && d.Territory != null)
            .ToList();

        foreach (var army in armies)
        {
            // Recharger l'armée pour avoir son état le plus à jour après actions précédentes
            var fresh = _context.Detachments
                .Include(d => d.Territory).ThenInclude(t => t!.Installation)
                .Include(d => d.Player).ThenInclude(p => p!.Game)
                    .ThenInclude(g => g!.Map).ThenInclude(m => m!.Territories)
                .FirstOrDefault(d => d.Id == army.Id);

            if (fresh == null || !fresh.Actionable || fresh.Force < 10 || fresh.Territory == null)
                continue;

            // Recharger l'or du joueur (peut avoir changé)
            player = _context.Players
                .Include(p => p.Detachments).ThenInclude(d => d.Territory)
                    .ThenInclude(t => t!.Installation)
                .First(p => p.Id == _player.Id);

            ChooseAction(fresh, player, enemies);
        }
    }

    private void ChooseAction(MilitaryDetachment army, Player player, List<Player> enemies)
    {
        // Priorité 1 : Renforcer si sur installation et suffisamment d'or
        if (army.Territory!.Installation != null && player.Gold >= 10)
        {
            int amount = Math.Min(5, player.Gold / 2);
            _armyService.ReinforceArmy(player, army, amount);
            Log($"IA reinforces at ({army.Territory.X},{army.Territory.Y}): +{amount} troops", player.Id);
            return;
        }

        // Priorité 2 : Améliorer camp en fort si or suffisant
        if (army.Territory.Installation?.Type == InstallationType.Camp && player.Gold >= 50)
        {
            _installationService.UpgradeToFort(army);
            Log($"IA upgrades to fort at ({army.Territory.X},{army.Territory.Y})", player.Id);
            return;
        }

        // Priorité 3 : Construire camp si pas d'installation et or suffisant
        if (army.Territory.Installation == null && player.Gold >= 20)
        {
            _installationService.BuildCamp(army);
            Log($"IA builds camp at ({army.Territory.X},{army.Territory.Y})", player.Id);
            return;
        }

        // Priorité 4 : Se déplacer vers l'ennemi le plus proche
        if (army.Energy > 0)
        {
            int dir = GetDirectionTowardEnemy(army, enemies);
            if (dir >= 0)
            {
                Log($"IA moves army from ({army.Territory.X},{army.Territory.Y}) toward enemy", player.Id);
                _armyService.TryMove(army.Id, dir);
                return;
            }

            // Pas d'ennemi accessible : mouvement aléatoire valide
            int randomDir = GetRandomValidMove(army);
            if (randomDir >= 0)
            {
                Log($"IA moves army from ({army.Territory.X},{army.Territory.Y}) (random)", player.Id);
                _armyService.TryMove(army.Id, randomDir);
                return;
            }
        }

        // Priorité 5 : Passer
        army.Actionable = false;
        _context.PlayerActions.Add(new PlayerAction
        {
            Description = $"IA passes at ({army.Territory.X},{army.Territory.Y})",
            Timestamp   = DateTime.Now,
            PlayerId    = player.Id
        });
        _context.SaveChanges();
        Log($"IA passes for army at ({army.Territory.X},{army.Territory.Y})", player.Id);
    }

    /// <summary>
    /// Trouve la direction (0=N,1=E,2=S,3=O) vers l'ennemi le plus proche
    /// en privilégiant les cases accessibles avec l'énergie disponible.
    /// </summary>
    private int GetDirectionTowardEnemy(MilitaryDetachment army, List<Player> enemies)
    {
        var enemyTerritories = enemies
            .SelectMany(e => e.Detachments)
            .Where(d => d.Territory != null)
            .Select(d => d.Territory!)
            .ToList();

        if (!enemyTerritories.Any()) return -1;

        // Trouver l'ennemi le plus proche (distance Manhattan)
        var nearest = enemyTerritories
            .OrderBy(t => Math.Abs(t.X - army.Territory!.X) + Math.Abs(t.Y - army.Territory.Y))
            .First();

        int dx = nearest.X - army.Territory!.X;
        int dy = nearest.Y - army.Territory.Y;

        // Directions préférées vers la cible
        var preferred = new List<int>();
        if (Math.Abs(dx) >= Math.Abs(dy))
        {
            if (dx > 0) preferred.Add(1); else if (dx < 0) preferred.Add(3);
            if (dy > 0) preferred.Add(2); else if (dy < 0) preferred.Add(0);
        }
        else
        {
            if (dy > 0) preferred.Add(2); else if (dy < 0) preferred.Add(0);
            if (dx > 0) preferred.Add(1); else if (dx < 0) preferred.Add(3);
        }

        // Charger la carte via le contexte
        var map = _context.Maps
            .First(m => m.Territories.Any(t => t.Id == army.TerritoryId));
        int w = map.Width, h = map.Height;

        foreach (int dir in preferred)
        {
            int nx = army.Territory.X + (dir == 1 ? 1 : dir == 3 ? -1 : 0);
            int ny = army.Territory.Y  + (dir == 2 ? 1 : dir == 0 ? -1 : 0);

            if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;

            var dest = _context.Territories
                .FirstOrDefault(t => t.MapId == map.Id && t.X == nx && t.Y == ny);
            if (dest == null) continue;
            if (dest.Terrain == TerrainType.Mer) continue;
            if (army.Energy < dest.TerrainEnergyCost) continue;

            return dir;
        }

        return -1;
    }

    /// <summary>
    /// Retourne une direction aléatoire valide (énergie suffisante, pas de mer, dans les limites).
    /// </summary>
    private int GetRandomValidMove(MilitaryDetachment army)
    {
        int[] dirs = { 0, 1, 2, 3 };
        dirs = dirs.OrderBy(_ => _rng.Next()).ToArray();

        var map = _context.Maps
            .First(m => m.Territories.Any(t => t.Id == army.TerritoryId));
        int w = map.Width, h = map.Height;

        foreach (int dir in dirs)
        {
            int nx = army.Territory!.X + (dir == 1 ? 1 : dir == 3 ? -1 : 0);
            int ny = army.Territory.Y  + (dir == 2 ? 1 : dir == 0 ? -1 : 0);

            if (nx < 0 || nx >= w || ny < 0 || ny >= h) continue;

            var dest = _context.Territories
                .FirstOrDefault(t => t.MapId == map.Id && t.X == nx && t.Y == ny);
            if (dest == null) continue;
            if (dest.Terrain == TerrainType.Mer) continue;
            if (army.Energy < dest.TerrainEnergyCost) continue;

            return dir;
        }
        return -1;
    }

    private void Log(string msg, int playerId)
    {
        ActionLog.Add(msg);
        // Enregistré dans PlayerAction comme requis par l'énoncé
        _context.PlayerActions.Add(new PlayerAction
        {
            Description = msg,
            Timestamp   = DateTime.Now,
            PlayerId    = playerId
        });
    }
}
