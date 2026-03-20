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

    public MenuManager(
        HugoLandContext context,
        GameDisplay display,
        ArmyService armyService,
        InstallationService installationService,
        CombatService combatService)
    {
        _context = context;
        _display = display;
        _armyService = armyService;
        _installationService = installationService;
        _combatService = combatService;
    }

    //  Menu principal
    public void Start()
    {
        bool running = true;

        while (running)
        {
            char choice = _display.DisplayMenu();

            switch (choice)
            {
                case '1':
                    StartNewGame();
                    break;

                case '2':
                    LoadGame();
                    break;

                case '3':
                    running = false;
                    break;
            }
        }
    }

    //  Nouvelle partie
    private void StartNewGame()
    {
        var game = _context.Games
            .Include(g => g.Map)
                .ThenInclude(m => m.Territories)
            .Include(g => g.Players)
                .ThenInclude(p => p.Detachments)
            .Include(g => g.Players)
                .ThenInclude(p => p.Installations)
            .First();

        // reset les données pour une nouvelle partie
        game.CurrentPlayerIndex = 0;
        game.TurnNumber = 1;

        foreach (var p in game.Players)
        {
            foreach (var a in p.Detachments)
            {
                a.Actionable = true;
                a.Energy = 5;
            }
        }

        _context.SaveChanges();


        PlayGame(game);
    }

    //  Charger une partie
    private void LoadGame()
    {
        var saves = _context.Games
            .Include(g => g.Map)
                .ThenInclude(m => m.Territories)
            .Include(g => g.Players)
                .ThenInclude(p => p.Detachments)
            .Include(g => g.Players)
                .ThenInclude(p => p.Installations)
            .ToList();

        int index = _display.DisplaySavesChoice(saves);

        var selected = saves[index - 1];

        PlayGame(selected);
    }

    //  Boucle de jeu principale
    private void PlayGame(Game game)
    {
        bool playing = true;

        while (playing)
        {
            var players = game.Players.ToList();
            Player current = players[game.CurrentPlayerIndex];

            // 1. Début de tour (affichage)
            _display.DisplayBeginningTurn(game, current);

            // 2. Énergie + revenus + entretien
            ApplyStartOfTurn(game, current);

            // 3. Actions des armées
            foreach (var army in current.Detachments.Where(d => d.Actionable))
            {
                _display.DisplayMap(
                    game,
                    current,
                    game.Map,
                    _context.Installations.ToList(),
                    _context.Detachments.ToList()
                );

                int choice = _display.DisplayArmyActions(current, army);

                HandleArmyChoice(choice, game, current, army);
            }

            // 4. Fin du tour → joueur suivant
            EndTurn(game);
        }
    }

    //  Début de tour : énergie, revenus, entretien
    private void ApplyStartOfTurn(Game game, Player player)
    {
        // Énergie
        foreach (var army in player.Detachments)
        {
            army.Energy = Math.Min(5, army.Energy + 2);
        }

        // Revenus
        int fortCount = player.Installations.Count(i => i.Type == InstallationType.Fortification);
        player.Gold += fortCount * 5;

        // Entretien
        int maintenance = 0;
        foreach (var army in player.Detachments)
        {
            if (army.Force >= 10)
                maintenance += (int)Math.Ceiling(army.Force / 10.0);
        }

        player.Gold -= maintenance;

        _context.SaveChanges();
    }

    //  Fin de tour
    private void EndTurn(Game game)
    {
        var players = game.Players.ToList();
        game.CurrentPlayerIndex = (game.CurrentPlayerIndex + 1) % players.Count;
        game.TurnNumber++;

        // Reset actionable
        foreach (var p in game.Players)
            foreach (var a in p.Detachments)
                a.Actionable = true;

        _context.SaveChanges();
    }

    //  Choix de l'armée
    private void HandleArmyChoice(int choice, Game game, Player player, MilitaryDetachment army)
    {
        switch (choice)
        {
            case 1:
                HandleMove(game, player, army);
                break;

            case 2:
                _installationService.BuildCamp(army);
                break;

            case 3:
                _installationService.UpgradeToFort(army);
                break;

            case 4:
                HandleReinforce(player, army);
                break;

            case 5:
                HandleSplit(game, player, army);
                break;

            case 6:
                army.Actionable = false;
                break;
        }

        _context.SaveChanges();
    }


    //  Déplacement
    private void HandleMove(Game game, Player player, MilitaryDetachment army)
    {
        char dir = _display.DisplayMoveArmy(player, game.Map, army);

        int direction = dir switch
        {
            'N' => 0,
            'E' => 1,
            'S' => 2,
            'W' => 3,
            _ => 0
        };

        _armyService.TryMove(army.Id, direction);
    }


    //  Renforcement
    private void HandleReinforce(Player player, MilitaryDetachment army)
    {
        Console.Write("How many soldiers to add? ");
        if (int.TryParse(Console.ReadLine(), out int amount))
        {
            _armyService.ReinforceArmy(player, army, amount);
        }
    }

    //  Split
    private void HandleSplit(Game game, Player player, MilitaryDetachment army)
    {
        Console.Write("How many soldiers to send to the new army? ");
        if (int.TryParse(Console.ReadLine(), out int amount))
        {
            Console.Write("Direction (N/S/E/W): ");
            char dir = Console.ReadLine().ToUpper()[0];

            int direction = dir switch
            {
                'N' => 0,
                'E' => 1,
                'S' => 2,
                'W' => 3,
                _ => 0
            };

            _armyService.TrySplit(army.Id, direction, amount);
        }
    }

}
