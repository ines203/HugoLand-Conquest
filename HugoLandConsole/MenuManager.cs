using HugoLand.Core.Services;
using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;
using Microsoft.EntityFrameworkCore;

namespace HugoLandConsole;

/// <summary>
/// Description : Classe responsable de la gestion des menus et de la saisie utilisateur dans la console.
/// date : 2026-03-10
/// </summary>
internal class MenuManager
{
    private readonly HugoLandContext _context;
    private readonly GameDisplay _display;
    private readonly ArmyService _armyService;
    private readonly InstallationService _installationService;
    private readonly CombatService _combatService;
    private readonly EconomyService _economyService;

    public MenuManager(
        HugoLandContext context,
        GameDisplay display,
        ArmyService armyService,
        InstallationService installationService,
        CombatService combatService,
        EconomyService economyService)
    {
        _context = context;
        _display = display;
        _armyService = armyService;
        _installationService = installationService;
        _combatService = combatService;
        _economyService = economyService;
    }

    // ── Menu principal ──────────────────────────────────────────
    public void Start()
    {
        bool running = true;
        while (running)
        {
            char choice = _display.DisplayMenu();
            switch (choice)
            {
                case '1': StartNewGame(); break;
                case '2': LoadGame(); break;
                case '3': running = false; break;
            }
        }
    }

    // ── Nouvelle partie ─────────────────────────────────────────
    private void StartNewGame()
    {
        var (name1, name2) = _display.DisplayCreatePlayers();

        var game = LoadGameWithIncludes().First();

        game.CurrentPlayerIndex = 0;
        game.TurnNumber = 1;
        game.Name = $"Game {DateTime.Now:yyyy-MM-dd HH\\hmm}";

        var players = game.Players.ToList();
        if (players.Count >= 1) players[0].Name = name1;
        if (players.Count >= 2) players[1].Name = name2;

        foreach (var p in game.Players)
        {
            p.Gold = 50;
            p.DebtTurns = 0;

            var fort = p.Installations.FirstOrDefault(i => i.Type == InstallationType.Fortification);
            var detachments = p.Detachments.ToList();

            // Supprimer les détachements en trop
            for (int idx = 1; idx < detachments.Count; idx++)
            {
                if (detachments[idx].Territory != null)
                    detachments[idx].Territory!.Detachment = null;
                detachments[idx].TerritoryId = null;
                _context.Detachments.Remove(detachments[idx]);
            }

            // Replacer l'armée principale sur sa fortification
            if (detachments.Count > 0 && fort != null)
            {
                var army = detachments[0];
                if (army.Territory != null)
                    army.Territory.Detachment = null;

                var fortTerritory = game.Map!.Territories
                    .FirstOrDefault(t => t.Id == fort.TerritoryId);
                if (fortTerritory != null)
                {
                    army.TerritoryId = fortTerritory.Id;
                    army.Territory = fortTerritory;
                    fortTerritory.Detachment = army;
                }

                army.Force = 30;
                army.Actionable = true;
                army.Energy = 5;
            }
        }

        _context.SaveChanges();
        PlayGame(game);
    }

    // ── Charger une partie ──────────────────────────────────────
    private void LoadGame()
    {
        var saves = LoadGameWithIncludes().ToList();
        int index = _display.DisplaySavesChoice(saves);
        if (index < 0) return;
        PlayGame(saves[index - 1]);
    }

    // ── Boucle de jeu principale ────────────────────────────────
    private void PlayGame(Game game)
    {
        while (true)
        {
            var players = game.Players.ToList();
            Player current = players[game.CurrentPlayerIndex];

            // Vérification défaite par dette
            if (current.DebtTurns >= 5)
            {
                Console.WriteLine($"\n  {current.Name}'s armies have dissolved due to lack of supply. Eliminated!");
                game.Status = GameStatus.Finished;
                _context.SaveChanges();
                Console.WriteLine("  Press Enter to return to menu.");
                Console.ReadLine();
                return;
            }

            // Début de tour
            _display.DisplayBeginningTurn(game, current);

            // Énergie
            foreach (var army in current.Detachments.ToList())
                _armyService.GainEnergy(army.Id);

            // Revenus AVANT entretien
            _economyService.CollectRevenue(current.Id);
            _economyService.PayMaintenance(current.Id);

            _context.Entry(current).Reload();

            // Actions des armées
            bool playerAbandoned = false;
            while (true)
            {
                var actionable = current.Detachments
                    .Where(d => d.Actionable && d.Force >= 10 && d.Territory != null)
                    .ToList();

                // Afficher la carte + liste des armées
                _display.DisplayMap(game, current, game.Map!,
                    _context.Installations.Include(i => i.Territory).ToList(),
                    _context.Detachments.Include(d => d.Territory).ToList());

                _display.DisplayArmyList(current, actionable);

                if (actionable.Count == 0)
                    break;

                int sel = _display.DisplayArmySelection(current, actionable);

                if (sel == 0) break; // fin du tour volontaire
                if (sel == -1) { playerAbandoned = true; break; }

                var selectedArmy = actionable[sel - 1];

                // Afficher la carte et les actions pour cette armée
                _display.DisplayMap(game, current, game.Map!,
                    _context.Installations.Include(i => i.Territory).ToList(),
                    _context.Detachments.Include(d => d.Territory).ToList());

                int action = _display.DisplayArmyActions(current, selectedArmy);
                HandleArmyChoice(action, game, current, selectedArmy);

                // Vérifier victoire après chaque action
                if (CheckVictory(game, current)) return;
            }

            if (playerAbandoned)
            {
                Console.WriteLine($"\n  {current.Name} has forfeited. Game over.");
                game.Status = GameStatus.Finished;
                _context.SaveChanges();
                Console.ReadLine();
                return;
            }

            // Instantané de tour
            SaveTurnSnapshot(game, current);

            // Fin du tour
            EndTurn(game);
        }
    }

