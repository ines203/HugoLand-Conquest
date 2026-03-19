using HugoLand.Core.Services;
using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;

namespace HugoLandConsole;

/// <summary>
/// Description : Classe responsable de la gestion des menus et de la saisie utilisateur dans la console.
/// date : 2026-03-10
/// </summary>
internal class MenuManager
{
    private readonly HugoLandContext _context;
    private readonly GameDisplay _display;
    private readonly GameService _gameService;
    private readonly ArmyService _armyService;
    private readonly InstallationService _installationService;
    private readonly CombatService _combatService;

    public MenuManager(
        HugoLandContext context,
        GameDisplay display,
        GameService gameService,
        ArmyService armyService,
        InstallationService installationService,
        CombatService combatService)
    {
        _context = context;
        _display = display;
        _gameService = gameService;
        _armyService = armyService;
        _installationService = installationService;
        _combatService = combatService;
    }

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

    private void StartNewGame()
    {
    }

    private void LoadGame()
    {
    }

    private void PlayGame(Game game)
    {
    }
}

