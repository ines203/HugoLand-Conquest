using HugoLand.Core.Services;
using HugoLandCore.Data;
using HugoLandCore.Models;
using HugoLandCore.Services;
using Microsoft.EntityFrameworkCore;

namespace HugoLandConsole;

/// <summary>
/// Description : Point d'entrée de l'application console pour HugoLand.
/// Date : 2026-03-10
/// </summary>
internal class Program
{
    static void Main(string[] args)
    {
        var options = new DbContextOptionsBuilder<HugoLandContext>()
            .UseInMemoryDatabase("HugoLandDB")
            .Options;

        var context = new HugoLandContext(options);

        // Vider et recréer la BD pour s'assurer que le seed est appliqué proprement
        context.Database.EnsureDeleted();
        context.Database.EnsureCreated();

        // Services
        var installationService = new InstallationService(context);
        var economyService = new EconomyService(context);
        var combatService = new CombatService(context, installationService);
        var armyService = new ArmyService(context, combatService, installationService);
        combatService.SetArmyService(armyService);
        var display = new GameDisplay(context);

        // Menu manager
        var menu = new MenuManager(
            context,
            display,
            armyService,
            installationService,
            combatService,
            economyService
        );

        // Lancer le jeu
        menu.Start();
    }
}