    // ── Fin de tour ─────────────────────────────────────────────
    private void EndTurn(Game game)
    {
        var players = game.Players.ToList();
        game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % players.Count;
        game.TurnNumber++;

        foreach (var p in game.Players)
            foreach (var a in p.Detachments)
                a.Actionable = a.Force >= 10;

        _context.SaveChanges();
    }

    // ── Vérifier victoire ───────────────────────────────────────
    private bool CheckVictory(Game game, Player current)
    {
        var opponent = game.Players.First(p => p.Id != current.Id);
        if (!opponent.Detachments.Any(d => d.Force >= 10))
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  ══════════════════════════════════");
            Console.WriteLine($"    VICTORY for {current.Name} !");
            Console.WriteLine("    The opponent has no armies left.");
            Console.WriteLine("  ══════════════════════════════════");
            Console.ResetColor();
            game.Status = GameStatus.Finished;
            _context.SaveChanges();
            Console.WriteLine("  Press Enter to return to menu.");
            Console.ReadLine();
            return true;
        }
        return false;
    }

    // ── TurnSnapshot ────────────────────────────────────────────
    private void SaveTurnSnapshot(Game game, Player player)
    {
        _context.Snapshots.Add(new TurnSnapshot
        {
            TurnNumber = game.TurnNumber,
            PlayerId = player.Id,
            GameId = game.Id,
            Gold = player.Gold,
            ArmyCount = player.Detachments.Count(d => d.Force >= 10),
            TotalSoldiers = player.Detachments.Sum(d => d.Force),
            FortificationCount = player.Installations.Count(i => i.Type == InstallationType.Fortification)
        });
        _context.SaveChanges();
    }

    // ── Choix d'action armée ────────────────────────────────────
    private void HandleArmyChoice(int choice, Game game, Player player, MilitaryDetachment army)
    {
        switch (choice)
        {
            case 1: HandleMove(game, player, army); break;
            case 2: _installationService.BuildCamp(army); break;
            case 3: _installationService.UpgradeToFort(army); break;
            case 4: HandleReinforce(player, army); break;
            case 5: HandleSplit(game, player, army); break;
            case 6: army.Actionable = false; break;
        }
        _context.SaveChanges();
    }

    private void HandleMove(Game game, Player player, MilitaryDetachment army)
    {
        char dir = _display.DisplayMoveArmy(player, game.Map!, army);
        int direction = dir switch { 'N' => 0, 'E' => 1, 'S' => 2, _ => 3 };
        _armyService.TryMove(army.Id, direction);
    }

    private void HandleReinforce(Player player, MilitaryDetachment army)
    {
        int troops = _display.DisplayReinforcement(player);
        _armyService.ReinforceArmy(player, army, troops);
    }

    private void HandleSplit(Game game, Player player, MilitaryDetachment army)
    {
        Console.Write("  How many troops to send to the new army? ");
        if (!int.TryParse(Console.ReadLine(), out int amount)) return;

        Console.Write("  Direction (N/S/E/O): ");
        string? input = Console.ReadLine();
        if (string.IsNullOrEmpty(input)) return;

        char dir = char.ToUpper(input[0]);
        int direction = dir switch { 'N' => 0, 'E' => 1, 'S' => 2, _ => 3 };
        _armyService.TrySplit(army.Id, direction, amount);
    }

    // ── Chargement EF Core ──────────────────────────────────────
    private IQueryable<Game> LoadGameWithIncludes()
    {
        return _context.Games
            .Include(g => g.Map)
                .ThenInclude(m => m.Territories)
                    .ThenInclude(t => t.Detachment)
            .Include(g => g.Map)
                .ThenInclude(m => m.Territories)
                    .ThenInclude(t => t.Installation)
            .Include(g => g.Players)
                .ThenInclude(p => p.Detachments)
                    .ThenInclude(d => d.Territory)
            .Include(g => g.Players)
                .ThenInclude(p => p.Installations)
                    .ThenInclude(i => i.Territory);
    }
}